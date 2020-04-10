using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlyingFive;
using FlyingSocket.Common;
using FlyingSocket.Utility;

namespace FlyingSocket.Client
{
    /// <summary>
    /// 默认数据协议的Socket客户端（异步模式）
    /// </summary>
    public partial class DefaultSocketClient
    {
        /// <summary>
        /// 长度是否使用网络字节顺序
        /// </summary>
        public bool NetByteOrder { get; set; }
        protected SocketAsyncEventArgs ConnectEventArgs { get; private set; }
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        protected int SocketBufferSize { get; private set; } = 32 * 1024;         //分包大小：32KB

        protected Socket _clientSocket = null;

        private IPEndPoint _remoteAddress = null;
        /// <summary>
        /// 客户端是否连接上
        /// </summary>
        public bool IsConnected { get { return this._clientSocket != null && this._clientSocket.Connected; } }
        /// <summary>
        /// 客户端ID
        /// </summary>
        public String ClientId { get; private set; }

        private string _authCode = null;
        /// <summary>
        /// 会话ID
        /// </summary>
        public string SessionID { get; private set; }
        /// <summary>
        /// 会话Token
        /// </summary>
        public string Token { get; private set; }
        public DefaultSocketClient(string clientId, string authCode)
        {
            ClientId = clientId;
            _authCode = authCode;
            _clientSocket = new Socket(SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

            OutgoingDataAssembler = new OutgoingDataAssembler();
            _receivedData = new byte[SocketBufferSize];
            ReceiveBuffer = new DynamicBufferManager(SocketBufferSize);
            SendBuffer = new DynamicBufferManager(SocketBufferSize);
            IncomingDataParser = new IncomingDataParser();

            this.ReceiveEventArgs = new SocketAsyncEventArgs() { SocketFlags = SocketFlags.None };
            this.ReceiveEventArgs.Completed += IO_Completed;
            //this.ReceiveEventArgs.SetBuffer(ReceiveBuffer.Buffer, 0, SocketBufferSize);

            this.SendEventArgs = new SocketAsyncEventArgs() { SocketFlags = SocketFlags.None };
            this.SendEventArgs.Completed += IO_Completed;
            this.SendEventArgs.SetBuffer(SendBuffer.Buffer, 0, SocketBufferSize);

            this.ConnectEventArgs = new SocketAsyncEventArgs();
            this.ConnectEventArgs.Completed += IO_Completed;
            this.DisconnectEventArgs = new SocketAsyncEventArgs();
            this.DisconnectEventArgs.Completed += IO_Completed;
        }

        public void Connect(string server_ip, int server_port)
        {
            if (this.IsConnected)
            {
                throw new InvalidOperationException("当前客户端处于已有连接中，不能重复连接。");
            }
            _remoteAddress = new IPEndPoint(IPAddress.Parse(server_ip), server_port);
            this._clientSocket.Connect(_remoteAddress);
            if (this.IsConnected) { this.OnConnected?.Invoke(this, EventArgs.Empty); }
        }

        public void ConnectAsync(string server_ip, int server_port)
        {
            if (this.IsConnected)
            {
                throw new InvalidOperationException("当前客户端处于已有连接中，不能重复连接。");
            }
            _remoteAddress = new IPEndPoint(IPAddress.Parse(server_ip), server_port);
            this.ConnectEventArgs.RemoteEndPoint = _remoteAddress;
            this.DisconnectEventArgs.RemoteEndPoint = _remoteAddress;
            //发送1字节的连接数据，标明使用默认数据协议
            //var connectData = new byte[] { Convert.ToByte(SocketProtocolType.Default) };
            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddRequest();
            OutgoingDataAssembler.AddCommand(CommandKeys.Login);
            OutgoingDataAssembler.AddValue(CommandKeys.Protocol, Convert.ToInt32(SocketProtocolType.Default));
            OutgoingDataAssembler.AddValue(CommandKeys.UserName, ClientId);
            OutgoingDataAssembler.AddValue(CommandKeys.Password, _authCode);
            var connectBuffer = new DynamicBufferManager(SocketBufferSize);
            WriteOutgoingData(connectBuffer);
            this.ConnectEventArgs.SetBuffer(connectBuffer.Buffer, 0, connectBuffer.DataCount);
            this._clientSocket.ConnectAsync(this.ConnectEventArgs);
        }
        
        public void Disconnect()
        {
            if (this.IsConnected)
            {
                this._clientSocket.Disconnect(false);
            }
        }

        public void DisconnectAsync()
        {
            if (this.IsConnected)
            {
                this._clientSocket.DisconnectAsync(this.DisconnectEventArgs);
            }
        }

        public event EventHandler<EventArgs> OnConnected;
        public event EventHandler<EventArgs> OnDisconnected;
        protected SocketAsyncEventArgs DisconnectEventArgs { get; private set; }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.LastOperation == SocketAsyncOperation.Connect)
                {
                    ConnectSucceed();
                }
                if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    _sendingReset.Set();
                    Console.WriteLine("客户端数据发送完成");
                    //if (_receivingReset.WaitOne(_socketTimeout))
                    //{
                    //    this.ReceiveEventArgs.SetBuffer(_receivedData, 0, _receivedData.Length);
                    //    _clientSocket.ReceiveAsync(this.ReceiveEventArgs);
                    //}
                }
                if (e.LastOperation == SocketAsyncOperation.Disconnect)
                {
                    OnDisconnected?.Invoke(this, EventArgs.Empty);
                }
                if (e.LastOperation == SocketAsyncOperation.Receive)
                {
                    AsyncReceiveData(e);
                }
            }
        }

        private void ConnectSucceed()
        {
            OnConnected?.Invoke(this, EventArgs.Empty);
            if (_receivingReset.WaitOne(_socketTimeout))
            {
                this.ReceiveEventArgs.SetBuffer(_receivedData, 0, _receivedData.Length);
                var success = this._clientSocket.ReceiveAsync(ReceiveEventArgs);
                Console.WriteLine(success ? "connect receive true" : "connect receive false");
            }
            else
            {
                throw new TimeoutException("ConnectSucceed操作超时");
            }
        }

        /// <summary>
        /// Socket异步IO操作超时时间，单位毫秒
        /// </summary>
        private int _socketTimeout = 1000;

    }
}
