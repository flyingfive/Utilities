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
    public class DefaultSocketClient
    {
        protected string _hostAddress = null;
        protected int _port = 0;

        /// <summary>
        /// 长度是否使用网络字节顺序
        /// </summary>
        public bool NetByteOrder { get; set; }
        /// <summary>
        /// 协议组装器，用来组装往外发送的命令
        /// </summary>
        protected OutgoingDataAssembler OutgoingDataAssembler { get; private set; }
        /// <summary>
        /// 发送数据的缓存，统一写到内存中，调用一次发送
        /// </summary>
        protected DynamicBufferManager SendBuffer { get; private set; }
        //此Socket上的发送事件参数
        public SocketAsyncEventArgs SendEventArgs { get; private set; }

        /// <summary>
        /// 收到数据的解析器，用于解析返回的内容
        /// </summary>
        protected IncomingDataParser IncomingDataParser { get; private set; }
        /// <summary>
        /// 接收数据的缓存(还没有像服务端一样对大数据做粘包处理)
        /// </summary>
        protected DynamicBufferManager ReceiveBuffer { get; private set; }
        /// <summary>
        /// 此Socket上的接收事件参数
        /// </summary>
        public SocketAsyncEventArgs ReceiveEventArgs { get; private set; }
        public SocketAsyncEventArgs ConnectEventArgs { get; private set; }
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        protected int SocketBufferSize { get; private set; } = 32 * 1024;         //分包大小：32KB

        public Socket _clientSocket = null;

        private IPEndPoint _remoteAddress = null;
        /// <summary>
        /// 客户端是否连接上
        /// </summary>
        public bool IsConnected { get { return this._clientSocket != null && this._clientSocket.Connected; } }
        /// <summary>
        /// 临时接收数据
        /// </summary>
        private byte[] _receivedData = null;
        public DefaultSocketClient()
        {
            _clientSocket = new Socket(SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

            OutgoingDataAssembler = new OutgoingDataAssembler();
            //SocketBufferSize = 4096;
            //var size = ConfigurationManager.AppSettings["SocketBufferSize"];
            //if (!string.IsNullOrEmpty(size))
            //{
            //    SocketBufferSize = size.TryConvert<int>(4096);
            //}
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
            var connectData = new byte[] { Convert.ToByte(SocketProtocolType.Default) };
            this.ConnectEventArgs.SetBuffer(connectData, 0, 1);
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
        public SocketAsyncEventArgs DisconnectEventArgs { get; private set; }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.LastOperation == SocketAsyncOperation.Connect)
                {
                    OnConnected?.Invoke(this, EventArgs.Empty);
                }
                if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    this.SendEventArgs.SetBuffer(null, 0, 0);
                    _resetSending.Set();
                    Console.WriteLine("客户端数据发送完成");
                    if (_resetReceiving.WaitOne(_socketTimeout))
                    {
                        this.ReceiveEventArgs.SetBuffer(_receivedData, 0, _receivedData.Length);
                        _clientSocket.ReceiveAsync(this.ReceiveEventArgs);
                    }
                }
                if (e.LastOperation == SocketAsyncOperation.Disconnect)
                {
                    OnDisconnected?.Invoke(this, EventArgs.Empty);
                    //Console.WriteLine("客户端连接断开");
                }
                if (e.LastOperation == SocketAsyncOperation.Receive)
                {
                    _resetReceiving.Set();
                    if (e.BytesTransferred == 0)            //服务端请求关闭连接
                    {
                        _clientSocket.DisconnectAsync(DisconnectEventArgs);
                    }
                    else
                    {
                        Console.WriteLine("客户端数据接收完成" + e.BytesTransferred);
                    }
                }
            }
        }

        public void SendAsync(string content)
        {
            SendAsync(Encoding.UTF8.GetBytes(content));
        }

        public void SendAsync(byte[] buffer)
        {
            if (!this.IsConnected) { throw new InvalidCastException("还未建立连接。"); }

            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddRequest();
            OutgoingDataAssembler.BeginRequest(buffer.Length);
            Console.WriteLine("客户端发送消息头");
            SendMessageHead();
            //return;
            var fileSize = 0;
            using (var stream = new MemoryStream(buffer))
            {
                stream.Position = fileSize;
                var readBuffer = new byte[SocketBufferSize];
                var i = 1;
                while (stream.Position < stream.Length)
                {
                    int count = stream.Read(readBuffer, 0, SocketBufferSize);
                    Console.WriteLine(string.Format("客户端第{0}次发送数据体：{1}", i++, count));
                    this.DoData(readBuffer, 0, count);
                }
                //发送EOF命令告诉服务端文件上传完成，此时服务端会关闭写入文件流。
                //todo:最好加上文件MD5校验
                Console.WriteLine("客户端发送结束消息。");
                this.DoEof(stream.Length);
            }
            Array.Clear(buffer, 0, buffer.Length);
            MemoryUtility.FreeMemory();
        }

        private int _socketTimeout = 1000;

        private AutoResetEvent _resetSending = new AutoResetEvent(true);
        private AutoResetEvent _resetReceiving = new AutoResetEvent(true);

        private void SendData(byte[] buffer, int offset, int count)
        {
            var success = _resetSending.WaitOne(_socketTimeout);
            if (!success)
            {
                Console.WriteLine("发送超时。");
                return;
            }
            var commandText = OutgoingDataAssembler.GetProtocolText();
            var data = Encoding.UTF8.GetBytes(commandText);
            int totalLength = sizeof(int) + data.Length + count; //获取总大小
            SendBuffer.Clear();
            SendBuffer.WriteInt(totalLength, false); //写入总大小
            SendBuffer.WriteInt(data.Length, false); //写入命令大小
            SendBuffer.WriteBuffer(data); //写入命令内容
            SendBuffer.WriteBuffer(buffer, offset, count); //写入二进制数据
            Send();
        }

        private void DoData(byte[] buffer, int offset, int count)
        {
            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddRequest();
            OutgoingDataAssembler.AddCommand(CommandKeys.Data);
            SendData(buffer, offset, count);
        }
        public void DoEof(Int64 fileSize)
        {
            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddRequest();
            OutgoingDataAssembler.AddCommand(CommandKeys.EOF);
            SendMessageHead();
        }

        private void SendMessageHead()
        {
            var success = _resetSending.WaitOne(_socketTimeout);
            if (!success)
            {
                Console.WriteLine("发送超时。");
                return;
            }
            var commandText = OutgoingDataAssembler.GetProtocolText();
            var data = Encoding.UTF8.GetBytes(commandText);
            int totalLength = sizeof(int) + data.Length; //获取总大小
            SendBuffer.Clear();
            SendBuffer.WriteInt(totalLength, false); //写入总大小
            SendBuffer.WriteInt(data.Length, false); //写入命令大小
            SendBuffer.WriteBuffer(data); //写入命令内容
            Send();
        }

        private void Send()
        {
            this.SendEventArgs.SetBuffer(SendBuffer.Buffer, 0, SendBuffer.DataCount);
            var willRaiseEvent = _clientSocket.SendAsync(this.SendEventArgs);
            //true,异步，在IO_Complete中完成，false，同步完成，不触发IO_Complete
            if (!willRaiseEvent)
            {
                this.SendEventArgs.SetBuffer(null, 0, 0);
                _resetSending.Set();


                var success = _resetReceiving.WaitOne(_socketTimeout);
                if (success)
                {
                    this.ReceiveEventArgs.SetBuffer(_receivedData, 0, _receivedData.Length);
                    _clientSocket.ReceiveAsync(this.ReceiveEventArgs);
                }
            }
        }
    }
}
