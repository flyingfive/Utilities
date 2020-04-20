using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace FlyingFive.DynamicProxy
{
    /// <summary>
    /// 默认接口代理实现点
    /// </summary>
    public abstract class DefaultInvocationHandler : IProxyInvocationHandler
    {
        private Type[] _interceptorTypes = Type.EmptyTypes;
        public DefaultInvocationHandler()
        {
        }

        internal void SetInterceptoryType(Type[] types)
        {
            _interceptorTypes = types;
            //todo:设置_interceptorTypes字段
        }

        /// <summary>
        /// 代理(远程)调用
        /// </summary>
        /// <param name="proxy">代理对象</param>
        /// <param name="method">代理的方法</param>
        /// <param name="parameters">调用方法的参数信息</param>
        /// <returns></returns>
        public virtual object Invoke(Object proxy, MethodInfo method, Object[] parameters)
        {
            var interfaceType = method.DeclaringType;
            var executionContext = new ProxyExecutionContext() { Proxy = proxy, Method = method, Arguments = parameters };
            //在自定义IOC容器中查找接口实现并调用返回。
            PreProceed(executionContext);
            PerformProceed(executionContext);
            PostProceed(executionContext);
            return executionContext.ReturnValue;
        }

        /// <summary>
        /// 调用前
        /// </summary>
        /// <param name="context"></param>
        public virtual void PreProceed(ProxyExecutionContext context) { }
        /// <summary>
        /// 调用后
        /// </summary>
        /// <param name="context"></param>
        public virtual void PostProceed(ProxyExecutionContext context) { }
        /// <summary>
        /// 执行调用
        /// </summary>
        /// <param name="context"></param>
        public abstract void PerformProceed(ProxyExecutionContext context);
    }

    /// <summary>
    /// 代理执行上下文环境
    /// </summary>
    public class ProxyExecutionContext
    {
        public object Proxy { get; internal set; }
        public MethodInfo Method { get; internal set; }
        public Object[] Arguments { get; internal set; }
        public object ReturnValue { get; set; }
    }
}
