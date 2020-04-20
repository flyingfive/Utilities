using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Globalization;
using System.Threading;

namespace FlyingFive.DynamicProxy
{
    /// <summary>
    /// 接口动态代理工厂
    /// </summary>
    public class ProxyObjectFactory
    {
        /// <summary>
        /// 代理类型中IProxyInvocationHandler类型成员变量handler的名称
        /// </summary>
        private const string HANDLER_NAME = "_handler";
        /// <summary>
        /// 代理类名称后缀
        /// </summary>
        private const string PROXY_SUFFIX = "Proxy";
        /// <summary>
        /// 已构建好的代理类型
        /// </summary>
        private static ConcurrentDictionary<string, Type> _cachedProxyType = null;
        private static readonly Hashtable _opCodeTypeMapper = new Hashtable();
        private static readonly MethodInfo _handlerInvokeMethod = typeof(IProxyInvocationHandler).GetMethod("Invoke", new Type[] { typeof(Object), typeof(MethodInfo), typeof(object[]) });
        static ProxyObjectFactory()
        {
            _cachedProxyType = new ConcurrentDictionary<string, Type>();
            _opCodeTypeMapper = new Hashtable();
            _opCodeTypeMapper.Add(typeof(System.Boolean), OpCodes.Ldind_I1);
            _opCodeTypeMapper.Add(typeof(System.Int16), OpCodes.Ldind_I2);
            _opCodeTypeMapper.Add(typeof(System.Int32), OpCodes.Ldind_I4);
            _opCodeTypeMapper.Add(typeof(System.Int64), OpCodes.Ldind_I8);
            _opCodeTypeMapper.Add(typeof(System.Double), OpCodes.Ldind_R8);
            _opCodeTypeMapper.Add(typeof(System.Single), OpCodes.Ldind_R4);
            _opCodeTypeMapper.Add(typeof(System.UInt16), OpCodes.Ldind_U2);
            _opCodeTypeMapper.Add(typeof(System.UInt32), OpCodes.Ldind_U4);
        }

        private static Object _locker = new Object();

        private ProxyObjectFactory() { }

        /// <summary>
        /// 默认的代理工厂实例
        /// </summary>
        /// <returns></returns>
        public static ProxyObjectFactory Default
        {
            get
            {
                if (Singleton<ProxyObjectFactory>.Instance == null)
                {
                    lock (_locker)
                    {
                        if (Singleton<ProxyObjectFactory>.Instance == null)
                        {
                            Singleton<ProxyObjectFactory>.Instance = new ProxyObjectFactory();
                        }
                    }
                }
                return Singleton<ProxyObjectFactory>.Instance;
            }
        }

        private static readonly Dictionary<Assembly, ModuleBuilder> _moduleBuilders = new Dictionary<Assembly, ModuleBuilder>();
        private const string DYNAMIC_ASSEMBLY_NAME = "__FlyingFive.LocalDynamicProxies";

        private Type _invocationHandlerType = null;

        public bool HasInitiated { get { return _invocationHandlerType != null; } }

        /// <summary>
        /// 配置IProxyInvocationHandler实现
        /// </summary>
        /// <param name="type"></param>
        public void SetupHandlerType(Type type)
        {
            if (type == null) { throw new ArgumentException("参数type不能为null。"); }
            if (HasInitiated) { throw new InvalidOperationException("已经完成初始化。"); }
            if (type.IsAssignableFrom(typeof(IProxyInvocationHandler)))
            {
                throw new NotImplementedException(string.Format("类型没有实现：{0}接口", typeof(IProxyInvocationHandler).FullName));
            }
            if (type.BaseType != typeof(DefaultInvocationHandler))
            {
                throw new ArgumentNullException(string.Format("代理调用类型必需继承父类：{0}", typeof(DefaultInvocationHandler).FullName));
            }
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new InvalidOperationException("代理调用类型IProxyInvocationHandler的实现必需具备默认的空参构造。");
            }
            var orginalType = Interlocked.CompareExchange<Type>(ref _invocationHandlerType, type, null);
            if (orginalType != null)
            {
                //todo:是否要抛出异常。
            }
        }

        /// <summary>
        /// 创建一个接口的本地代理实例
        /// </summary>
        /// <typeparam name="T">创建代理的接口类型</typeparam>
        /// <returns></returns>
        public T CreateInterfaceProxyWithoutTarget<T>(params Type[] interceptorTypes) where T : class
        {
            var interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
            {
                throw new ApplicationException("只能创建接口类型的代理!");
            }
            if (!HasInitiated) { throw new InvalidOperationException("还没有初始化IProxyInvocationHandler代理调用类型，请先使用SetupHandlerType方法进行初始化。"); }
            var handler = Activator.CreateInstance(_invocationHandlerType) as IProxyInvocationHandler;
            var implementationClassName = string.Format("{0}.{1}{2}", DYNAMIC_ASSEMBLY_NAME, interfaceType.FullName, PROXY_SUFFIX);
            var proxyType = ResolveLocalProxyType(implementationClassName, interfaceType);

            //创建接口的代理实例（使用代理类型上有一个IProxyInvocationHandler类型参数的构造方法进行创建）
            T proxyObj = Activator.CreateInstance(proxyType, new object[] { handler }) as T;
            return proxyObj;
        }

