using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using FlyingSocket.Core;

namespace FlyingSocket.Server
{
    /// <summary>
    /// 表示一个Socket客户端用户
    /// </summary>
    public class SocketUserToken
    {
        /// <summary>
        /// 客户端标识ID
        /// </summary>
        public string TokenId { get; set; }

        private byte[] _receiveBuffer = null;
        /// <summary>
        /// 此Socket上的接收事件参数
        /// </summary>
        public SocketAsyncEventArgs ReceiveEventArgs { get; private set; }
        //此Socket上的发送事件参数
        public SocketAsyncEventArgs SendEventArgs { get; private set; }
        /// <summary>
        /// 此Socket上的数据接收缓存
        /// </summary>
        public DynamicBufferManager ReceiveBuffer { get; private set; }
        /// <summary>
        /// 此Socket上的数据发送缓存
        /// </summary>
        public SendBufferManager SendBuffer { get; private set; }

        /// <summary>
        /// 协议对象
        /// </summary>
        public SocketInvokeElement AsyncSocketInvokeElement { get; set; }

        private Socket _connectedSocket = null;
        /// <summary>
        /// 已连接上的Socket对象
        /// </summary>
        public Socket ConnectSocket
        {
            get
            {
                return _connectedSocket;
            }
            set
            {
                _connectedSocket = value;
                if (_connectedSocket == null) //清理缓存
                {
                    if (AsyncSocketInvokeElement != null)
                    {
                        AsyncSocketInvokeElement.Close();
                    }
                    ReceiveBuffer.Clear(ReceiveBuffer.DataCount);
                    SendBuffer.ClearPacket();
                }
                AsyncSocketInvokeElement = null;
                ReceiveEventArgs.AcceptSocket = _connectedSocket;
                SendEventArgs.AcceptSocket = _connectedSocket;
            }
        }

        public DateTime ConnectDateTime { get; set; }
        public DateTime ActiveDateTime { get; set; }

        public SocketUserToken(int receiveBufferSize)
        {
            AsyncSocketInvokeElement = null;
            ReceiveEventArgs = new SocketAsyncEventArgs();
            ReceiveEventArgs.UserToken = this;
            _receiveBuffer = new byte[receiveBufferSize];
            ReceiveEventArgs.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);
            SendEventArgs = new SocketAsyncEventArgs();
            SendEventArgs.UserToken = this;
            ReceiveBuffer = new DynamicBufferManager(receiveBufferSize);//ProtocolConst.InitBufferSize);
            SendBuffer = new SendBufferManager(receiveBufferSize);// ProtocolConst.InitBufferSize); ;
        }
    }
}
