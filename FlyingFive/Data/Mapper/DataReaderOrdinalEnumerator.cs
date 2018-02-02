using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Mapper
{

    /// <summary>
    /// DataReader读取器序列的枚举器
    /// </summary>
    public class DataReaderOrdinalEnumerator
    {
        /// <summary>
        /// 将枚举器进行到下一个枚举索引的方法
        /// </summary>
        public static readonly MethodInfo MethodOfNext = null;

        static DataReaderOrdinalEnumerator()
        {
            MethodInfo method = typeof(DataReaderOrdinalEnumerator).GetMethod("Next");
            MethodOfNext = method;
        }

        private IList<int> _readerOrdinals = null;
        /// <summary>
        /// 当前顺序
        /// </summary>
        public int CurrentOrdinal { get; private set; }
        /// <summary>
        /// 下一个顺序
        /// </summary>
        private int _next = -1;

        public DataReaderOrdinalEnumerator(IList<int> readerOrdinals)
        {
            this._readerOrdinals = readerOrdinals;
            Reset();
        }

        /// <summary>
        /// 下一个枚举索引
        /// </summary>
        /// <returns></returns>
        public int Next()
        {
            this.CurrentOrdinal = _readerOrdinals[_next];
            this._next++;
            return this.CurrentOrdinal;
        }

        /// <summary>
        /// 重置枚举索引
        /// </summary>
        public void Reset()
        {
            this._next = 0;
            this.CurrentOrdinal = -1;
        }
    }

    /// <summary>
    /// 对象激活器枚举,当查询中返回匿名复合对象结果时使用
    /// </summary>
    public class ObjectActivatorEnumerator
    {
        /// <summary>
        /// 下一个对象激活器的方法
        /// </summary>
        public static readonly MethodInfo MethodOfNext = null;

        static ObjectActivatorEnumerator()
        {
            MethodInfo method = typeof(ObjectActivatorEnumerator).GetMethod("Next");
            MethodOfNext = method;
        }

        private IList<IObjectActivator> _objectActivators = null;
        private int _next = 0;


        public ObjectActivatorEnumerator(IList<IObjectActivator> objectActivators)
        {
            this._objectActivators = objectActivators;
            this._next = 0;
        }

        /// <summary>
        /// 下一个对象激活器
        /// </summary>
        /// <returns></returns>
        public IObjectActivator Next()
        {
            IObjectActivator ret = this._objectActivators[this._next];
            this._next++;
            return ret;
        }

        /// <summary>
        /// 重置枚举索引
        /// </summary>
        public void Reset()
        {
            this._next = 0;
        }
    }
}
