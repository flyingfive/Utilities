using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FlyingSocket.Server.Protocol;
using FlyingSocket.Common;
using System.Diagnostics;
using System.Reflection;
using FlyingFive;

namespace FlyingSocket.Server
{
    public class FlyingSocketServer
    {
        private Socket _listenSocket = null;

        /// <summary>
        /// 最大支持连接个数
        /// </summary>
        //private int _maxConnections = 8000;
        /// <summary>
        /// 每个连接接收缓存大小
        /// </summary>
        //private int _receiveBufferSize = 4096;
        /// <summary>
        /// 限制访问接收连接的线程数，用来控制最大并发数
        /// </summary>
        private Semaphore _maxNumberAcceptedClients = null;
        /// <summary>
        /// Socket连接池
        /// </summary>
        private SocketUserTokenPool _socketUserTokenPool = null;
        /// <summary>
        /// 在线客户端列表
        /// </summary>
        public SocketUserTokenList ConnectedClients { get; private set; }
        /// <summary>
        /// 此服务器上的默认数据通讯协议实例集合
        /// </summary>
        public ProtocolCollection<DefaultDataProtocol> DefaultInstances { get; private set; }
        /// <summary>
        /// 此服务器上的上传协议实例集合
        /// </summary>
        public ProtocolCollection<UploadSocketProtocol> UploadInstances { get; private set; }
        /// <summary>
        /// 此服务器上的下载协议实例集合
        /// </summary>
        public ProtocolCollection<DownloadSocketProtocol> DownloadInstances { get; private set; }

        private DaemonThread _daemonThread = null;
        /// <summary>
        /// 此Socket服务实例的配置
        /// </summary>
        public FlyingSocketConfig SocketConfig { get; private set; }
        /// <summary>
        /// Socket服务实例的名称
        /// </summary>
        public string Name { get { return SocketConfig == null ? "FlyingSocketServer" : SocketConfig.ServerName; } }

        /// <summary>
        /// 使用默认配置的Socket服务
        /// </summary>
        public FlyingSocketServer() : this(FlyingSocketConfig.GetConfig())
        {
        }

        /// <summary>
        /// 使用指定配置的Socket服务
        /// </summary>
        /// <param name="config"></param>
        public FlyingSocketServer(FlyingSocketConfig config)
        {
            this.SocketConfig = config;
            _socketUserTokenPool = new SocketUserTokenPool(SocketConfig.MaxConnections);
            ConnectedClients = new SocketUserTokenList();
            _maxNumberAcceptedClients = new Semaphore(SocketConfig.MaxConnections, SocketConfig.MaxConnections);
            DefaultInstances = new ProtocolCollection<DefaultDataProtocol>();
            DownloadInstances = new ProtocolCollection<DownloadSocketProtocol>();
            UploadInstances = new ProtocolCollection<UploadSocketProtocol>();
            Init();
        }

        private void Init()
        {
            for (int i = 0; i < SocketConfig.MaxConnections; i++) //按照连接数建立读写对象
            {
                var userToken = new SocketUserToken(SocketConfig.BufferSize);
                userToken.ReceiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                userToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                _socketUserTokenPool.Push(userToken);
            }
        }

        public event EventHandler<EventArgs> ServerStarted;
        /// <summary>
        /// Socket监听工作地址
        /// </summary>
        public IPEndPoint WorkingAddress { get; private set; }

        /// <summary>
        /// 监听端口，启动Socket服务
        /// </summary>
        public void Start()
        {
            WorkingAddress = new IPEndPoint(IPAddress.Parse("0.0.0.0"), SocketConfig.Port);
            _listenSocket = new Socket(WorkingAddress.AddressFamily, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            _listenSocket.Bind(WorkingAddress);
            _listenSocket.Listen(SocketConfig.MaxConnections);

            StartAccept(null);
            _daemonThread = new DaemonThread(this);
            Debug.WriteLine(string.Format("开始监听端口：{0}", WorkingAddress.ToString()));
            if (ServerStarted != null)
            {
                ServerStarted(this, EventArgs.Empty);
            }
        }

        public void Stop()
        {
            _listenSocket.Close();
        }

        /// <summary>
        /// g开始接受客户端连接
        /// </summary>
        /// <param name="acceptEventArgs"></param>
        public void StartAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs == null)
            {
                acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                acceptEventArgs.AcceptSocket = null; //释放上次绑定的Socket，等待下一个Socket连接
            }

