using FlyingFive.Data.Emit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace FlyingFive.Data.Mapper
{
    /// <summary>
    /// 表示成员映射器
    /// </summary>
    public interface IMemberMapper
    {
        /// <summary>
        /// 根据映射从DataReader赋值给实体对象的属性
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        void Map(object instance, IDataReader reader, int ordinal);
    }

    public static class MemberMapperHelper
    {
        private static int _sequenceNumber = 0;
        private static readonly Dictionary<Assembly, ModuleBuilder> _moduleBuilders = new Dictionary<Assembly, ModuleBuilder>();
        private static readonly ConcurrentDictionary<MemberInfo, Type> _typeCache = new System.Collections.Concurrent.ConcurrentDictionary<MemberInfo, Type>();

        /// <summary>
        /// 创建成员映射器对象
        /// </summary>
        /// <param name="member">成员对象</param>
        /// <returns></returns>
        public static IMemberMapper CreateMemberMapper(MemberInfo member)
        {
            Type type = CreateMemberMapperType(member);
            var mapper = Activator.CreateInstance(type) as IMemberMapper;
            return mapper;
        }


        /// <summary>
        /// 创建成员映射器的具体类型
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Type CreateMemberMapperType(MemberInfo member)
        {
            Type mapperType = null;
            if (member == null) { throw new ArgumentNullException("参数：member不能为null"); }
            if (_typeCache.TryGetValue(member, out mapperType))
            {
                return mapperType;
            }
            var instanceType = member.DeclaringType;
            var assembly = typeof(IMemberMapper).Assembly;
            ModuleBuilder dynamicModuleBuilder;
            if (!_moduleBuilders.TryGetValue(assembly, out dynamicModuleBuilder))
            {
                lock (assembly)
                {
                    if (!_moduleBuilders.TryGetValue(assembly, out dynamicModuleBuilder))
                    {
                        var assemblyName = new AssemblyName(String.Format(CultureInfo.InvariantCulture, "DynamicMemberMappers.{0}", assembly.FullName));
                        assemblyName.Version = new Version(1, 0, 0, 0);
                        var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                        dynamicModuleBuilder = assemblyBuilder.DefineDynamicModule("DynamicMemberMapperModule");
                        _moduleBuilders.Add(assembly, dynamicModuleBuilder);
                    }
                }
            }
            var typeName = string.Format("DynamicMemberMappers.{0}_{1}_{2}{3}", instanceType.FullName, member.Name, Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper(), Interlocked.Increment(ref _sequenceNumber).ToString());
            TypeAttributes typeAttributes = TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed;
            var typeBuilder = dynamicModuleBuilder.DefineType(typeName, typeAttributes, null, new Type[] { typeof(IMemberMapper) });
            //实现IMemberMapper.Map方法
            var methodBuilder = typeBuilder.DefineMethod("Map", MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.HasThis, typeof(void), new Type[] { typeof(object), typeof(IDataReader), typeof(Int32) });
            var il = methodBuilder.GetILGenerator();
            var parameterStartIndex = 1;
            il.Emit(OpCodes.Ldarg_S, parameterStartIndex);          //加载第一个参数instance
            il.Emit(OpCodes.Castclass, instanceType);
            var getDataMethod = Extensions.DataReaderMethods.GetReaderMethod(member.GetMemberType());

            il.Emit(OpCodes.Ldarg_S, parameterStartIndex + 1);      //加载第二个参数dataReader
            il.Emit(OpCodes.Ldarg_S, parameterStartIndex + 2);      //加载第三个参数ordinal
            il.EmitCall(OpCodes.Call, getDataMethod, null);         //调用DataReaderMethods中定义的对应获取数据的方法，调用后的栈顶为该方法的返回值
            SetValueIL(il, member);                                 //将栈顶的返回值通过IL赋值给实例成员
            il.Emit(OpCodes.Ret);                                   //方法结束返回
            mapperType = typeBuilder.CreateType();
            _typeCache.GetOrAdd(member, mapperType);
            return mapperType;
        }

        /// <summary>
        /// 使用MSIL指令将当前堆栈上的值赋给成员对象
        /// </summary>
        /// <param name="il">MSIL指令</param>
        /// <param name="member">成员对象(属性或字段)</param>
        private static void SetValueIL(ILGenerator il, MemberInfo member)
        {
            MemberTypes memberType = member.MemberType;
            if (memberType == MemberTypes.Property)
            {
                MethodInfo setter = ((PropertyInfo)member).GetSetMethod();
                il.EmitCall(OpCodes.Callvirt, setter, null);//给属性赋值
            }
            else if (memberType == MemberTypes.Field)
            {
                il.Emit(OpCodes.Stfld, ((FieldInfo)member));//给字段赋值
            }
            else
            {
                throw new NotSupportedException("不支持字段或属性外的其它成员操作");
            }
        }
    }
}
