using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Interception
{
    /// <summary>
    /// 全局DB拦截
    /// </summary>
    public static class GlobalDbInterception
    {
        static volatile List<IDbCommandInterceptor> _interceptors = new List<IDbCommandInterceptor>();
        static readonly object _lockObject = new object();

        /// <summary>
        /// 添加一个拦截器到全局
        /// </summary>
        /// <param name="interceptor"></param>
        public static void Add(IDbCommandInterceptor interceptor)
        {
            if (interceptor == null) { throw new ArgumentNullException("参数: interceptor不能为null"); }
            lock (_lockObject)
            {
                List<IDbCommandInterceptor> newList = _interceptors.ToList();
                newList.Add(interceptor);
                newList.TrimExcess();
                _interceptors = newList;
            }
        }

        /// <summary>
        /// 移除一个全局DB拦截器
        /// </summary>
        /// <param name="interceptor"></param>
        public static void Remove(IDbCommandInterceptor interceptor)
        {
            if (interceptor == null) { throw new ArgumentNullException("参数: interceptor不能为null"); }
            lock (_lockObject)
            {
                List<IDbCommandInterceptor> newList = _interceptors.ToList();
                newList.Remove(interceptor);
                newList.TrimExcess();
                _interceptors = newList;
            }
        }

        /// <summary>
        /// 获取启用的全局DB拦截器
        /// </summary>
        /// <returns></returns>
        public static IDbCommandInterceptor[] GetInterceptors()
        {
            return _interceptors.ToArray();
        }
    }
}
