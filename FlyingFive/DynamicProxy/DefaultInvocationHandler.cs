using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.DynamicProxy
{
    /// <summary>
    /// 默认接口代理实现点
    /// </summary>
    public class DefaultInvocationHandler : IProxyInvocationHandler
    {
        public DefaultInvocationHandler()
        {
        }

        /// <summary>
        /// 代理(远程)调用
        /// </summary>
        /// <param name="proxy">代理对象</param>
        /// <param name="method">代理的方法</param>
        /// <param name="parameters">调用方法的参数信息</param>
        /// <returns></returns>
        public Object Invoke(Object proxy, MethodInfo method, Object[] parameters)
        {
            var interfaceType = method.DeclaringType;
            var implementType = Type.EmptyTypes.FirstOrDefault();
            //todo:查找接口上定义的拦截器:interfaceType.GetCustomAttribute<T>
            //在自定义IOC容器中查找接口实现并调用返回。
            //todo:begin invoke...
            throw new NotImplementedException("未实现。");
            //todo:after invoke...
        }
    }
}
