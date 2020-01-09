using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using FlyingSocket.Core;
using FlyingFive;
using System.IO;

namespace FlyingSocket.Server
{
    /// <summary>
    /// Socket自定义协议基类。所有的协议处理都从本类继承
    /// </summary>
    public class BaseSocketProtocol : SocketInvokeElement
    {
        /// <summary>
        /// 登录的业务用户名
        /// </summary>
        public string UserName { get; private set; }
        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool Logined { get; private set; }
        /// <summary>
        /// 协议名
        /// </summary>
        public string ProtocolName { get; protected set; }
        /// <summary>
        /// 文件协议工作目录
        /// </summary>
        public string FileWorkingDirectory { get; private set; }

        public BaseSocketProtocol(FlyingSocketServer socketServer, SocketUserToken userToken)
            : base(socketServer, userToken)
        {
            UserName = "";
            ProtocolName = "";
            Logined = false;
            FileWorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(FileWorkingDirectory)) { Directory.CreateDirectory(FileWorkingDirectory); }
        }

        public bool DoLogin()
        {
            var userName = "";
            var password = "";
            if (IncomingDataParser.GetValue(ProtocolKey.UserName, ref userName) & IncomingDataParser.GetValue(ProtocolKey.Password, ref password))
            {
                if (password.Equals("admin".MD5(), StringComparison.CurrentCultureIgnoreCase))
                {
                    OutgoingDataAssembler.AddSuccess();
                    UserName = userName;
                    Logined = true;
                    //Program.Logger.InfoFormat("{0} login success", userName);
                }
                else
                {
                    OutgoingDataAssembler.AddFailure(ProtocolCode.UserOrPasswordError, "");
                    //Program.Logger.ErrorFormat("{0} login failure,password error", userName);
                }
            }
            else
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.ParameterError, "");
            }
            return SendResult();
        }

        public bool DoActive()
        {
            OutgoingDataAssembler.AddSuccess();
            return SendResult();
        }
    }
}
