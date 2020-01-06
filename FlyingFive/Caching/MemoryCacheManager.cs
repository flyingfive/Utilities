using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;

namespace FlyingFive.Caching
{
    /// <summary>
    /// 内存静态(模拟)缓存
    /// </summary>
    public class MemoryCacheManager : ICacheManager
    {
        public MemoryCacheManager()
        {
            this.Cache = new MemoryCache("_DefaultCache");
        }

        /// <summary>
        /// 缓存实例
        /// </summary>
        protected ObjectCache Cache { get; private set; }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">缓存对象类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key)) { throw new ArgumentNullException("key不能为null"); }
            return (T)Cache[key];
        }

        /// <summary>
        /// 设置长期缓存(没有有效期)
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">对象</param>
        public void Set(string key, object data)
        {
            if (string.IsNullOrEmpty(key)) { throw new ArgumentNullException("key不能为null"); }
            if (data == null) { return; }
            var policy = new CacheItemPolicy();
            Cache.Add(new CacheItem(key, data), policy);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">对象</param>
        /// <param name="expires">失效期（从本地当前时间开始计算）</param>
        public void Set(string key, object data, TimeSpan expires)
        {
            if (string.IsNullOrEmpty(key)) { throw new ArgumentNullException("key不能为null"); }
            if (data == null) { return; }
            var policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now.Add(expires);
            Cache.Add(new CacheItem(key, data), policy);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key)) { throw new ArgumentNullException("key不能为null"); }
            Cache.Remove(key);
        }

        /// <summary>
        /// 批量移除缓存
        /// </summary>
        /// <param name="pattern">缓存键模式</param>
        public void RemoveByPattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) { throw new ArgumentNullException("pattern不能为null"); }
            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keysToRemove = new List<String>();

            foreach (var item in Cache)
                if (regex.IsMatch(item.Key))
                    keysToRemove.Add(item.Key);

            foreach (string key in keysToRemove)
            {
                Remove(key);
            }
        }

        /// <summary>
        /// 缓存键是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public bool IsSet(string key)
        {
            if (string.IsNullOrEmpty(key)) { throw new ArgumentNullException("key不能为null"); }
            return (Cache.Contains(key));
        }

        /// <summary>
        /// 清除全部缓存
        /// </summary>
        public void Clear()
        {
            foreach (var item in Cache)
            {
                Remove(item.Key);
            }
        }
    }
}
