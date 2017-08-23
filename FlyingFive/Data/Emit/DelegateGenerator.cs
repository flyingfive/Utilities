using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data.Emit
{
    /// <summary>
    /// 委托生成工具
    /// </summary>
    public static class DelegateGenerator
    {
        ///// <summary>
        ///// 创建对象生成器
        ///// </summary>
        ///// <param name="constructor">对象构造函数</param>
        ///// <returns></returns>
        //public static Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> CreateObjectGenerator(ConstructorInfo constructor)
        //{
        //    Utils.CheckNull(constructor);

        //    Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> ret = null;

        //    var pExp_reader = Expression.Parameter(typeof(IDataReader), "reader");
        //    var pExp_readerOrdinalEnumerator = Expression.Parameter(typeof(ReaderOrdinalEnumerator), "readerOrdinalEnumerator");
        //    var pExp_objectActivatorEnumerator = Expression.Parameter(typeof(ObjectActivatorEnumerator), "objectActivatorEnumerator");

        //    ParameterInfo[] parameters = constructor.GetParameters();
        //    List<Expression> arguments = new List<Expression>(parameters.Length);

        //    foreach (ParameterInfo parameter in parameters)
        //    {
        //        if (MappingTypeSystem.IsMappingType(parameter.ParameterType))
        //        {
        //            var readerMethod = DataReaderConstant.GetReaderMethod(parameter.ParameterType);
        //            //int ordinal = readerOrdinalEnumerator.Next();
        //            var readerOrdinal = Expression.Call(pExp_readerOrdinalEnumerator, ReaderOrdinalEnumerator.NextMethodInfo);
        //            //DataReaderExtensions.GetValue(reader,readerOrdinal)
        //            var getValue = Expression.Call(readerMethod, pExp_reader, readerOrdinal);
        //            arguments.Add(getValue);
        //        }
        //        else
        //        {
        //            //IObjectActivator oa = objectActivatorEnumerator.Next();
        //            var oa = Expression.Call(pExp_objectActivatorEnumerator, ObjectActivatorEnumerator.NextMethodInfo);
        //            //object obj = oa.CreateInstance(IDataReader reader);
        //            var entity = Expression.Call(oa, typeof(IObjectActivator).GetMethod("CreateInstance"), pExp_reader);
        //            //(T)entity;
        //            var val = Expression.Convert(entity, parameter.ParameterType);
        //            arguments.Add(val);
        //        }
        //    }

        //    var body = Expression.New(constructor, arguments);

        //    ret = Expression.Lambda<Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object>>(body, pExp_reader, pExp_readerOrdinalEnumerator, pExp_objectActivatorEnumerator).Compile();

        //    return ret;
        //}

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
    }
}
