using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FlyingSocket.Server.Protocol;
using FlyingSocket.Core;
using System.Diagnostics;

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
        public LogOutputSocketProtocolMgr LogOutputSocketProtocolMgr { get; private set; }
        public UploadSocketProtocolMgr UploadSocketProtocolMgr { get; private set; }
        public DownloadSocketProtocolMgr DownloadSocketProtocolMgr { get; private set; }

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
            //SocketTimeOut = 5 * 60 * 1000;
            //_maxConnections = numConnections;
            //_receiveBufferSize = ProtocolConst.ReceiveBufferSize;
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

            LogOutputSocketProtocolMgr = new LogOutputSocketProtocolMgr();
            DownloadSocketProtocolMgr = new DownloadSocketProtocolMgr();
            UploadSocketProtocolMgr = new UploadSocketProtocolMgr();
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

        public IPEndPoint WorkingAddress { get; private set; }

        /// <summary>
        /// 监听端口，启动Socket服务
        /// </summary>
        public void Start()
        {
            WorkingAddress = new IPEndPoint(IPAddress.Parse("0.0.0.0"), SocketConfig.Port);
            _listenSocket = new Socket(WorkingAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(WorkingAddress);
            _listenSocket.Listen(SocketConfig.MaxConnections);
            //Program.Logger.InfoFormat("Start listen socket {0} success", localEndPoint.ToString());
            StartAccept(null);
            _daemonThread = new DaemonThread(this);
            Debug.WriteLine(string.Format("开始监听端口：{0}", WorkingAddress.ToString()));
            if (ServerStarted != null)
            {
                ServerStarted(this, EventArgs.Empty);
            }
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
            try
            {
                ProcessAccept(acceptEventArgs);
            }
            catch (Exception E)
            {
                throw E;
                //Program.Logger.ErrorFormat("Accept client {0} error, message: {1}", acceptEventArgs.AcceptSocket, E.Message);
                //Program.Logger.Error(E.StackTrace);  
            }
        }

        public event EventHandler<UserTokenEvent> OnClientConnected;

        /// <summary>
        /// 这里处理完表示一个成功建立1个客户端连接
        /// </summary>
        /// <param name="acceptEventArgs"></param>
        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            //Program.Logger.InfoFormat("Client connection accepted. Local Address: {0}, Remote Address: {1}",
            //    acceptEventArgs.AcceptSocket.LocalEndPoint, acceptEventArgs.AcceptSocket.RemoteEndPoint);

            var userToken = _socketUserTokenPool.Pop();
            ConnectedClients.Add(userToken); //添加到正在连接列表,同步操作
            userToken.ConnectSocket = acceptEventArgs.AcceptSocket;
            userToken.ConnectSocket.Blocking = false;
            userToken.ConnectSocket.SendBufferSize = 4096;
            userToken.ConnectSocket.ReceiveBufferSize = 4096;
            userToken.ConnectDateTime = DateTime.Now;
            OnClientConnected?.Invoke(this, new UserTokenEvent(userToken));
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
            catch (Exception E)
            {
                throw E;
                //Program.Logger.ErrorFormat("Accept client {0} error, message: {1}", userToken.ConnectSocket, E.Message);
                //Program.Logger.Error(E.StackTrace);                
            }
            StartAccept(acceptEventArgs); //把当前异步事件释放，等待下次连接
        }

        /// <summary>
        /// 一次Socket端口完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="asyncEventArgs"></param>
        private void IO_Completed(object sender, SocketAsyncEventArgs asyncEventArgs)
        {
            var userToken = asyncEventArgs.UserToken as SocketUserToken;
            userToken.ActiveDateTime = DateTime.Now;
            try
            {
                lock (userToken)
                {
                    if (asyncEventArgs.LastOperation == SocketAsyncOperation.Receive)
                    {
                        ProcessReceive(asyncEventArgs);
                    }
                    else if (asyncEventArgs.LastOperation == SocketAsyncOperation.Send)
                    {
                        ProcessSend(asyncEventArgs);
                    }
                    else if (asyncEventArgs.LastOperation == SocketAsyncOperation.Disconnect)
                    {
                    }
                    else
                    {
                        throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                    }
                }
            }
            catch (Exception E)
            {
                throw E;
                //Program.Logger.ErrorFormat("IO_Completed {0} error, message: {1}", userToken.ConnectSocket, E.Message);
                //Program.Logger.Error(E.StackTrace);
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            var userToken = receiveEventArgs.UserToken as SocketUserToken;
            if (userToken.ConnectSocket == null) { return; }
            userToken.ActiveDateTime = DateTime.Now;
            if (userToken.ReceiveEventArgs.BytesTransferred > 0 && userToken.ReceiveEventArgs.SocketError == SocketError.Success)
            {
                int offset = userToken.ReceiveEventArgs.Offset;
                int count = userToken.ReceiveEventArgs.BytesTransferred;
                if ((userToken.AsyncSocketInvokeElement == null) & (userToken.ConnectSocket != null)) //存在Socket对象，并且没有绑定协议对象，则进行协议对象绑定
                {
                    BuildingSocketInvokeElement(userToken);
                    offset = offset + 1;
                    count = count - 1;
                }
                if (userToken.AsyncSocketInvokeElement == null) //如果没有解析对象，提示非法连接并关闭连接
                {
                    //Program.Logger.WarnFormat("Illegal client connection. Local Address: {0}, Remote Address: {1}", userToken.ConnectSocket.LocalEndPoint, 
                    //    userToken.ConnectSocket.RemoteEndPoint);

                    //userToken.AsyncSocketInvokeElement = new UploadSocketProtocol(this, userToken);
                    //bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                    //if (!willRaiseEvent)
                    //{
                    //    ProcessReceive(userToken.ReceiveEventArgs);
                    //}
                    CloseClientConnection(userToken);
                }
                else
                {
                    if (count > 0) //处理接收数据
                    {
                        //如果处理数据返回失败，则断开连接
                        if (!userToken.AsyncSocketInvokeElement.ProcessReceive(userToken.ReceiveEventArgs.Buffer, offset, count))
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
                CloseClientConnection(userToken);
            }
        }

        private void BuildingSocketInvokeElement(SocketUserToken userToken)
        {
            byte flag = userToken.ReceiveEventArgs.Buffer[userToken.ReceiveEventArgs.Offset];
            if (flag == (byte)ProtocolFlag.Upload)
            {
                userToken.AsyncSocketInvokeElement = new UploadSocketProtocol(this, userToken);
            }
            else if (flag == (byte)ProtocolFlag.Download)
            {
                userToken.AsyncSocketInvokeElement = new DownloadSocketProtocol(this, userToken);
            }
            else if (flag == (byte)ProtocolFlag.RemoteStream)
            {
                userToken.AsyncSocketInvokeElement = new RemoteStreamSocketProtocol(this, userToken);
            }
            else if (flag == (byte)ProtocolFlag.Throughput)
            {
                userToken.AsyncSocketInvokeElement = new ThroughputSocketProtocol(this, userToken);
            }
            else if (flag == (byte)ProtocolFlag.Control)
            {
                userToken.AsyncSocketInvokeElement = new ControlSocketProtocol(this, userToken);
            }
            else if (flag == (byte)ProtocolFlag.LogOutput)
            {
                userToken.AsyncSocketInvokeElement = new LogOutputSocketProtocol(this, userToken);
            }
            if (userToken.AsyncSocketInvokeElement != null)
            {
                //Program.Logger.InfoFormat("Building socket invoke element {0}.Local Address: {1}, Remote Address: {2}",
                //    userToken.AsyncSocketInvokeElement, userToken.ConnectSocket.LocalEndPoint, userToken.ConnectSocket.RemoteEndPoint);
            }
        }

        private bool ProcessSend(SocketAsyncEventArgs sendEventArgs)
        {
            SocketUserToken userToken = sendEventArgs.UserToken as SocketUserToken;
            if (userToken.AsyncSocketInvokeElement == null) { return false; }
            userToken.ActiveDateTime = DateTime.Now;
            if (sendEventArgs.SocketError == SocketError.Success)
            {
                return userToken.AsyncSocketInvokeElement.SendCompleted(); //调用子类回调函数
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
                return false;
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
            //Program.Logger.InfoFormat("Client connection disconnected. {0}", socketInfo);
            try
            {
                //Shutdown方法客户端会接收到0字节的消息
                userToken.ConnectSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception E)
            {
                throw E;
                //Program.Logger.ErrorFormat("CloseClientSocket Disconnect client {0} error, message: {1}", socketInfo, E.Message);
            }
            userToken.ConnectSocket.Close();
            userToken.ConnectSocket.Dispose();
            userToken.ConnectSocket = null; //释放引用，并清理缓存，包括释放协议对象等资源

            _maxNumberAcceptedClients.Release();
            //重新放入池中以便下次复用
            _socketUserTokenPool.Push(userToken);
            ConnectedClients.Remove(userToken);
        }
    }

    public class UserTokenEvent : EventArgs
    {
        public SocketUserToken UserToken { get; private set; }

        public UserTokenEvent(SocketUserToken userToken) { this.UserToken = userToken; }
    }
}
