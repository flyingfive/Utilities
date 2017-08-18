using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Caching
{
    /// <summary>
    /// 缓存器扩展方法
    /// </summary>
    public static class CacheExtensions
    {
        /// <summary>
        /// 尝试从缓存中获取,如果获取不到则调用方法并默认缓存1小时
        /// </summary>
        /// <typeparam name="T">要获取的数据类型</typeparam>
        /// <param name="cacheManager">缓存管理器</param>
        /// <param name="key">缓存键</param>
        /// <param name="acquire">获取方法</param>
        /// <returns></returns>
        public static T TryGet<T>(this ICacheManager cacheManager, string key, Func<T> acquire)
        {
            return TryGet(cacheManager, key, 60, acquire);
        }

        /// <summary>
        /// 尝试从缓存中获取,如果获取不到则调用方法并缓存指定时间
        /// </summary>
        /// <typeparam name="T">要获取的数据类型</typeparam>
        /// <param name="cacheManager">缓存管理器</param>
        /// <param name="key">缓存键</param>
        /// <param name="cacheTime">缓存时长[单位:分钟]</param>
        /// <param name="acquire">获取方法</param>
        /// <returns></returns>
        public static T TryGet<T>(this ICacheManager cacheManager, string key, int cacheTime, Func<T> acquire)
        {
            if (cacheManager.IsSet(key))
            {
                return cacheManager.Get<T>(key);
            }
            else
            {
                var result = acquire();
                cacheManager.Set(key, result, cacheTime);
                return result;
            }
        }
    }
}
