using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyingSocket.Server
{
    /// <summary>
    /// socket服务端配置
    /// </summary>
    public sealed class FlyingSocketConfig : ConfigurationSection
    {
        public FlyingSocketConfig() { }

        /// <summary>
        /// 获取默认配置
        /// </summary>
        /// <returns></returns>
        public static FlyingSocketConfig GetConfig()
        {
            return GetConfig("SocketServer");
        }

        public static FlyingSocketConfig GetConfig(string sectionName)
        {
            var section = (FlyingSocketConfig)ConfigurationManager.GetSection(sectionName);
            if (section == null)
                return new FlyingSocketConfig();
            return section;
        }
        /// <summary>
        /// 服务名称
        /// </summary>
        [ConfigurationProperty("ServerName", IsRequired = false, DefaultValue = "FlyingSocketServer")]
        public string ServerName { get { return base["ServerName"].ToString(); } set { base["ServerName"] = value; } }

        /// <summary>
        /// 服务端口
        /// </summary>
        [ConfigurationProperty("Port", IsRequired = true, DefaultValue = 52520)]
        public int Port { get { return (int)base["Port"]; } set { base["Port"] = value; } }

        /// <summary>
        /// 最大连接数限制，默认5000
        /// </summary>
        [ConfigurationProperty("MaxConnections", IsRequired = false, DefaultValue = 5000)]
        public int MaxConnections { get { return (int)base["MaxConnections"]; } set { base["MaxConnections"] = value; } }

        /// <summary>
        /// 接收和发送缓冲区大小，默认4096
        /// </summary>
        [ConfigurationProperty("BufferSize", IsRequired = false, DefaultValue = 1024 * 4)]
        public int BufferSize { get { return (int)base["BufferSize"]; } set { base["BufferSize"] = value; } }

        /// <summary>
        /// 超时时间（单位毫秒，默认60S）
        /// </summary>
        [ConfigurationProperty("Timeout", IsRequired = false, DefaultValue = 60 * 1000)]
        public int Timeout { get { return (int)base["Timeout"]; } set { base["Timeout"] = value; } }
    }
}
