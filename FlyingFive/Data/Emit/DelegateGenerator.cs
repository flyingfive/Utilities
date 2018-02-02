using FlyingFive.Data.Infrastructure;
using FlyingFive.Data.Mapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Emit
{
    /// <summary>
    /// 委托生成工具
    /// </summary>
    public static class DelegateGenerator
    {
        /// <summary>
        /// 根据实体的构造函数创建实体对象的生成器
        /// </summary>
        /// <param name="constructor">对象构造函数</param>
        /// <returns></returns>
        public static Func<IDataReader, DataReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> CreateObjectGenerator(ConstructorInfo constructor)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException("参数：constructor不能为NULL.");
            }
            //声明一个DataReader参数
            var paraReaderExp = Expression.Parameter(typeof(IDataReader), "dataReader");
            //声明一个DataReader序列枚举器参数
            var paraReaderOridinalEnumeratorExp = Expression.Parameter(typeof(DataReaderOrdinalEnumerator), "readerOrdinal");
            //声明一个对象激活序列枚举器参数
            var paraActivatorOrdinalEnumeratorExp = Expression.Parameter(typeof(ObjectActivatorEnumerator), "activatorOrdinal");
            //构造器参参数列表
            var parameters = constructor.GetParameters();
            var arguments = new List<Expression>();
            foreach (var item in parameters)
            {
                if (SupportedMappingTypes.IsMappingType(item.ParameterType))
                {
                    var readerMethod = Extensions.DataReaderMethods.GetReaderMethod(item.ParameterType);
                    //调用DataReaderOrdinalEnumerator的Next方法,此时返回了DataReader的下一个读取序列
                    var readerOrdinal = Expression.Call(paraReaderOridinalEnumeratorExp, DataReaderOrdinalEnumerator.MethodOfNext);
                    //调用DataReaderMethods对应的方法,从DataReader中读取数据
                    var getValue = Expression.Call(readerMethod, paraReaderExp, readerOrdinal);
                    arguments.Add(getValue);
                }
                else
                {
                    //获取下一索引位置的对象激活器
                    var nextObjectActivatorExp = Expression.Call(paraActivatorOrdinalEnumeratorExp, ObjectActivatorEnumerator.MethodOfNext);
                    var methodOfCreateInstance = typeof(IObjectActivator).GetMethod("CreateInstance");
                    //调用激活器的创建对象方法
                    var entityExp = Expression.Call(nextObjectActivatorExp, methodOfCreateInstance, paraReaderExp);
                    //将创建的对象转换成构造参数中的数据类型
                    var paramValue = Expression.Convert(entityExp, item.ParameterType);
                    arguments.Add(paramValue);
                }
            }
            var body = Expression.New(constructor, arguments);
            var ret = Expression.Lambda<Func<IDataReader, DataReaderOrdinalEnumerator, ObjectActivatorEnumerator, object>>(body, paraReaderExp, paraReaderOridinalEnumeratorExp, paraActivatorOrdinalEnumeratorExp).Compile();
            return ret;
        }

        /// <summary>
        /// 创建从DataReader获取指定类型数据的生成器
        /// </summary>
        /// <param name="dataType">要获取的数据类型</param>
        /// <returns></returns>
        public static Func<IDataReader, int, object> CreateMappingTypeGenerator(Type dataType)
        {
            var parameterReader = Expression.Parameter(typeof(IDataReader), "reader");
            var parameterOrdinal = Expression.Parameter(typeof(int), "ordinal");
            var readerMethod = Extensions.DataReaderMethods.GetReaderMethod(dataType);
            var getValue = Expression.Call(readerMethod, parameterReader, parameterOrdinal);
            var methodBody = Expression.Convert(getValue, typeof(object));
            Func<IDataReader, int, object> ret = Expression.Lambda<Func<IDataReader, int, object>>(methodBody, parameterReader, parameterOrdinal).Compile();
            return ret;
        }

        /// <summary>
        /// 创建成员的setter访问器
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static Action<object, object> CreateValueSetter(MemberInfo memberInfo)
        {
            if (memberInfo.MemberType != MemberTypes.Property || memberInfo.MemberType != MemberTypes.Field) { throw new NotImplementedException("不支持的操作!"); }
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null) { return CreateValueSetter(propertyInfo); }
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null) { return CreateValueSetter(fieldInfo); }
            throw new NotImplementedException("不支持的操作!");
        }

        public static Action<object, object> CreateValueSetter(PropertyInfo propertyInfo)
        {
            var p = Expression.Parameter(typeof(object), "instance");
            var pValue = Expression.Parameter(typeof(object), "value");
            var instance = Expression.Convert(p, propertyInfo.DeclaringType);
            var value = Expression.Convert(pValue, propertyInfo.PropertyType);
            var pro = Expression.Property(instance, propertyInfo);
            var setValue = Expression.Assign(pro, value);
            Expression body = setValue;
            var lambda = Expression.Lambda<Action<object, object>>(body, p, pValue);
            Action<object, object> ret = lambda.Compile();
            return ret;
        }

        public static Action<object, object> CreateValueSetter(FieldInfo fieldInfo)
        {
            var p = Expression.Parameter(typeof(object), "instance");
            var pValue = Expression.Parameter(typeof(object), "value");
            var instance = Expression.Convert(p, fieldInfo.DeclaringType);
            var value = Expression.Convert(pValue, fieldInfo.FieldType);

            var field = Expression.Field(instance, fieldInfo);
            var setValue = Expression.Assign(field, value);

            Expression body = setValue;

            var lambda = Expression.Lambda<Action<object, object>>(body, p, pValue);
            Action<object, object> ret = lambda.Compile();
            return ret;
        }

        /// <summary>
        /// 创建成员的getter访问器
        /// </summary>
        /// <param name="propertyOrField"></param>
        /// <returns></returns>
        public static Func<object, object> CreateValueGetter(MemberInfo propertyOrField)
        {
            var p = Expression.Parameter(typeof(object), "a");
            var instance = Expression.Convert(p, propertyOrField.DeclaringType);
            var memberAccess = Expression.MakeMemberAccess(instance, propertyOrField);
            Type type = propertyOrField.GetMemberType();
            Expression body = memberAccess;
            if (type.IsValueType)
            {
                body = Expression.Convert(memberAccess, typeof(object));
            }
            var lambda = Expression.Lambda<Func<object, object>>(body, p);
            Func<object, object> ret = lambda.Compile();
            return ret;
        }
    }
}
