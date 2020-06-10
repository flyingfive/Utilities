using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using Microsoft.Win32;

namespace FlyingFive.Windows.Service.Installation
{

    /*
     * 
     * >installutil.exe  /account="LocalSystem" /name="test_sevice" /displayname="测试服务" /desc="测试windows服务" /starttype="Automatic" "C:\Users\Administrator\source\repos\TestWinService\TestWinService\bin\Debug\FlyingFive.dll" /LogFile="C:\Users\Administrator\source\repos\TestWinService\TestWinService\bin\Debug\FlyingFive.dll.install.log" /dependencies="MSSQL$SQL2012,sxposflag10_cache_8e0603a0" /target="C:\Users\Administrator\source\repos\TestWinService\TestWinService\bin\Debug\TestWinService.exe" /args="aaa fff bbb"
        installutil.exe  
            /account="LocalSystem"
            /name="test_sevice" 
            /displayname="测试服务" 
            /desc="测试windows服务" 
            /starttype="Automatic" 
            "C:\Users\Administrator\source\repos\TestWinService\TestWinService\bin\Debug\FlyingFive.dll" 
            /LogFile="C:\Users\Administrator\source\repos\TestWinService\TestWinService\bin\Debug\FlyingFive.dll.install.log" 
            /dependencies="MSSQL$SQL2012,sxposflag10_cache_8e0603a0" 
            /target="C:\Users\Administrator\source\repos\TestWinService\TestWinService\bin\Debug\TestWinService.exe" 
            /args="aaa fff bbb"         
    */


    /// <summary>
    /// 根据参数指定Windows服务信息的动态安装器
    /// </summary>
    [RunInstaller(true)]
    public partial class DynamicInstaller : System.Configuration.Install.Installer
    {

        private ServiceProcessInstaller _processInstaller = null;
        private System.ServiceProcess.ServiceInstaller _serviceInstaller = null;

        public string ServiceName
        {
            get { return _serviceInstaller.ServiceName; }
            set { _serviceInstaller.ServiceName = value; }
        }
        public string DisplayName
        {
            get { return _serviceInstaller.DisplayName; }
            set { _serviceInstaller.DisplayName = value; }
        }
        public string Description
        {
            get { return _serviceInstaller.Description; }
            set { _serviceInstaller.Description = value; }
        }
        public ServiceStartMode StartType
        {
            get { return _serviceInstaller.StartType; }
            set { _serviceInstaller.StartType = value; }
        }
        public ServiceAccount Account
        {
            get { return _processInstaller.Account; }
            set { _processInstaller.Account = value; }
        }
        public string ServiceUsername
        {
            get { return _processInstaller.Username; }
            set { _processInstaller.Username = value; }
        }
        public string ServicePassword
        {
            get { return _processInstaller.Password; }
            set { _processInstaller.Password = value; }
        }
        private string _commandLine = string.Empty;
        private string[] _commandArgs = new string[0];

        public DynamicInstaller()
        {
            InitializeComponent();
            _processInstaller = new ServiceProcessInstaller();
            _processInstaller.Account = ServiceAccount.LocalSystem;
            _processInstaller.Username = null;
            _processInstaller.Password = null;
            _serviceInstaller = new System.ServiceProcess.ServiceInstaller();
            _serviceInstaller.StartType = ServiceStartMode.Automatic;

            _commandLine = Environment.CommandLine;
            WriteLog("Environment.CommandLine=" + _commandLine);
            _commandArgs = _commandLine.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToArray().Skip(1).Select(x => x.Trim()).ToArray();
            this.ServiceName = GetInstallArgument("name");
            this.DisplayName = GetInstallArgument("displayname");
            this.Description = GetInstallArgument("desc");

            var dependencies = GetInstallArgument("dependencies"); //服务依赖项
            if (!string.IsNullOrWhiteSpace(dependencies))
            {
                var dependencyServiceName = dependencies.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //过滤无效的服务名，大小写不敏感
                dependencyServiceName = dependencyServiceName.Where(name => WinServiceUtility.CheckServiceInstalled(name)).ToArray();
                if (dependencyServiceName.Length > 0)
                {
                    this._serviceInstaller.ServicesDependedOn = dependencyServiceName;
                }
            }

            Installers.AddRange(new Installer[] { _processInstaller, _serviceInstaller });
            this.AfterInstall += DynamicInstaller_AfterInstall;
        }

