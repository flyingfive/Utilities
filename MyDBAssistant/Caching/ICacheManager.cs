﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDBAssistant
{
    /// <summary>
    /// 表示缓存管理器
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// 获取一个缓存对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        T Get<T>(string key);
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存数据</param>
        /// <param name="cacheTime">缓存时间</param>
        void Set(string key, object data, int cacheTime);
        /// <summary>
        /// 移除一个缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        void Remove(string key);
        /// <summary>
        /// 根据正则批量移除缓存
        /// </summary>
        /// <param name="pattern">正则表达式</param>
        void RemoveByPattern(string pattern);
        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        bool IsSet(string key);
        /// <summary>
        /// 清空所有缓存
        /// </summary>
        void Clear();
    }
}
