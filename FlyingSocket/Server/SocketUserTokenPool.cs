using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingSocket.Server
{
    /// <summary>
    /// (LIFO规则的)Socket客户端连接对象池
    /// </summary>
    public class SocketUserTokenPool
    {
        private Stack<SocketUserToken> _dataPool = null;

        public SocketUserTokenPool(int capacity)
        {
            _dataPool = new Stack<SocketUserToken>(capacity);
        }

        public void Push(SocketUserToken item)
        {
            if (item == null)
            {
                throw new ArgumentException("Items added to a AsyncSocketUserToken cannot be null");
            }
            lock (_dataPool)
            {
                item.SessionId = string.Empty;
                item.ClientId = string.Empty;
                item.Token = string.Empty;
                item.ConnectedTime = DateTime.MinValue;
                item.ActiveTime = DateTime.MinValue;
                _dataPool.Push(item);
            }
        }

        public SocketUserToken Pop()
        {
            lock (_dataPool)
            {
                if (_dataPool.Count == 0) { return null; }
                var item = _dataPool.Pop();
                item.SessionId = Guid.NewGuid().ToString("N");
                return item;
            }
        }

        public int Count
        {
            get { return _dataPool.Count; }
        }
    }

    /// <summary>
    /// Socket客户端连接对象集合
    /// </summary>
    public class SocketUserTokenList
    {
        private List<SocketUserToken> _innerList = null;

        public SocketUserTokenList()
        {
            _innerList = new List<SocketUserToken>();
        }

        public void Add(SocketUserToken userToken)
        {
            lock (_innerList)
            {
                _innerList.Add(userToken);
            }
        }

        public void Remove(SocketUserToken userToken)
        {
            lock (_innerList)
            {
                _innerList.Remove(userToken);
            }
        }

        public void CopyList(ref SocketUserToken[] array)
        {
            lock (_innerList)
            {
                array = new SocketUserToken[_innerList.Count];
                _innerList.CopyTo(array);
            }
        }
    }
}
