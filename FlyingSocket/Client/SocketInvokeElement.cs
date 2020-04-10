using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FlyingFive;
using FlyingSocket.Common;
using FlyingSocket.Server.Protocol;

namespace FlyingSocket.Client
{
    /// <summary>
    /// 客户端Socket调用节点（同步模式）
    /// </summary>
    public abstract class SocketInvokeElement
    {
        protected TcpClient _tcpClient = null;
        protected string _hostAddress = null;
        protected int _port = 0;
        protected SocketProtocolType SocketProtocolType { get; private set; }
        public bool Connected { get { return _tcpClient != null && _tcpClient.Client.Connected; } }

        public event EventHandler<EventArgs> OnConnected;
        /// <summary>
        /// Socket操作超时时间，单位：毫秒
        /// </summary>
        protected int SocketTimeOut { get { if (_tcpClient == null) { return -1; } return _tcpClient.SendTimeout; } set { if (_tcpClient == null) { return; } _tcpClient.SendTimeout = value; _tcpClient.ReceiveTimeout = value; } }
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
        protected SocketAsyncEventArgs ReceiveEventArgs { get; private set; }
        //此Socket上的发送事件参数
        protected SocketAsyncEventArgs SendEventArgs { get; private set; }
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        protected int SocketBufferSize { get; private set; }

