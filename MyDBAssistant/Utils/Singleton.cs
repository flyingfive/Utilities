using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDBAssistant.Utils
{
    /// <summary>
    /// 提供单例模式的统一实现
    /// </summary>
    public class Singleton<T> : Singleton
    {
        private static T instance;

        /// <summary>
        /// 泛型单例对象
        /// </summary>
        public static T Instance
        {
            get { return instance; }
            set
            {
                instance = value;
                AllSingletons[typeof(T)] = value;
            }
        }
    }

    /// <summary>
    /// 泛型集合单例实现
    /// </summary>
    /// <typeparam name="T">集合中的泛型类型</typeparam>
    public class SingletonList<T> : Singleton<IList<T>>
    {
        static SingletonList()
        {
            Singleton<IList<T>>.Instance = new List<T>();
        }

        /// <summary>
        /// 所有单例对象(泛型集合)
        /// </summary>
        public new static IList<T> Instance
        {
            get { return Singleton<IList<T>>.Instance; }
        }
    }

    /// <summary>
    /// 键值对字典的单例实现
    /// </summary>
    /// <typeparam name="TKey">字典键类型</typeparam>
    /// <typeparam name="TValue">字典值类型</typeparam>
    public class SingletonDictionary<TKey, TValue> : Singleton<IDictionary<TKey, TValue>>
    {
        static SingletonDictionary()
        {
            Singleton<Dictionary<TKey, TValue>>.Instance = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// 所有单例对象(字典)
        /// </summary>
        public new static IDictionary<TKey, TValue> Instance
        {
            get { return Singleton<Dictionary<TKey, TValue>>.Instance; }
        }
    }

    /// <summary>
    /// 底层单例
    /// </summary>
    public class Singleton
    {
        static Singleton()
        {
            allSingletons = new Dictionary<Type, object>();
        }

        private static readonly IDictionary<Type, object> allSingletons;

        /// <summary>
        /// 所有单例对象
        /// </summary>
        public static IDictionary<Type, object> AllSingletons
        {
            get { return allSingletons; }
        }
    }
}
