using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.DynamicProxy
{
    /// <summary>
    /// 表示代理调用实现的处理点
    /// </summary>
    public interface IProxyInvocationHandler
    {
        /// <summary>
        /// 代理调用
        /// </summary>
        /// <param name="proxy">代理对象</param>
        /// <param name="method">代理方法</param>
        /// <param name="parameters">调用的方法参数信息</param>
        /// <returns></returns>
        Object Invoke(Object proxy, MethodInfo method, Object[] parameters);
    }

    /// <summary>
    /// 代理执行的拦截器
    /// </summary>
    public interface IProxyExecutionInterceptor
    {
        /// <summary>
        /// 执行处理前
        /// </summary>
        /// <param name="executionContext">执行上下文</param>
        void PreProceed(ProxyExecutionContext executionContext);
        /// <summary>
        /// 执行处理后
        /// </summary>
        /// <param name="executionContext">执行上下文</param>
        void PostProceed(ProxyExecutionContext executionContext);
    }
}
