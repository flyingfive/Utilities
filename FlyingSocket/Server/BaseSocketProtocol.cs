using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using FlyingSocket.Common;
using FlyingFive;
using System.IO;
using FlyingSocket.Server.Protocol;

namespace FlyingSocket.Server
{
    /// <summary>
    /// Socket自定义协议基类。所有的协议处理都从本类继承
    /// </summary>
    public abstract class BaseSocketProtocol : SocketInvokeElement
    {
        /// <summary>
        /// 协议类型
        /// </summary>
        public SocketProtocolType ProtocolType { get; protected set; }
        /// <summary>
        /// 文件协议工作目录
        /// </summary>
        public string FileWorkingDirectory { get; private set; }

        public BaseSocketProtocol(FlyingSocketServer socketServer, SocketUserToken userToken)
            : base(socketServer, userToken)
        {
            var attr = this.GetType().GetCustomAttribute<ProtocolNameAttribute>(false);
            if (attr == null)
            {
                throw new InvalidOperationException("无法识别到处理程序的协议类型。");
            }
            ProtocolType = attr.ProtocolType;
            FileWorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(FileWorkingDirectory)) { Directory.CreateDirectory(FileWorkingDirectory); }
        }

        public bool DoActive()
        {
            OutgoingDataAssembler.AddSuccess();
            return SendResult();
        }
    }
}
