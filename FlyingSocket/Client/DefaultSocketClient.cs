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
        protected SocketAsyncEventArgs SendEventArgs { get; private set; }

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
        protected SocketAsyncEventArgs ReceiveEventArgs { get; private set; }
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
        /// 临时接收数据
        /// </summary>
        private byte[] _receivedData = null;
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
            //var connectData = new byte[] { Convert.ToByte(SocketProtocolType.Default) };


            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddRequest();
            OutgoingDataAssembler.AddCommand(CommandKeys.Identify);
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
                    OnConnected?.Invoke(this, EventArgs.Empty);
                    if (_receivingReset.WaitOne(_socketTimeout))
                    {
                        this.ReceiveEventArgs.SetBuffer(_receivedData, 0, _receivedData.Length);
                        this._clientSocket.ReceiveAsync(ReceiveEventArgs);
                    }
                }
                if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    _sendingReset.Set();
                    Console.WriteLine("客户端数据发送完成");
                    if (_receivingReset.WaitOne(_socketTimeout))
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
                    _receivingReset.Set();
                    if (e.BytesTransferred == 0)            //服务端请求关闭连接
                    {
                        _clientSocket.DisconnectAsync(DisconnectEventArgs);
                    }
                    else
                    {
                        //会将服务端的多次数据响应全部集中一起返回。这里应解析字节大小外再重新调用ReceiveAsync方法循环处理。
                        Console.WriteLine("客户端数据接收完成" + e.BytesTransferred);
                        //var text = Encoding.UTF8.GetString(e.Buffer, 8, e.BytesTransferred - 8);
                        //Debug.WriteLine(text);
                    }
                }
            }
        }
        /// <summary>
        /// 异步发送字符串内容
        /// </summary>
        /// <param name="content"></param>
        public void SendAsync(string content)
        {
            SendAsync(Encoding.UTF8.GetBytes(content));
        }

        /// <summary>
        /// 异步发送指定数据
        /// </summary>
        /// <param name="buffer"></param>
        public void SendAsync(byte[] buffer)
        {
            if (!this.IsConnected) { throw new InvalidCastException("还未建立连接。"); }

            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddRequest();
            OutgoingDataAssembler.BeginRequest(buffer.Length);
            Console.WriteLine("客户端发送消息头");

            WriteOutgoingData(SendBuffer);
            SendBufferData();

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
                    this.SendDataCommand(readBuffer, 0, count);
                }
                //发送EOF命令告诉服务端文件上传完成，此时服务端会关闭写入文件流。
                //todo:最好加上文件MD5校验
                Console.WriteLine("客户端发送结束消息。");
                this.EOF(stream.Length);
            }
            Array.Clear(buffer, 0, buffer.Length);
            MemoryUtility.FreeMemory();
        }

        /// <summary>
        /// Socket异步IO操作超时时间，单位毫秒
        /// </summary>
        private int _socketTimeout = 1000;
        /// <summary>
        /// 数据发送重置信号
        /// </summary>
        private AutoResetEvent _sendingReset = new AutoResetEvent(true);
        /// <summary>
        /// 数据接收重置信号
        /// </summary>
        private AutoResetEvent _receivingReset = new AutoResetEvent(true);

        /// <summary>
        /// 结束命令（待添加MD5数据校验）
        /// </summary>
        /// <param name="fileSize"></param>
        protected void EOF(Int64 fileSize)
        {
            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddRequest();
            OutgoingDataAssembler.AddCommand(CommandKeys.EOF);
            WriteOutgoingData(SendBuffer);
            SendBufferData();
        }

        /// <summary>
        /// 发送数据处理命令
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        private void SendDataCommand(byte[] buffer, int offset, int count)
        {
            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddRequest();
            OutgoingDataAssembler.AddCommand(CommandKeys.Data);
            WriteOutgoingData(SendBuffer, count);
            SendBuffer.WriteBuffer(buffer, offset, count); //写入二进制数据
            SendBufferData();
        }

        /// <summary>
        /// 写入待发送的数据到指定缓存
        /// </summary>
        /// <param name="buffer">缓存对象</param>
        /// <param name="additionalDataCount">Data命令中发送实际数据的长度</param>
        private void WriteOutgoingData(DynamicBufferManager buffer, int additionalDataCount = 0)
        {
            if (additionalDataCount < 0) { additionalDataCount = 0; }
            var protocolText = OutgoingDataAssembler.GetProtocolText();
            var commandData = Encoding.UTF8.GetBytes(protocolText);
            //获取总大小(int命令长度+命令内容的总长度+数据长度)，实际发送大小要在此基础加上sizeof(int)
            var totalLength = ProtocolCode.IntegerSize + commandData.Length + additionalDataCount;
            buffer.Clear();
            buffer.WriteInt(totalLength, false);                           //写入总大小    4byte
            buffer.WriteInt(commandData.Length, false);                    //写入命令大小  8byte
            buffer.WriteBuffer(commandData);                               //写入命令内容  8byte + data.Length
        }

        /// <summary>
        /// 将SendBuffer中的缓存数据发送出去
        /// </summary>
        private void SendBufferData()
        {
            var success = _sendingReset.WaitOne(_socketTimeout);
            if (!success)
            {
                Console.WriteLine("发送超时。");
                return;
            }
            this.SendEventArgs.SetBuffer(SendBuffer.Buffer, 0, SendBuffer.DataCount);
            var willRaiseEvent = _clientSocket.SendAsync(this.SendEventArgs);
            //true,异步，在IO_Complete中完成，false，同步完成，不触发IO_Complete
            if (!willRaiseEvent)
            {
                this.SendEventArgs.SetBuffer(null, 0, 0);
                _sendingReset.Set();

                success = _receivingReset.WaitOne(_socketTimeout);
                if (success)
                {
                    this.ReceiveEventArgs.SetBuffer(_receivedData, 0, _receivedData.Length);
                    _clientSocket.ReceiveAsync(this.ReceiveEventArgs);
                }
            }
        }
    }
}