        /// <summary>
        /// 构造接口的本地代理实现类
        /// </summary>
        /// <param name="implementationClassName">实现类名称</param>
        /// <param name="interfaceType">接口类型</param>
        /// <returns></returns>
        internal static Type BuildImplementationProxyType(string implementationClassName, Type interfaceType)
        {
            ModuleBuilder dynamicModuleBuilder = null;
            var assembly = typeof(IProxyInvocationHandler).Assembly;
            if (!_moduleBuilders.TryGetValue(assembly, out dynamicModuleBuilder))
            {
                lock (assembly)
                {
                    if (!_moduleBuilders.TryGetValue(assembly, out dynamicModuleBuilder))
                    {
                        var assemblyName = new AssemblyName(String.Format(CultureInfo.InvariantCulture, "{0}.{1}", DYNAMIC_ASSEMBLY_NAME, assembly.FullName));
                        assemblyName.Version = new Version(1, 0, 0, 0);

                        var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                        dynamicModuleBuilder = assemblyBuilder.DefineDynamicModule("DynamicProxyModule");
                        _moduleBuilders.Add(assembly, dynamicModuleBuilder);
                    }
                }
            }
            var baseType = typeof(Object);
            var typeBuilder = dynamicModuleBuilder.DefineType(implementationClassName, TypeAttributes.Class | TypeAttributes.Public, baseType, new Type[] { interfaceType });

            var handlerType = typeof(IProxyInvocationHandler);
            var handlerField = typeBuilder.DefineField(HANDLER_NAME, handlerType, FieldAttributes.Private);

            var baseCtor = baseType.GetConstructor(new Type[0]);
            var delegateConstructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { handlerType });
            var constructorIL = delegateConstructor.GetILGenerator();

            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_1);
            constructorIL.Emit(OpCodes.Stfld, handlerField);
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Call, baseCtor);
            constructorIL.Emit(OpCodes.Ret);

            GenerateMethod(interfaceType, handlerField, typeBuilder);
            var proxyType = typeBuilder.CreateType();
            return proxyType;
        }

        private static void GenerateMethod(Type interfaceType, FieldBuilder handlerField, TypeBuilder typeBuilder)
        {
            MetaDataFactory.Add(interfaceType);
            var interfaceMethods = interfaceType.GetMethods();
            var i = 0;
            foreach (var methodInfo in interfaceMethods)
            {
                var methodArguments = methodInfo.GetParameters();
                int argumentCount = methodArguments.Length;
                var argumentTypes = methodArguments.Select(a => a.ParameterType).ToArray();

                var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard, methodInfo.ReturnType, argumentTypes);

                #region 方法实现
                var iLGenerator = methodBuilder.GetILGenerator();

                //定义方法返回类型局部变量
                if (!methodInfo.ReturnType.Equals(typeof(void)))
                {
                    iLGenerator.DeclareLocal(methodInfo.ReturnType);
                    if (methodInfo.ReturnType.IsValueType && !methodInfo.ReturnType.IsPrimitive)
                    {
                        iLGenerator.DeclareLocal(methodInfo.ReturnType);
                    }
                }

                //方法存在参数的话就定义一个object[]变量存放外部传递的参数值
                if (argumentCount > 0)
                {
                    iLGenerator.DeclareLocal(typeof(System.Object[]));
                }

                //声明一个标签用来定义调用handler的方法
                var handlerLabel = iLGenerator.DefineLabel();
                //声明一个标签定义方法返回 
                var returnLabel = iLGenerator.DefineLabel();

                //加载当前计算堆栈
                iLGenerator.Emit(OpCodes.Ldarg_0);
                //加载IProxyInvocationHandler类型成员变量handler
                iLGenerator.Emit(OpCodes.Ldfld, handlerField);
                //如果成员变量handler不为null，则跳转
                iLGenerator.Emit(OpCodes.Brtrue_S, handlerLabel);
                //handler为null的时候，当方法存在返回值则返回null，否则直接返回
                if (!methodInfo.ReturnType.Equals(typeof(void)))
                {
                    if (methodInfo.ReturnType.IsValueType && !methodInfo.ReturnType.IsPrimitive && !methodInfo.ReturnType.IsEnum)
                    {
                        iLGenerator.Emit(OpCodes.Ldloc_1);
                    }
                    else
                    {
                        //空引用加载到计算堆栈
                        iLGenerator.Emit(OpCodes.Ldnull);
                    }
                    //将计算堆栈上的（空引用）存储到第一个方法局部变量（第一个声明的局部变量为返回值）
                    iLGenerator.Emit(OpCodes.Stloc_0);
                    //跳到返回语句，方法到这里直接返回
                    iLGenerator.Emit(OpCodes.Br_S, returnLabel);
                }

                //handler不为null，继续下面执行
                iLGenerator.MarkLabel(handlerLabel);

                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, handlerField);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                //准备调用MetaDataFactory.GetMethod方法的第一个参数
                iLGenerator.Emit(OpCodes.Ldstr, interfaceType.FullName);
                //准备调用MetaDataFactory.GetMethod方法的第二个参数
                iLGenerator.Emit(OpCodes.Ldc_I4, i);
                //调用方法获取到要被代理调用的接口方法信息
                iLGenerator.Emit(OpCodes.Call, MetaDataFactory.GetInterfaceMethod);

                //加载参数列表个数到计算堆栈
                iLGenerator.Emit(OpCodes.Ldc_I4, argumentCount);
                //new一个object[]数组，长度为上面加载的参数列表个数
                iLGenerator.Emit(OpCodes.Newarr, typeof(System.Object));

                //将传递的参数按顺序存入刚才new出来的object[]数组变量，值类型将进行装箱
                if (argumentCount > 0)
                {
                    iLGenerator.Emit(OpCodes.Stloc_1);
                    for (int j = 0; j < argumentCount; j++)
                    {
                        iLGenerator.Emit(OpCodes.Ldloc_1);
                        iLGenerator.Emit(OpCodes.Ldc_I4, j);
                        iLGenerator.Emit(OpCodes.Ldarg, j + 1);
                        if (argumentTypes[j].IsValueType)
                        {
                            iLGenerator.Emit(OpCodes.Box, argumentTypes[j]);
                        }
                        iLGenerator.Emit(OpCodes.Stelem_Ref);
                    }
                    iLGenerator.Emit(OpCodes.Ldloc_1);
                }

                //调用IProxyInvocationHandler类型handler实例上的Invoke方法
                iLGenerator.Emit(OpCodes.Callvirt, _handlerInvokeMethod);

                if (!methodInfo.ReturnType.Equals(typeof(void)))
                {
                    //如果返回值为值类型则进行拆箱
                    if (methodInfo.ReturnType.IsValueType)
                    {
                        iLGenerator.Emit(OpCodes.Unbox, methodInfo.ReturnType);
                        if (methodInfo.ReturnType.IsEnum)
                        {
                            iLGenerator.Emit(OpCodes.Ldind_I4);
                        }
                        else if (!methodInfo.ReturnType.IsPrimitive)
                        {
                            iLGenerator.Emit(OpCodes.Ldobj, methodInfo.ReturnType);
                        }
                        else
                        {
                            iLGenerator.Emit((OpCode)_opCodeTypeMapper[methodInfo.ReturnType]);
                        }
                    }

                    //将返回值存储到第一个位置的局部变量
                    iLGenerator.Emit(OpCodes.Stloc_0);
                    //跳转到返回标签
                    iLGenerator.Emit(OpCodes.Br_S, returnLabel);
                    //标记返回语句
                    iLGenerator.MarkLabel(returnLabel);
                    //加载返回值变量（第一个局部变量）到计算堆栈上
                    iLGenerator.Emit(OpCodes.Ldloc_0);
                }
                else
                {
                    //空返回时移除顶部堆栈的值
                    iLGenerator.Emit(OpCodes.Pop);
                    iLGenerator.MarkLabel(returnLabel);
                }

                //方法返回，结束
                iLGenerator.Emit(OpCodes.Ret);
                #endregion
                i++;
            }

            //如果接口上还有接口，迭代创建其它接口方法的代理
            foreach (Type parentType in interfaceType.GetInterfaces())
            {
                GenerateMethod(parentType, handlerField, typeBuilder);
            }
        }

        /// <summary>
        /// 解析获取接口的本地代理类型
        /// </summary>
        /// <param name="typeName">代理类型名称</param>
        /// <param name="interfaceType">代理的接口类型</param>
        /// <returns></returns>
        private static Type ResolveLocalProxyType(string typeName, Type interfaceType)
        {
            Type proxyType = null;
            if (!_cachedProxyType.TryGetValue(typeName, out proxyType))
            {
                lock (_locker)
                {
                    if (!_cachedProxyType.TryGetValue(typeName, out proxyType))
                    {
                        proxyType = BuildImplementationProxyType(typeName, interfaceType);
                        _cachedProxyType.GetOrAdd(typeName, proxyType);
                        return proxyType;
                    }
                }
            }
            return proxyType;
        }
    }
}
