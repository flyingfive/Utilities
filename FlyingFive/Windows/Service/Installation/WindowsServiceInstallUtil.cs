using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace FlyingFive.Windows.Service.Installation
{
    /// <summary>
    /// 
    /// </summary>
    public class WindowsServiceInstallUtil
    {
        /// <summary>
        /// .Net运行库路径
        /// </summary>
        public static string InstallUtilPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();

        protected WindowsServiceInstallInfo _wsInstallInfo = null;

        public WindowsServiceInstallUtil(WindowsServiceInstallInfo wsInstallInfo)
        {
            _wsInstallInfo = wsInstallInfo;
        }


        /// <summary>
        /// 执行InstallUtil.exe命令
        /// </summary>
        /// <param name="installUtilArguments">安装参数</param>
        /// <returns></returns>
        private static bool CallInstallUtil(string installUtilArguments)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = Path.Combine(InstallUtilPath, "InstallUtil.exe");
            proc.StartInfo.Arguments = installUtilArguments;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;

            proc.Start();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 安装具体服务
        /// </summary>
        /// <param name="dependencies">服务依赖项名称，如果存在(多个用,分隔)</param>
        /// <returns></returns>
        public bool Install(string dependencies)
        {
            return Install(_wsInstallInfo.InstallLogFile, dependencies);
        }

        /// <summary>
        /// 通过FlyingFive.dll代为安装Windows服务
        /// </summary>
        /// <param name="exeFile">实际的exe文件全路径</param>
        /// <param name="dependencies">服务依赖项名称，如果存在(多个用,分隔)</param>
        /// <param name="startArgs">服务启动参数，如果存在(多个用,分隔。key1=value1,key2=value2形式)</param>
        /// <returns></returns>
        internal bool InstallFlyingWinService(string exeFile, string dependencies = "", string startArgs = null)
        {
            var installUtilArguments = GenerateInstallutilInstallArgs(_wsInstallInfo.InstallLogFile, dependencies);
            //指定target参数
            installUtilArguments += " /target=\"" + exeFile + "\"";
            //指定服务启动参数
            if (!string.IsNullOrWhiteSpace(startArgs))
            {
                installUtilArguments += " /args=\"" + startArgs + "\"";
            }
            return CallInstallUtil(installUtilArguments);
        }

        /// <summary>
        /// 安装服务
        /// </summary>
        /// <param name="logFilePath">安装日志文件</param>
        /// <param name="dependencies">服务依赖项名称，如果存在(,分隔)</param>
        /// <returns></returns>
        public virtual bool Install(string logFilePath, string dependencies)
        {
            string installUtilArguments = GenerateInstallutilInstallArgs(logFilePath, dependencies);
            return CallInstallUtil(installUtilArguments);
        }

        protected string GenerateInstallutilInstallArgs(string logFilePath, string dependencies)
        {
            string installUtilArguments = " /account=\"" + _wsInstallInfo.WsAccountType + "\"";
            if (string.IsNullOrEmpty(_wsInstallInfo.WindowsServiceName) == false)
            {
                installUtilArguments += " /name=\"" + _wsInstallInfo.WindowsServiceName + "\"";
            }
            if (string.IsNullOrEmpty(_wsInstallInfo.WinSvcDisplayName) == false)
            {
                installUtilArguments += " /displayname=\"" + _wsInstallInfo.WinSvcDisplayName + "\"";
            }
            if (string.IsNullOrEmpty(_wsInstallInfo.Description) == false)
            {
                installUtilArguments += " /desc=\"" + _wsInstallInfo.Description + "\"";
            }
            if (string.IsNullOrEmpty(_wsInstallInfo.WsStartType) == false)
            {
                installUtilArguments += " /starttype=\"" + _wsInstallInfo.WsStartType + "\"";
            }
            if (_wsInstallInfo.WsAccountType == WindowsServiceAccountType.User)
            {
                installUtilArguments += " /user=\"" + _wsInstallInfo.WsAccountUserName + "\" /password=\"" + _wsInstallInfo.WsAccountPassword + "\"";
            }
            installUtilArguments += " \"" + Path.Combine(_wsInstallInfo.WindowsServicePath, _wsInstallInfo.WindowsServiceAssemblyName) + "\"";

            if (string.IsNullOrEmpty(logFilePath.Trim()) == false)
            {
                installUtilArguments += " /LogFile=\"" + logFilePath + "\"";
            }
            if (!string.IsNullOrWhiteSpace(dependencies))
            {
                installUtilArguments = string.Concat(installUtilArguments, string.Format(" /dependencies=\"{0}\"", dependencies));
            }
            return installUtilArguments;
        }

        /// <summary>
        /// 卸载windows服务
        /// </summary>
        /// <returns></returns>
        public bool Uninstall()
        {
            return Uninstall(string.Empty);
        }

        /// <summary>
        /// 卸载windows服务
        /// </summary>
        /// <param name="logFilePath">卸载日志文件</param>
        /// <returns></returns>
        public virtual bool Uninstall(string logFilePath)
        {
            string installUtilArguments = GenerateInstallutilUninstallArgs(logFilePath);
            return CallInstallUtil(installUtilArguments);
        }

        protected string GenerateInstallutilUninstallArgs(string logFilePath)
        {
            string installUtilArguments = " /u ";
            if (string.IsNullOrEmpty(_wsInstallInfo.WindowsServiceName) == false)
            {
                installUtilArguments += " /name=\"" + _wsInstallInfo.WindowsServiceName + "\"";
            }
            installUtilArguments += " \"" + Path.Combine(_wsInstallInfo.WindowsServicePath, _wsInstallInfo.WindowsServiceAssemblyName) + "\"";
            if (string.IsNullOrEmpty(logFilePath.Trim()) == false)
            {
                installUtilArguments += " /LogFile=\"" + logFilePath + "\"";
            }

            return installUtilArguments;
        }
    }

}