            _maxNumberAcceptedClients.WaitOne(); //获取信号量
            bool willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArgs);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArgs);
            }
        }

        /// <summary>
        /// 客户端连接完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="acceptEventArgs"></param>
        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs acceptEventArgs)
        {
            ProcessAccept(acceptEventArgs);
        }

        public event EventHandler<UserTokenEventArgs> ClientConnected;

        /// <summary>
        /// 这里处理完表示一个成功建立1个客户端连接
        /// </summary>
        /// <param name="acceptEventArgs"></param>
        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            var remoteSocket = acceptEventArgs.AcceptSocket;
            if (remoteSocket == null || !remoteSocket.Connected) { return; }
            var userToken = _socketUserTokenPool.Pop();
            if (userToken == null)
            {
                throw new InvalidOperationException("当前已达最大可用连接数。");
            }
            ConnectedClients.Add(userToken); //添加到正在连接列表,同步操作
            userToken.ConnectSocket = remoteSocket;
            userToken.ConnectSocket.Blocking = false;                                   //同样使用异步非阻塞方式与客户端通讯
            userToken.ConnectSocket.SendBufferSize = SocketConfig.BufferSize;
            userToken.ConnectSocket.ReceiveBufferSize = SocketConfig.BufferSize;
            userToken.ConnectedTime = DateTime.Now;
            ClientConnected?.Invoke(this, new UserTokenEventArgs(userToken));
            try
            {
                bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                if (!willRaiseEvent)
                {
                    lock (userToken)
                    {
                        ProcessReceive(userToken.ReceiveEventArgs);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            StartAccept(acceptEventArgs); //把当前异步事件释放，等待下次连接
        }

        public event EventHandler<UserTokenEventArgs> ClientDisconnected;
        /// <summary>
        /// 一次Socket端口完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="asyncEventArgs"></param>
        private void IO_Completed(object sender, SocketAsyncEventArgs asyncEventArgs)
        {
            var userToken = asyncEventArgs.UserToken as SocketUserToken;
            if (userToken == null) { return; }
            userToken.ActiveTime = DateTime.Now;
            lock (userToken)
            {
                if (asyncEventArgs.LastOperation == SocketAsyncOperation.Receive)
                {
                    Console.WriteLine("server received...");
                    ProcessReceive(asyncEventArgs);
                }
                else if (asyncEventArgs.LastOperation == SocketAsyncOperation.Send)
                {
                    ProcessSend(asyncEventArgs);
                }
                else if (asyncEventArgs.LastOperation == SocketAsyncOperation.Disconnect)
                {
                    ClientDisconnected?.Invoke(this, new UserTokenEventArgs(userToken));
                }
                else
                {
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            var userToken = receiveEventArgs.UserToken as SocketUserToken;
            if (userToken.ConnectSocket == null) { return; }
            userToken.ActiveTime = DateTime.Now;
            if (userToken.ReceiveEventArgs.BytesTransferred > 0 && userToken.ReceiveEventArgs.SocketError == SocketError.Success)
            {
                var offset = userToken.ReceiveEventArgs.Offset;
                var receiveCount = userToken.ReceiveEventArgs.BytesTransferred;
                //存在Socket对象，并且没有绑定协议对象，则进行协议对象绑定
                if (userToken.SocketInvokeProtocol == null && userToken.ConnectSocket != null) 
                {
                    BuildingSocketInvokeProtocol(userToken);
                    offset = offset + 1;
                    receiveCount = receiveCount - 1;
                    if (userToken.SocketInvokeProtocol == null)
                    {
                        CloseClientConnection(userToken);       //如果没有解析出协议对象，提示非法连接并关闭连接
                    }

                    bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                    if (!willRaiseEvent)
                    {
                        ProcessReceive(userToken.ReceiveEventArgs);
                    }
                    return;
                }
                if (userToken.SocketInvokeProtocol == null)
                {
                    CloseClientConnection(userToken);       //如果没有解析出协议对象，提示非法连接并关闭连接
                    return;
                }
                else
                {
                    if (receiveCount > 0) //处理接收数据
                    {
                        //如果处理数据返回失败，则断开连接
                        var success = userToken.SocketInvokeProtocol.ProcessReceive(userToken.ReceiveEventArgs.Buffer, offset, receiveCount);
                        if (!success)
                        {
                            CloseClientConnection(userToken);
                        }
                        else
                        {
                            //否则投递下次接受数据请求
                            var willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                            if (!willRaiseEvent)
                            {
                                ProcessReceive(userToken.ReceiveEventArgs);
                            }
                        }
                    }
                    else
                    {
                        bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                        if (!willRaiseEvent)
                        {
                            ProcessReceive(userToken.ReceiveEventArgs);
                        }
                    }
                }
            }
            else
            {
                //没有接收到数据或接收错误时关闭连接。
                CloseClientConnection(userToken);
            }
        }

        public event EventHandler<ClientVerificationEventArgs> ClientAuthorization;

        /// <summary>
        /// 建立Socket连接后马上解析此客户端的通讯协议
        /// </summary>
        /// <param name="userToken">客户端用户对象</param>
        private void BuildingSocketInvokeProtocol(SocketUserToken userToken)
        {
            if (userToken.ReceiveEventArgs.BytesTransferred <= ProtocolCode.IntegerSize)
            {
                return;
            }
            if (ClientAuthorization == null)
            {
                throw new NotImplementedException("没有定义客户端认证实现。");
            }
            var fixedHeadCount = ProtocolCode.IntegerSize + ProtocolCode.IntegerSize;
            var commandText = Encoding.UTF8.GetString(userToken.ReceiveEventArgs.Buffer
                , userToken.ReceiveEventArgs.Offset + fixedHeadCount
                , userToken.ReceiveEventArgs.BytesTransferred - fixedHeadCount);
            var parser = new IncomingDataParser();
            var success = parser.DecodeProtocolText(commandText);
            if (!success) { return; }
            //在没有识别客户端通讯协议前不接受除Identify信息外的其它命令
            if (!string.Equals(parser.Command, CommandKeys.Login))
            {
                throw new NotSupportedException("在没有识别客户端前不能接受非身份认证外的其它指令数据。");
            }
            var typeValue = 0;
            if (!parser.GetValue(CommandKeys.Protocol, ref typeValue)) { return; }
            if (!Enum.IsDefined(typeof(SocketProtocolType), typeValue))
            {
                throw new InvalidOperationException(string.Format("未定义的通讯协议：{0}", typeValue.ToString()));
            }
            var protocolType = (SocketProtocolType)Enum.ToObject(typeof(SocketProtocolType), typeValue);
            if (protocolType == SocketProtocolType.Undefine)
            {
                return;
            }
            var instanceType = this.GetType().Assembly.GetTypes()
                .Where(t => t.IsClass)
                .Where(t => t.IsDefined(typeof(ProtocolNameAttribute)))
                .Where(t => t.GetCustomAttribute<ProtocolNameAttribute>().ProtocolType == protocolType).FirstOrDefault();
            if (instanceType == null)
            {
                throw new InvalidOperationException(string.Format("不受支持的通讯协议：{0}", typeValue.ToString()));
            }
            var param = new object[] { this, userToken };
            var element = Activator.CreateInstance(instanceType, param) as SocketInvokeElement;
            if (element == null)
            {
                throw new InvalidCastException(string.Format("协议类型{0}不是正确的Socket调用", instanceType.Name));
            }
            var usrName = "";
            var password = "";
            if (!parser.GetValue(CommandKeys.UserName, ref usrName)) { return; }
            if (!parser.GetValue(CommandKeys.Password, ref password)) { return; }
            var args = new ClientVerificationEventArgs() { ClientId = usrName, AuthCode = password };
            ClientAuthorization(this, args);
            if (!args.Success)
            {
                Debug.WriteLine(string.Format("客户端：{0}认证失败，将关闭此连接。远程地址：{1}", usrName, userToken.ConnectSocket.RemoteEndPoint.ToString()));
                CloseClientConnection(userToken);
                return;
            }
            userToken.Token = userToken.SessionId.MD5();
            userToken.ClientId = usrName;
            var assembler = new OutgoingDataAssembler();
            assembler.AddResponse();
            assembler.AddCommand(CommandKeys.Login);
            assembler.AddSuccess();
            assembler.AddValue(CommandKeys.SessionID, userToken.SessionId);
            assembler.AddValue(CommandKeys.Token, userToken.Token);
            var responseCommand = assembler.GetProtocolText();
            var responseData = Encoding.UTF8.GetBytes(responseCommand);

            var bufferManager = new DynamicBufferManager(SocketConfig.BufferSize);
            var totalLength = ProtocolCode.IntegerSize + responseData.Length; //获取总大小
            bufferManager.Clear();
            bufferManager.WriteInt(totalLength, false); //写入总大小
            bufferManager.WriteInt(responseData.Length, false); //写入命令大小
            bufferManager.WriteBuffer(responseData); //写入命令内容
            this.SendAsyncEvent(userToken.ConnectSocket, userToken.SendEventArgs, bufferManager.Buffer, 0, bufferManager.DataCount);
            userToken.SocketInvokeProtocol = element;
        }

        private bool ProcessSend(SocketAsyncEventArgs sendEventArgs)
        {
            SocketUserToken userToken = sendEventArgs.UserToken as SocketUserToken;
            if (userToken.SocketInvokeProtocol == null) { return false; }
            userToken.ActiveTime = DateTime.Now;
            if (sendEventArgs.SocketError == SocketError.Success)
            {
                return userToken.SocketInvokeProtocol.SendCompleted(); //调用子类回调函数
            }
            else
            {
                CloseClientConnection(userToken);
                return false;
            }
        }

        public bool SendAsyncEvent(Socket connectSocket, SocketAsyncEventArgs sendEventArgs, byte[] buffer, int offset, int count)
        {
            if (connectSocket == null)
            {
                return false;
            }
            sendEventArgs.SetBuffer(buffer, offset, count);
            bool willRaiseEvent = connectSocket.SendAsync(sendEventArgs);
            if (!willRaiseEvent)
            {
                return ProcessSend(sendEventArgs);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 完成后关闭Socket连接
        /// </summary>
        /// <param name="userToken"></param>
        public void CloseClientConnection(SocketUserToken userToken)
        {
            if (userToken.ConnectSocket == null) { return; }
            var socketInfo = string.Format("Local Address: {0} Remote Address: {1}", userToken.ConnectSocket.LocalEndPoint, userToken.ConnectSocket.RemoteEndPoint);
            try
            {
                //Shutdown方法客户端会接收到0字节的消息
                userToken.ConnectSocket.Shutdown(SocketShutdown.Both);
                userToken.ConnectSocket.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
            userToken.ConnectSocket.Dispose();
            userToken.ConnectSocket = null; //释放引用，并清理缓存，包括释放协议对象等资源
            _maxNumberAcceptedClients.Release();
            //重新放入池中以便下次复用
            _socketUserTokenPool.Push(userToken);
            ConnectedClients.Remove(userToken);
        }

        public void ClientMessageReceived(byte[] data)
        {
            var msg = Encoding.UTF8.GetString(data);
            Console.WriteLine(msg);
        }
    }

    public class UserTokenEventArgs : EventArgs
    {
        public SocketUserToken UserToken { get; private set; }

        public UserTokenEventArgs(SocketUserToken userToken) { this.UserToken = userToken; }
    }

    public class ClientVerificationEventArgs : EventArgs
    {
        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 认证码（密码）
        /// </summary>
        public string AuthCode { get; set; }
        /// <summary>
        /// 是否认证通过
        /// </summary>
        public bool Success { get; set; }
        ///// <summary>
        ///// 通过后的临时token值
        ///// </summary>
        //public string Token { get; set; }

    }
}