        public SocketInvokeElement()
        {
            var attr = this.GetType().GetCustomAttribute<ProtocolNameAttribute>(false);
            if (attr == null)
            {
                throw new InvalidOperationException("无法识别到处理程序的协议类型。");
            }
            SocketProtocolType = attr.ProtocolType;
            _tcpClient = new TcpClient();
            _tcpClient.Client.Blocking = true; //使用阻塞模式，即同步模式
            SocketTimeOut = 60 * 1000;//ProtocolConst.SocketTimeOut;
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
            this.ReceiveEventArgs.Completed += IO_Completed;
            this.SendEventArgs = new SocketAsyncEventArgs();
            this.SendEventArgs.Completed += IO_Completed;
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {

        }

        public string SessionID { get; private set; }
        public string TokenId { get; private set; }

        public bool Connect(string host, int port)
        {
            try
            {
                var task = _tcpClient.ConnectAsync(host, port);
                task.Wait();

                OutgoingDataAssembler.Clear();
                OutgoingDataAssembler.AddRequest();
                OutgoingDataAssembler.AddCommand(CommandKeys.Login);
                OutgoingDataAssembler.AddValue(CommandKeys.Protocol, Convert.ToInt32(SocketProtocolType));
                OutgoingDataAssembler.AddValue(CommandKeys.UserName, "admin");
                OutgoingDataAssembler.AddValue(CommandKeys.Password, "admin".MD5());

                SendCommand();

                var success = ReceiveCommand();
                if (!success) { _tcpClient.Close(); return false; }
                success = CheckErrorCode();
                if (!success) { _tcpClient.Close(); return false; }
                var sessionId = "";
                var token = "";
                if (!IncomingDataParser.GetValue(CommandKeys.SessionID, ref sessionId)) { _tcpClient.Close(); return false; }
                if (!IncomingDataParser.GetValue(CommandKeys.Token, ref token)) { _tcpClient.Close(); return false; }
                SessionID = sessionId;
                TokenId = token;
                _hostAddress = host;
                _port = port;
                if (Connected)
                {
                    OnConnected?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (SocketException ex)
            {
            }
            catch (Exception ex)
            {
            }
            return Connected;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            _tcpClient.Close();
            _tcpClient = new TcpClient();
        }


        public void SendCommand()
        {
            var commandText = OutgoingDataAssembler.GetProtocolText();
            var data = Encoding.UTF8.GetBytes(commandText);
            int totalLength = ProtocolCode.IntegerSize + data.Length; //获取总大小(int命令长度+命令内容的总长度)，实际发送大小要在此基础加上sizeof(int)
            SendBuffer.Clear();
            SendBuffer.WriteInt(totalLength, false);                    //写入总大小    4byte
            SendBuffer.WriteInt(data.Length, false);                    //写入命令大小  8byte
            SendBuffer.WriteBuffer(data);                               //写入命令内容  8byte + data.Length
            //使用阻塞模式，Socket会一次发送完所有数据后返回
            var cnt = _tcpClient.Client.Send(SendBuffer.Buffer, 0, SendBuffer.DataCount, SocketFlags.None);
            //SendEventArgs.SetBuffer(SendBuffer.Buffer, 0, SendBuffer.DataCount);
            //_tcpClient.Client.SendAsync(this.SendEventArgs);
        }

        public void SendCommand(byte[] buffer, int offset, int count)
        {
            var commandText = OutgoingDataAssembler.GetProtocolText();
            var bufferUTF8 = Encoding.UTF8.GetBytes(commandText);
            var totalLength = ProtocolCode.IntegerSize + bufferUTF8.Length + count; //获取总大小
            SendBuffer.Clear();
            SendBuffer.WriteInt(totalLength, false); //写入总大小
            SendBuffer.WriteInt(bufferUTF8.Length, false); //写入命令大小
            SendBuffer.WriteBuffer(bufferUTF8); //写入命令内容
            SendBuffer.WriteBuffer(buffer, offset, count); //写入二进制数据
            _tcpClient.Client.Send(SendBuffer.Buffer, 0, SendBuffer.DataCount, SocketFlags.None);
        }

        public bool ReceiveCommand()
        {
            ReceiveBuffer.Clear();
            _tcpClient.Client.Receive(ReceiveBuffer.Buffer, ProtocolCode.IntegerSize, SocketFlags.None);
            int packetLength = BitConverter.ToInt32(ReceiveBuffer.Buffer, 0); //获取包长度
            if (NetByteOrder)
            {
                packetLength = System.Net.IPAddress.NetworkToHostOrder(packetLength); //把网络字节顺序转为本地字节顺序
            }
            ReceiveBuffer.SetBufferSize(ProtocolCode.IntegerSize + packetLength); //保证接收有足够的空间
            _tcpClient.Client.Receive(ReceiveBuffer.Buffer, ProtocolCode.IntegerSize, packetLength, SocketFlags.None);
            int commandLen = BitConverter.ToInt32(ReceiveBuffer.Buffer, ProtocolCode.IntegerSize); //取出命令长度
            string tmpStr = Encoding.UTF8.GetString(ReceiveBuffer.Buffer, ProtocolCode.IntegerSize + ProtocolCode.IntegerSize, commandLen);
            if (!IncomingDataParser.DecodeProtocolText(tmpStr)) //解析命令
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool RecvCommand(out byte[] buffer, out int offset, out int size)
        {
            ReceiveBuffer.Clear();
            _tcpClient.Client.Receive(ReceiveBuffer.Buffer, ProtocolCode.IntegerSize, SocketFlags.None);
            int packetLength = BitConverter.ToInt32(ReceiveBuffer.Buffer, 0); //获取包长度
            if (NetByteOrder)
            {
                packetLength = System.Net.IPAddress.NetworkToHostOrder(packetLength); //把网络字节顺序转为本地字节顺序
            }
            ReceiveBuffer.SetBufferSize(ProtocolCode.IntegerSize + packetLength); //保证接收有足够的空间
            _tcpClient.Client.Receive(ReceiveBuffer.Buffer, ProtocolCode.IntegerSize, packetLength, SocketFlags.None);
            int commandLen = BitConverter.ToInt32(ReceiveBuffer.Buffer, ProtocolCode.IntegerSize); //取出命令长度
            string tmpStr = Encoding.UTF8.GetString(ReceiveBuffer.Buffer, ProtocolCode.IntegerSize + ProtocolCode.IntegerSize, commandLen);
            if (!IncomingDataParser.DecodeProtocolText(tmpStr)) //解析命令
            {
                buffer = null;
                offset = 0;
                size = 0;
                return false;
            }
            else
            {
                buffer = ReceiveBuffer.Buffer;
                offset = commandLen + ProtocolCode.IntegerSize + ProtocolCode.IntegerSize;
                size = packetLength - offset;
                return true;
            }
        }
        public string ErrorString { get; protected set; }
        public bool CheckErrorCode()
        {
            int code = 0;
            IncomingDataParser.GetValue(CommandKeys.Code, ref code);
            var status = code.TryConvert<CommandResult>();
            if (status == CommandResult.Success)
            {
                return true;
            }
            else
            {
                ErrorString = ProtocolCode.GetErrorString(status);
                return false;
            }
        }
    }
}
