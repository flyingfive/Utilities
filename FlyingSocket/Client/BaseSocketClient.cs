using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyingFive;
using FlyingSocket.Common;

namespace FlyingSocket.Client
{
    /// <summary>
    /// 遵守指定通讯协议的Socket客户端
    /// </summary>
    public abstract class BaseSocketClient : SocketInvokeElement
    {
        public BaseSocketClient()
            : base()
        {
        }

    }
}
