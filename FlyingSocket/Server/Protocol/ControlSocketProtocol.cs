using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using FlyingSocket.Core;

namespace FlyingSocket.Server.Protocol
{
    /// <summary>
    /// 命令控制协议
    /// </summary>
    public class ControlSocketProtocol : BaseSocketProtocol
    {
        public ControlSocketProtocol(FlyingSocketServer socketServer, SocketUserToken userToken)
            : base("Control", socketServer, userToken) { }

        public override void Close()
        {
            base.Close();
        }

        public override bool ProcessCommand(byte[] buffer, int offset, int count) //处理分完包的数据，子类从这个方法继承
        {
            var command = ParseCommand(IncomingDataParser.Command);
            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddResponse();
            OutgoingDataAssembler.AddCommand(IncomingDataParser.Command);
            if (!CheckLogined(command)) //检测登录
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.UserHasLogined, "");
                return SendResult();
            }
            if (command == ControlSocketCommand.Login)
            {
                return DoLogin();
            }
            else if (command == ControlSocketCommand.Active)
            {
                return DoActive();
            }
            else if (command == ControlSocketCommand.GetClients)
            {
                return DoGetClients();
            }
            else
            {
                //Program.Logger.Error("Unknow command: " + m_incomingDataParser.Command);
                return false;
            }
        }

        protected ControlSocketCommand ParseCommand(string command)
        {
            if (command.Equals(ProtocolKey.Active, StringComparison.CurrentCultureIgnoreCase))
            {
                return ControlSocketCommand.Active;
            }
            else if (command.Equals(ProtocolKey.Login, StringComparison.CurrentCultureIgnoreCase))
            {
                return ControlSocketCommand.Login;
            }
            else if (command.Equals(ProtocolKey.GetClients, StringComparison.CurrentCultureIgnoreCase))
            {
                return ControlSocketCommand.GetClients;
            }
            else
            {
                return ControlSocketCommand.None;
            }
        }

        public bool CheckLogined(ControlSocketCommand command)
        {
            if ((command == ControlSocketCommand.Login) | (command == ControlSocketCommand.Active))
            {
                return true;
            }
            else
            {
                return base.Logined;
            }
        }

        public bool DoGetClients()
        {
            SocketUserToken[] userTokenArray = null;
            FlyingSocketServer.ConnectedClients.CopyList(ref userTokenArray);
            OutgoingDataAssembler.AddSuccess();
            string socketText = "";
            for (int i = 0; i < userTokenArray.Length; i++)
            {
                try
                {
                    socketText = userTokenArray[i].ConnectSocket.LocalEndPoint.ToString() + "\t"
                        + userTokenArray[i].ConnectSocket.RemoteEndPoint.ToString() + "\t"
                        + (userTokenArray[i].SocketInvokeProtocol as BaseSocketProtocol).Name + "\t"
                        + (userTokenArray[i].SocketInvokeProtocol as BaseSocketProtocol).UserName + "\t"
                        + userTokenArray[i].SocketInvokeProtocol.ConnectTime.ToString() + "\t"
                        + userTokenArray[i].SocketInvokeProtocol.ActiveTime.ToString();
                    OutgoingDataAssembler.AddValue(ProtocolKey.Item, socketText);
                }
                catch (Exception E)
                {
                    throw E;
                    //Program.Logger.ErrorFormat("Get client error, message: {0}", E.Message);
                    //Program.Logger.Error(E.StackTrace);
                }
            }
            return SendResult();
        }
    }
}
