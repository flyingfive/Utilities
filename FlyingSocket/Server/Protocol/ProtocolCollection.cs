using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyingSocket.Server.Protocol
{
    /// <summary>
    /// 协议对象全集
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProtocolCollection<T> where T : BaseSocketProtocol
    {
        private List<T> _innerList = null;

        public ProtocolCollection()
        {
            if (typeof(T).IsAbstract) { throw new NotSupportedException("不支持抽象类型。"); }
            _innerList = new List<T>();
        }

        public int Count() { return _innerList.Count(); }

        public T ElementAt(int index)
        {
            if (index < 0 || index >= _innerList.Count)
            {
                throw new IndexOutOfRangeException();
            }
            return _innerList.ElementAt(index);
        }

        public void Add(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
            _innerList.Add(value);
        }

        public void Remove(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
            _innerList.Remove(value);
        }
    }
}
