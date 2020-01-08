using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyingSocket.Server
{
    public class FlyingSocketFactory
    {
        private static object _syncObj = new object();
        private static FlyingSocketServer _defaultServer = null;
        /// <summary>
        /// 默认配置的Socket服务端
        /// </summary>
        public static FlyingSocketServer Default
        {
            get
            {
                if (_defaultServer == null)
                {
                    lock (_syncObj)
                    {
                        if (_defaultServer == null)
                        {
                            _defaultServer = new FlyingSocketServer();
                        }
                    }
                }
                return _defaultServer;
            }
        }
    }
}
