using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using NetFwTypeLib;

namespace FlyingFive.Windows
{
    /// <summary>
    /// Windows防火墙工具
    /// </summary>
    public partial class FirewallUtility
    {
        /// <summary>  
        /// 添加防火墙例外端口  
        /// </summary>  
        /// <param name="name">名称</param>  
        /// <param name="port">端口</param>  
        /// <param name="protocol">协议(TCP、UDP)</param>  
        public static void NetFwAddPorts(string name, string port, string protocol, string chname = "")
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(port)) { return; }
            if ("tcp,udp".IndexOf(protocol, StringComparison.CurrentCultureIgnoreCase) < 0)
            {
                throw new ArgumentException("参数:protocol只能为tcp或udp");
            }
            //创建firewall管理类的实例  
            var netFwMgr = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr")) as INetFwMgr;
            var openPort = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwOpenPort")) as INetFwOpenPort;
            openPort.Name = name;
            openPort.Port = Convert.ToInt32(port);
            openPort.Protocol = string.Equals(protocol, "TCP", StringComparison.CurrentCultureIgnoreCase) ? NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP : NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP;
            openPort.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL;
            openPort.Enabled = true;
            var exists = false;
            //加入到防火墙的管理策略  
            foreach (INetFwOpenPort mPort in netFwMgr.LocalPolicy.CurrentProfile.GloballyOpenPorts)
            {
                if (openPort.Port == mPort.Port)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                netFwMgr.LocalPolicy.CurrentProfile.GloballyOpenPorts.Add(openPort);
                string str = "netsh advfirewall firewall add rule name=" + (string.IsNullOrEmpty(chname) ? name : chname) + " dir=out action=allow protocol=TCP localport= " + port;
                CommandUtility.RunDosCommand(str);
            }
        }

        /// <summary>  
        /// 将应用程序添加到防火墙例外  
        /// </summary>  
        /// <param name="name">应用程序名称</param>  
        /// <param name="executablePath">应用程序可执行文件全路径</param>  
        public static void NetFwAddApps(string name, string executablePath)
        {
            //创建firewall管理类的实例  
            var netFwMgr = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr")) as INetFwMgr;
            var app = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwAuthorizedApplication")) as INetFwAuthorizedApplication;
            //在例外列表里，程序显示的名称  
            app.Name = name;
            //程序的路径及文件名  
            app.ProcessImageFileName = executablePath;
            //是否启用该规则  
            app.Enabled = true;
            //加入到防火墙的管理策略  
            netFwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(app);
            var exists = false;
            //加入到防火墙的管理策略  
            foreach (INetFwAuthorizedApplication mApp in netFwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications)
            {
                if (app == mApp)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                netFwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(app);
            }
        }

        /// <summary>  
        /// 删除防火墙例外端口  
        /// </summary>  
        /// <param name="port">端口</param>  
        /// <param name="protocol">协议（TCP、UDP）</param>  
        public static void NetFwDelApps(int port, string protocol)
        {
            if ("tcp,udp".IndexOf(protocol, StringComparison.CurrentCultureIgnoreCase) < 0)
            {
                throw new ArgumentException("参数:protocol只能为tcp或udp");
            }
            var netFwMgr = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr")) as INetFwMgr;
            if (string.Equals(protocol, "TCP", StringComparison.CurrentCultureIgnoreCase))
            {
                netFwMgr.LocalPolicy.CurrentProfile.GloballyOpenPorts.Remove(port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
            }
            else
            {
                netFwMgr.LocalPolicy.CurrentProfile.GloballyOpenPorts.Remove(port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP);
            }
        }

        /// <summary>  
        /// 删除防火墙例外中应用程序  
        /// </summary>  
        /// <param name="executablePath">程序的绝对路径</param>  
        public static void NetFwDelApps(string executablePath)
        {
            var netFwMgr = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr")) as INetFwMgr;
            netFwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Remove(executablePath);
        }
    }
}
