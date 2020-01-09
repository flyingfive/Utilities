using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FlyingSocket.Server
{
    /// <summary>
    /// 守护进程
    /// </summary>
    internal class DaemonThread
    {
        private Thread _thread = null;
        private FlyingSocketServer _socketServer = null;

        public DaemonThread(FlyingSocketServer asyncSocketServer)
        {
            FlyingFive.Data.UtilExceptions.CheckNull(asyncSocketServer);
            _socketServer = asyncSocketServer;
            _thread = new Thread(DaemonThreadStart);
            _thread.Start();
        }

        private void DaemonThreadStart()
        {
            while (_thread.IsAlive)
            {
                SocketUserToken[] userTokenArray = null;
                _socketServer.ConnectedClients.CopyList(ref userTokenArray);
                for (int i = 0; i < userTokenArray.Length; i++)
                {
                    if (!_thread.IsAlive)
                        break;
                    try
                    {
                        if ((DateTime.Now - userTokenArray[i].ActiveDateTime).Milliseconds > _socketServer.SocketConfig.Timeout) //超时Socket断开
                        {
                            lock (userTokenArray[i])
                            {
                                _socketServer.CloseClientConnection(userTokenArray[i]);
                            }
                        }
                    }
                    catch (Exception E)
                    {
                        throw E;
                        //Program.Logger.ErrorFormat("Daemon thread check timeout socket error, message: {0}", E.Message);
                        //Program.Logger.Error(E.StackTrace);
                    }
                }

                for (int i = 0; i < 60 * 1000 / 10; i++) //每分钟检测一次
                {
                    if (!_thread.IsAlive) { break; }
                    Thread.Sleep(10);
                }
            }
        }

        public void Close()
        {
            _thread.Abort();
            _thread.Join();
        }
    }
}
