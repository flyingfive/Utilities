using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace FlyingFive.DynamicProxy
{
    /// <summary>
    /// 默认接口代理调用处理实现点
    /// </summary>
    public abstract class BaseInvocationHandler : IProxyInvocationHandler
    {
        private IProxyExecutionInterceptor[] _interceptors = null;
        public BaseInvocationHandler()
        {
        }

        /// <summary>
        /// 设置代理接口上的执行拦截器
        /// </summary>
        /// <param name="types"></param>
        internal void SetInterceptoryType(Type[] types)
        {
            var instances = new List<IProxyExecutionInterceptor>();
            foreach (var item in types)
            {
                if (!typeof(IProxyExecutionInterceptor).IsAssignableFrom(item))
                {
                    throw new InvalidOperationException(string.Format("类型{0}不是正确的代理执行拦截器类型", item.FullName));
                }
                if (!item.HasDefaultEmptyConstructor())
                {
                    throw new InvalidOperationException(string.Format("代理执行拦截器类型{0}不具备默认空参构造。", item.FullName));
                }
                instances.Add(Activator.CreateInstance(item) as IProxyExecutionInterceptor);
            }
            _interceptors = instances.ToArray();
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
            PreProceed(executionContext);
            BeforeInvoke(executionContext);
            PerformProceed(executionContext);
            AfterInvoke(executionContext);
            PostProceed(executionContext);
            return executionContext.ReturnValue;
        }

        private void AfterInvoke(ProxyExecutionContext executionContext)
        {
            if (_interceptors == null) { return; }
            foreach (var interceptor in _interceptors)
            {
                interceptor.PostProceed(executionContext);
            }
        }

        private void BeforeInvoke(ProxyExecutionContext executionContext)
        {
            if (_interceptors == null) { return; }
            foreach (var interceptor in _interceptors)
            {
                interceptor.PreProceed(executionContext);
            }
        }

        /// <summary>
        /// 调用前
        /// </summary>
        /// <param name="context"></param>
        protected virtual void PreProceed(ProxyExecutionContext context) { }
        /// <summary>
        /// 调用后
        /// </summary>
        /// <param name="context"></param>
        protected virtual void PostProceed(ProxyExecutionContext context) { }
        /// <summary>
        /// 执行调用
        /// </summary>
        /// <param name="context"></param>
        protected abstract void PerformProceed(ProxyExecutionContext context);
    }

    /// <summary>
    /// 代理执行上下文环境
    /// </summary>
    public class ProxyExecutionContext
    {
        /// <summary>
        /// 附加数据
        /// </summary>
        public Dictionary<string, object> Addition { get; set; }
        /// <summary>
        /// 代理对象
        /// </summary>
        public object Proxy { get; internal set; }
        /// <summary>
        /// 代理执行的接口方法
        /// </summary>
        public MethodInfo Method { get; internal set; }
        /// <summary>
        /// 代理执行接口方案的参数数据
        /// </summary>
        public Object[] Arguments { get; internal set; }
        /// <summary>
        /// 代理执行后的返回值
        /// </summary>
        public object ReturnValue { get; set; }

        public ProxyExecutionContext() { Addition = new Dictionary<string, object>(); }
    }
}
