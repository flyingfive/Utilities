using FlyingFive.Data.Mapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace FlyingFive.Data.Emit
{
    /// <summary>
    /// 动态类生成工具
    /// </summary>
    public static class DynamicClassGenerator
    {
        private static int _sequenceNumber = 0;
        private static readonly Dictionary<Assembly, ModuleBuilder> _moduleBuilders = new Dictionary<Assembly, ModuleBuilder>();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<MemberInfo, Type> _typeCache = new System.Collections.Concurrent.ConcurrentDictionary<MemberInfo, Type>();


        /// <summary>
        /// 创建一个类型成员的映射器类型(IMemberMapper接口的实现类型)
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Type CreateMemberMapperClass(MemberInfo member)
        {
            Type mapperType = null;
            if (_typeCache.TryGetValue(member, out mapperType))
            {
                return mapperType;
            }

            var entityType = member.DeclaringType;
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

            TypeAttributes typeAttributes = TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed;
            TypeBuilder tb = dynamicModuleBuilder.DefineType(
                string.Format("DynamicMemberMappers.{0}_{1}_{2}{3}", entityType.FullName, member.Name, Guid.NewGuid().ToString("N").Substring(0, 5), Interlocked.Increment(ref _sequenceNumber).ToString())
                , typeAttributes, null, new Type[] { typeof(IMemberMapper) });

            tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName);
            //实现IMemberMapper.Map方法
            var methodBuilder = tb.DefineMethod("Map", MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.HasThis, typeof(void), new Type[] { typeof(object), typeof(IDataReader), typeof(int) });
            ILGenerator il = methodBuilder.GetILGenerator();
            
            int parameStartIndex = 1;

            il.Emit(OpCodes.Ldarg_S, parameStartIndex);//将第一个参数 object 对象加载到栈顶
            il.Emit(OpCodes.Castclass, member.DeclaringType);//将 object 对象转换为强类型对象 此时栈顶为强类型的对象

            var readerMethod = Extensions.DataReaderMethods.GetReaderMethod(member.GetMemberType());

            //ordinal
            il.Emit(OpCodes.Ldarg_S, parameStartIndex + 1);    //加载参数DataReader
            il.Emit(OpCodes.Ldarg_S, parameStartIndex + 2);    //加载参数 readOrdinal
            il.EmitCall(OpCodes.Call, readerMethod, null);     //调用对应的 DataReader方法 得到 value  此时栈顶为 从DataReader中取出的值

            EmitHelper.SetValueIL(il, member);                 // 成员赋值; 此时栈顶为空

            il.Emit(OpCodes.Ret);                              //方法返回
            mapperType = tb.CreateType();
            _typeCache.GetOrAdd(member, mapperType);

            return mapperType;
        }

        /// <summary>
        /// Throws a data exception, only used internally
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="index"></param>
        /// <param name="reader"></param>
        private static void ThrowDataException(Exception ex, int index, IDataReader reader)
        {
            string name = "(n/a)", value = "(n/a)";
            if (reader != null && index >= 0 && index < reader.FieldCount)
            {
                name = reader.GetName(index);
                object val = reader.GetValue(index);
                if (val == null || val is DBNull)
                {
                    value = "<null>";
                }
                else
                {
                    value = Convert.ToString(val) + " - " + Type.GetTypeCode(val.GetType());
                }
            }
            throw new DataException(string.Format("Error parsing column {0} ({1}={2})", index, name, value), ex);
        }
    }
}
