using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FlyingFive.Windows.Service.Installation
{

    /// <summary>
    /// installer需要的相关配置信息，如服务名称，服务介绍，用户名，密码等等
    /// </summary>
    public class WindowsServiceInstallInfo
    {
        private string _windowsServiceName = null;
        private string _windowsServiceDisplayName = null;
        private string _wsDescription = null;
        private readonly string _windowsServicePath = null;
        private readonly string _windowsServiceAssemblyName = null;
        private readonly WindowsServiceAccountType _wsAccountType;
        private readonly string _wsAccountUserName = string.Empty;
        private readonly string _wsAccountPassword = string.Empty;
        private readonly string _wsStartType = "Automatic";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowsServiceName">windows服务名称</param>
        /// <param name="description">描述</param>
        /// <param name="windowsServicePath">服务文件路径</param>
        /// <param name="windowsServiceAssemblyName">服务文件名称</param>
        /// <param name="wsAccountType">启动身份类型</param>
        /// <param name="wsAccountUserName">启动账号名</param>
        /// <param name="wsAccountPassword">启动账号密码</param>
        /// <param name="wsStartType">启动类型</param>
        public WindowsServiceInstallInfo(string windowsServiceName, string displayName, string description, string windowsServicePath, string windowsServiceAssemblyName, WindowsServiceAccountType wsAccountType, string wsAccountUserName, string wsAccountPassword, string wsStartType)
        {
            _windowsServiceName = windowsServiceName.Trim();
            _windowsServiceDisplayName = displayName.Trim();
            _wsDescription = description.Trim();
            _windowsServicePath = windowsServicePath;
            _windowsServiceAssemblyName = windowsServiceAssemblyName;
            _wsAccountType = wsAccountType;
            _wsAccountUserName = wsAccountUserName;
            _wsAccountPassword = wsAccountPassword;
            _wsStartType = wsStartType;

            if (_wsAccountType == WindowsServiceAccountType.User && string.IsNullOrEmpty(_wsAccountUserName) == true)
            {
                throw new Exception("Username has to be provided if AccountType to start the windows service is USER");
            }
        }

        /// <summary>
        /// 卸载专用
        /// </summary>
        /// <param name="windowsServiceName"></param>
        public WindowsServiceInstallInfo(string windowsServiceName, string windowsServicePath, string windowsServiceAssemblyName)
        {
            _windowsServiceName = windowsServiceName.Trim();
            _windowsServiceName = windowsServiceName.Trim();
            _windowsServicePath = windowsServicePath;
            _windowsServiceAssemblyName = windowsServiceAssemblyName;
        }

        /// <summary>
        /// 常用配置(默认用户名和密码都为空)
        /// </summary>
        /// <param name="windowsServiceName">服务名</param>
        /// <param name="displayName">服务显示名</param>
        /// <param name="description">服务简介</param>
        /// <param name="windowsServicePath">服务所在文件夹路径</param>
        /// <param name="windowsServiceAssemblyName">服务对应exe文件名称</param>
        public WindowsServiceInstallInfo(string windowsServiceName, string displayName, string description, string windowsServicePath, string windowsServiceAssemblyName, string startType)
            : this(windowsServiceName, displayName, description, windowsServicePath, windowsServiceAssemblyName, WindowsServiceAccountType.LocalSystem, string.Empty, string.Empty, startType) { }

        /// <summary>
        /// 常用配置(可配置账户类型，用户名和密码)
        /// </summary>
        /// <param name="windowsServiceName"></param>
        /// <param name="displayName"></param>
        /// <param name="windowsServicePath"></param>
        /// <param name="windowsServiceAssemblyName"></param>
        /// <param name="accountType"></param>
        /// <param name="wsAccountUserName"></param>
        /// <param name="wsAccountPassword"></param>
        public WindowsServiceInstallInfo(string windowsServiceName, string displayName, string windowsServicePath, string windowsServiceAssemblyName, WindowsServiceAccountType accountType, string wsAccountUserName, string wsAccountPassword, string startType)
            : this(windowsServiceName, displayName, displayName, windowsServicePath, windowsServiceAssemblyName, accountType, wsAccountUserName, wsAccountPassword, startType) { }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string WindowsServiceName
        {
            get { return _windowsServiceName; }
            set { _windowsServiceName = value; }
        }

        /// <summary>
        /// 服务显示名称
        /// </summary>
        public string WinSvcDisplayName
        {
            get { return _windowsServiceDisplayName; }
            set { _windowsServiceDisplayName = value; }
        }

        /// <summary>
        /// 服务描述
        /// </summary>
        public string Description
        {
            get { return _wsDescription; }
            set { _wsDescription = value; }
        }

        /// <summary>
        /// 服务文件路径
        /// </summary>
        public string WindowsServicePath
        {
            get { return _windowsServicePath; }
        }
        /// <summary>
        /// 服务文件名称
        /// </summary>
        public string WindowsServiceAssemblyName
        {
            get { return _windowsServiceAssemblyName; }
        }
        /// <summary>
        /// 账户类型
        /// </summary>
        public WindowsServiceAccountType WsAccountType
        {
            get { return _wsAccountType; }
        }
        /// <summary>
        /// 启动账号名称
        /// </summary>
        public string WsAccountUserName
        {
            get { return _wsAccountUserName; }
        }
        /// <summary>
        /// 启动账号密码
        /// </summary>
        public string WsAccountPassword
        {
            get { return _wsAccountPassword; }
        }

        /// <summary>
        /// 启动类型
        /// </summary>
        public string WsStartType
        {
            get { return _wsStartType; }
        }

        /// <summary>
        /// 安装日志文件
        /// </summary>
        public string InstallLogFile
        {
            get
            {
                if (string.IsNullOrEmpty(WindowsServicePath) || string.IsNullOrEmpty(WindowsServiceAssemblyName))
                {
                    return Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "winService.install.log");
                }
                var logFile = Path.Combine(WindowsServicePath, WindowsServiceAssemblyName + ".install.log");
                return logFile;
            }
        }

    }

}
