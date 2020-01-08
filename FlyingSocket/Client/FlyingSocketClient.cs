using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlyingFive;
using FlyingSocket.Core;

namespace FlyingSocket.Client
{
    public class FlyingSocketClient
    {
        protected string _hostAddress = null;
        protected int _port = 0;

        /// <summary>
        /// 长度是否使用网络字节顺序
        /// </summary>
        public bool NetByteOrder { get; set; }
        /// <summary>
        /// 接收数据的缓存
        /// </summary>
        protected DynamicBufferManager ReceiveBuffer { get; private set; }
        /// <summary>
        /// 协议组装器，用来组装往外发送的命令
        /// </summary>
        protected OutgoingDataAssembler OutgoingDataAssembler { get; private set; }
        /// <summary>
        /// 收到数据的解析器，用于解析返回的内容
        /// </summary>
        protected IncomingDataParser IncomingDataParser { get; private set; }
        /// <summary>
        /// 发送数据的缓存，统一写到内存中，调用一次发送
        /// </summary>
        protected DynamicBufferManager SendBuffer { get; private set; }

        /// <summary>
        /// 此Socket上的接收事件参数
        /// </summary>
        public SocketAsyncEventArgs ReceiveEventArgs { get; private set; }
        //此Socket上的发送事件参数
        public SocketAsyncEventArgs SendEventArgs { get; private set; }
        public SocketAsyncEventArgs ConnectEventArgs { get; private set; }
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        protected int SocketBufferSize { get; private set; }

        public Socket _clientSocket = null;
        protected ProtocolFlag _protocolFlag = ProtocolFlag.Upload;

        private IPEndPoint _remoteAddress = null;
        /// <summary>
        /// 客户端是否连接上
        /// </summary>
        public bool Connected { get { return this._clientSocket != null && this._clientSocket.Connected; } }
        public FlyingSocketClient()
        {
            _clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _clientSocket.Blocking = false;

            OutgoingDataAssembler = new OutgoingDataAssembler();
            SocketBufferSize = 4096;
            var size = ConfigurationManager.AppSettings["SocketBufferSize"];
            if (!string.IsNullOrEmpty(size))
            {
                SocketBufferSize = size.TryConvert<int>(4096);
            }
            ReceiveBuffer = new DynamicBufferManager(SocketBufferSize);
            SendBuffer = new DynamicBufferManager(SocketBufferSize);
            IncomingDataParser = new IncomingDataParser();
            this.ReceiveEventArgs = new SocketAsyncEventArgs();
            this.ReceiveEventArgs.SetBuffer(ReceiveBuffer.Buffer, 0, SocketBufferSize);
            this.SendEventArgs = new SocketAsyncEventArgs();
            this.SendEventArgs.SetBuffer(SendBuffer.Buffer, 0, SocketBufferSize);
            this.ConnectEventArgs = new SocketAsyncEventArgs();
            this.ConnectEventArgs.SetBuffer(0, SocketBufferSize);
            this.ReceiveEventArgs.Completed += IO_Completed;
            this.SendEventArgs.Completed += IO_Completed;
            this.ConnectEventArgs.Completed += IO_Completed;
        }

        public void Connect(string server_ip, int server_port)
        {
            if (this.Connected)
            {
                throw new InvalidOperationException("当前客户端处于已有连接中，不能重复连接。");
            }
            _remoteAddress = new IPEndPoint(IPAddress.Parse(server_ip), server_port);
            this._clientSocket.Connect(_remoteAddress);
            if (this.Connected) { this.OnConnected?.Invoke(this, EventArgs.Empty); }
        }

        public void ConnectAsync(string server_ip, int server_port)
        {
            if (this.Connected)
            {
                throw new InvalidOperationException("当前客户端处于已有连接中，不能重复连接。");
            }
            _remoteAddress = new IPEndPoint(IPAddress.Parse(server_ip), server_port);
            this.ConnectEventArgs.RemoteEndPoint = _remoteAddress;
            //this.ReceiveEventArgs.RemoteEndPoint = _remoteAddress;
            //this.SendEventArgs.RemoteEndPoint = _remoteAddress;
            this._clientSocket.ConnectAsync(this.ConnectEventArgs);
            //byte[] socketFlag = new byte[1];
            //socketFlag[0] = (byte)_protocolFlag;
            //_tcpClient.Client.Send(socketFlag, SocketFlags.None); //发送标识
        }

        public event EventHandler<EventArgs> OnConnected;

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.LastOperation == SocketAsyncOperation.Connect)
                {
                    OnConnected?.Invoke(this, EventArgs.Empty);
                    _clientSocket.ReceiveAsync(this.ReceiveEventArgs);
                }
                if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    Console.WriteLine("客户端数据发送完成");
                }
                if (e.LastOperation == SocketAsyncOperation.Receive)
                {
                    Console.WriteLine("客户端数据接收完成");
                }
                if (e.LastOperation == SocketAsyncOperation.Disconnect)
                {
                }
            }
        }

        public void SendAsync(string content)
        {
            SendAsync(Encoding.UTF8.GetBytes(content));
        }
        public void SendAsync(byte[] buffer)
        {
            if (!this.Connected) { throw new InvalidCastException("还未建立连接。"); }
            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddRequest();
            OutgoingDataAssembler.AddCommand(ProtocolKey.Data);
            SendCommand(buffer, 0, buffer.Length);
        }


        private void SendCommand(byte[] buffer, int offset, int count)
        {
            var commandText = OutgoingDataAssembler.GetProtocolText();
            byte[] bufferUTF8 = Encoding.UTF8.GetBytes(commandText);
            int totalLength = sizeof(int) + bufferUTF8.Length + count; //获取总大小
            SendBuffer.Clear();
            SendBuffer.WriteInt(totalLength, false); //写入总大小
            SendBuffer.WriteInt(bufferUTF8.Length, false); //写入命令大小
            SendBuffer.WriteBuffer(bufferUTF8); //写入命令内容
            SendBuffer.WriteBuffer(buffer, offset, count); //写入二进制数据
            //_tcpClient.Client.Send(SendBuffer.Buffer, 0, SendBuffer.DataCount, SocketFlags.None);
            this.SendEventArgs.SetBuffer(SendBuffer.Buffer, 0, SendBuffer.DataCount);
            _clientSocket.SendAsync(this.SendEventArgs);
        }
    }
}
