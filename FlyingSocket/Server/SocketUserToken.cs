using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using FlyingSocket.Common;

namespace FlyingSocket.Server
{
    /// <summary>
    /// 表示一个Socket客户端用户
    /// </summary>
    public class SocketUserToken
    {
        /// <summary>
        /// 业务身份ID
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 连接会话的临时令牌
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 客户端连接会话ID
        /// </summary>
        public string SessionId { get; set; }
        /// <summary>
        /// 此Socket上的接收事件参数
        /// </summary>
        public SocketAsyncEventArgs ReceiveEventArgs { get; private set; }
        /// <summary>
        /// 此Socket上的数据接收缓存
        /// </summary>
        public DynamicBufferManager ReceiveBuffer { get; private set; }
        /// <summary>
        /// 此Socket上的发送事件参数
        /// </summary>
        public SocketAsyncEventArgs SendEventArgs { get; private set; }
        /// <summary>
        /// 此Socket上的数据发送缓存
        /// </summary>
        public SendBufferManager SendBuffer { get; private set; }
        /// <summary>
        /// 此Socket上的通讯协议对象
        /// </summary>
        public SocketInvokeElement SocketInvokeProtocol { get; set; }
        /// <summary>
        /// 接收数据临时存放区
        /// </summary>

        private byte[] _receiveBuffer = null;

        private Socket _connectedSocket = null;
        /// <summary>
        /// 已连接上客户端的远程Socket对象
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
                    if (SocketInvokeProtocol != null)
                    {
                        SocketInvokeProtocol.Close();
                    }
                    ReceiveBuffer.Clear(ReceiveBuffer.DataCount);
                    SendBuffer.ClearPacket();
                }
                SocketInvokeProtocol = null;
                ReceiveEventArgs.AcceptSocket = _connectedSocket;
                SendEventArgs.AcceptSocket = _connectedSocket;
            }
        }
        /// <summary>
        /// 连接建立时间
        /// </summary>
        public DateTime ConnectedTime { get; set; }
        /// <summary>
        /// 最后一次活动时间
        /// </summary>
        public DateTime ActiveTime { get; set; }

        /// <summary>
        /// 创建socket用户对象实例
        /// </summary>
        /// <param name="bufferSize">此实例的数据发送接收缓存大小</param>
        public SocketUserToken(int bufferSize)
        {
            SocketInvokeProtocol = null;
            ActiveTime = DateTime.MinValue;
            ConnectedTime = DateTime.MinValue;
            _receiveBuffer = new byte[bufferSize];
            ReceiveBuffer = new DynamicBufferManager(bufferSize);
            ReceiveEventArgs = new SocketAsyncEventArgs();
            ReceiveEventArgs.UserToken = this;
            ReceiveEventArgs.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);
            SendBuffer = new SendBufferManager(bufferSize);
            SendEventArgs = new SocketAsyncEventArgs();
            SendEventArgs.UserToken = this;
            SendEventArgs.SetBuffer(SendBuffer.DynamicBufferManager.Buffer, 0, SendBuffer.DynamicBufferManager.Buffer.Length);
        }
    }
}
