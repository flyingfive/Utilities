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
        public string ErrorString { get; protected set; }
        protected string _userName = null;
        protected string _password = null;

        public BaseSocketClient()
            : base()
        {
        }

        public bool CheckErrorCode()
        {
            int code = 0;
            IncomingDataParser.GetValue(ProtocolKey.Code, ref code);
            var status = code.TryConvert<ProtocolStatus>();
            if (status == ProtocolStatus.Success)
            {
                return true;
            }
            else
            {
                ErrorString = ProtocolCode.GetErrorString(status);
                return false;
            }
        }

        public bool DoActive()
        {
            try
            {
                OutgoingDataAssembler.Clear();
                OutgoingDataAssembler.AddRequest();
                OutgoingDataAssembler.AddCommand(ProtocolKey.Active);
                SendCommand();
                var success = ReceiveCommand();
                if (success)
                {
                    return CheckErrorCode();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception E)
            {
                //记录日志
                ErrorString = E.Message;
                return false;
            }
        }

        public bool DoLogin(string userName, string password)
        {
            try
            {
                OutgoingDataAssembler.Clear();
                OutgoingDataAssembler.AddRequest();
                OutgoingDataAssembler.AddCommand(ProtocolKey.Login);
                OutgoingDataAssembler.AddValue(ProtocolKey.UserName, userName);
                OutgoingDataAssembler.AddValue(ProtocolKey.Password, password.MD5());
                SendCommand();
                var success = ReceiveCommand();
                if (success)
                {
                    success = CheckErrorCode();
                    if (success)
                    {
                        _userName = userName;
                        _password = password;
                    }
                    return success;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception E)
            {
                //记录日志
                ErrorString = E.Message;
                return false;
            }
        }

        public bool ReConnect()
        {
            if (_tcpClient.Connected && (!DoActive()))
            {
                return true;
            }
            else
            {
                if (!_tcpClient.Connected)
                {
                    try
                    {
                        Connect(_hostAddress, _port);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        public bool ReconnectAndLogin()
        {
            if (_tcpClient.Connected && (!DoActive()))
            {
                return true;
            }
            else
            {
                if (!_tcpClient.Connected)
                {
                    try
                    {
                        Disconnect();
                        Connect(_hostAddress, _port);
                        return DoLogin(_userName, _password);
                    }
                    catch (Exception E)
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
