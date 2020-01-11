using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyingSocket.Core;

namespace FlyingSocket.Server.Protocol
{
    /// <summary>
    /// 默认数据通讯协议
    /// </summary>
    [ProtocolName(FlyingProtocolType.Default)]
    public class DefaultDataProtocol : BaseSocketProtocol
    {
        public DefaultDataProtocol(FlyingSocketServer socketServer, SocketUserToken userToken) : base("Default", socketServer, userToken) { }


        public override bool ProcessCommand(byte[] buffer, int offset, int count)
        {
            var data = new byte[count];
            Array.Copy(buffer, offset, data, 0, count);
            base.FlyingSocketServer.ClientMessageReceived(data);
            return true;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ProtocolNameAttribute : System.Attribute
    {
        public FlyingProtocolType ProtocolType { get; private set; }

        public ProtocolNameAttribute(FlyingProtocolType type) { this.ProtocolType = type; }

    }
}