        private string GetInstallArgument(string argumentName)
        {
            if (!argumentName.EndsWith("=")) { argumentName = string.Concat(argumentName, "="); }
            var value = _commandArgs.Where(x => x.StartsWith(argumentName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (!string.IsNullOrEmpty(value)) { value = value.Substring(argumentName.Length); }
            if (value.StartsWith("\"")) { value = value.Substring(1); }
            if (value.EndsWith("\"")) { value = value.Substring(0, value.Length - 1); }
            return value;
        }

        private void DynamicInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            try
            {
                var registryPath = string.Format(@"SYSTEM\CurrentControlSet\services\{0}", this._serviceInstaller.ServiceName);
                var binFile = string.Empty;
                binFile = RegistryUtility.ReadRegistryValue(RegistryHive.LocalMachine, registryPath, "ImagePath");
                if (string.IsNullOrEmpty(binFile)) { return; }

                //通过注册表修改服务实际的启动文件和参数
                var target = GetInstallArgument("target=");
                var args = GetInstallArgument("args=");
                if (!string.IsNullOrWhiteSpace(target))// || !string.IsNullOrWhiteSpace(args))
                {
                    if (binFile.StartsWith("\"")) { binFile = binFile.Substring(1); }
                    if (binFile.EndsWith("\"")) { binFile = binFile.Substring(0, binFile.Length - 1); }
                    binFile = string.IsNullOrWhiteSpace(target) || !File.Exists(target) ? binFile : target;
                }
                if (!string.IsNullOrEmpty(args))
                {
                    args = string.Join(" ", args.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(c => string.Concat("/", c)));
                    binFile = string.Format("{0} {1}", binFile, args);
                }
                //var command = string.Format("{0} -{1}", binFile, args);
                RegistryUtility.WriteRegistryValue(RegistryHive.LocalMachine, registryPath, "ImagePath", binFile);
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString());
                throw new OperationCanceledException("安装发生错误。", ex);
            }
        }
        private void WriteLog(string text)
        {
#if DEBUG
            System.IO.File.AppendAllLines("C:\\win_service_install.log", new List<string>() { text });
#endif
        }

        /// <summary>
        /// 获取安装上下文环境中的指定参数
        /// </summary>
        /// <PARAM name="key">参数键</PARAM>
        /// <returns>参数值</returns>
        private string GetContextParameter(string key)
        {
            string result = string.Empty;
            if (this.Context.Parameters.ContainsKey(key) == true)
            {
                result = this.Context.Parameters[key];
            }
            return result;
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            base.OnBeforeInstall(savedState);

            var isUserAccount = false;

            string name = GetContextParameter("name").Trim();
            if (string.IsNullOrEmpty(name) == false)
            {
                _serviceInstaller.ServiceName = name;
            }
            string displayName = GetContextParameter("displayname").Trim();
            if (string.IsNullOrEmpty(displayName) == false)
            {
                _serviceInstaller.DisplayName = displayName;
            }
            string desc = GetContextParameter("desc").Trim();
            if (string.IsNullOrEmpty(desc) == false)
            {
                _serviceInstaller.Description = desc;
            }

            var startType = GetContextParameter("starttype");
            switch (startType.ToLower())
            {
                default:
                    break;
                case "automatic":
                    _serviceInstaller.StartType = ServiceStartMode.Automatic;
                    break;
                case "manual":
                    _serviceInstaller.StartType = ServiceStartMode.Manual;
                    break;
                case "disabled":
                    _serviceInstaller.StartType = ServiceStartMode.Disabled;
                    break;
            }

            var acct = GetContextParameter("account");
            switch (acct.ToLower())
            {
                default:
                    break;
                case "user":
                    _processInstaller.Account = ServiceAccount.User;
                    isUserAccount = true;
                    break;
                case "localservice":
                    _processInstaller.Account = ServiceAccount.LocalService;
                    break;
                case "localsystem":
                    _processInstaller.Account = ServiceAccount.LocalSystem;
                    break;
                case "networkservice":
                    _processInstaller.Account = ServiceAccount.NetworkService;
                    break;
            }

            var username = GetContextParameter("user").Trim();
            var password = GetContextParameter("password").Trim();

            if (isUserAccount)
            {
                if (!string.IsNullOrWhiteSpace(username))
                {
                    _processInstaller.Username = username;
                }
                if (!string.IsNullOrWhiteSpace(password))
                {
                    _processInstaller.Password = password;
                }
            }
        }

        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            base.OnBeforeUninstall(savedState);

            var name = GetContextParameter("name").Trim();
            if (string.IsNullOrEmpty(name) == false)
            {
                _serviceInstaller.ServiceName = name;
            }
        }
    }
}
