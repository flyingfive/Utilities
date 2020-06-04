using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using FlyingFive.Windows.Service.Installation;

namespace FlyingFive.Windows.Service
{
    /// <summary>
    /// Windows服务工具
    /// </summary>
    public class WinServiceUtility
    {
        /// <summary>
        /// 检查Windows服务是否已安装
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool CheckServiceInstalled(string serviceName)
        {
            var existsService = ServiceController.GetServices().Where(s => s.ServiceName.Equals(serviceName, StringComparison.CurrentCultureIgnoreCase)).SingleOrDefault();
            return existsService != null;
        }

        /// <summary>
        /// 安装Windows服务
        /// </summary>
        /// <param name="exeFile">exe文件名全路径</param>
        /// <param name="svcName">服务名</param>
        /// <param name="displayName">显示名</param>
        /// <param name="description">描述</param>
        /// <param name="dependencies">服务依赖项名称，如果存在(多个用,分隔)</param>
        /// <returns></returns>
        public static bool InstallService(string exeFile, string svcName, string displayName, string description, string dependencies)
        {
            if (string.IsNullOrWhiteSpace(exeFile) || string.IsNullOrWhiteSpace(svcName)) { return false; }
            if (!File.Exists(exeFile)) { return false; }
            if (string.IsNullOrWhiteSpace(displayName)) { displayName = svcName; }
            var fileInfo = new FileInfo(exeFile);
            var serviceInstallInfo = new WindowsServiceInstallInfo(svcName, displayName, description, fileInfo.DirectoryName, fileInfo.Name, "Automatic");
            var installUtil = new WindowsServiceInstallUtil(serviceInstallInfo);
            var succeed = installUtil.Install(dependencies);
            return succeed;
        }

        /// <summary>
        /// 通过FlyingFive.dll代为安装Windows服务
        /// </summary>
        /// <param name="exeFile">实际的exe文件全路径</param>
        /// <param name="svcName">服务名</param>
        /// <param name="displayName">显示名</param>
        /// <param name="description">描述</param>
        /// <param name="dependencies">服务依赖项名称，如果存在(多个用,分隔)</param>
        /// <param name="startArgs">服务启动参数，如果存在(多个用空格分隔)</param>
        /// <returns></returns>
        public static bool InstallFlyingWindowsService(string exeFile, string svcName, string displayName, string description, string dependencies, string startArgs)
        {
            if (string.IsNullOrWhiteSpace(exeFile) || string.IsNullOrWhiteSpace(svcName)) { return false; }
            if (!File.Exists(exeFile)) { return false; }
            var dllFile = Path.Combine(Path.GetDirectoryName(exeFile), "FlyingFive.dll");
            if (!File.Exists(dllFile)) { return false; }
            if (string.IsNullOrWhiteSpace(displayName)) { displayName = svcName; }
            //var fileInfo = new FileInfo(exeFile);
            var serviceInstallInfo = new WindowsServiceInstallInfo(svcName, displayName, description, Path.GetDirectoryName(dllFile), dllFile, "Automatic");
            var installUtil = new WindowsServiceInstallUtil(serviceInstallInfo);
            var succeed = installUtil.InstallFlyingWinService(exeFile, dependencies, startArgs);
            return succeed;
        }

        /// <summary>
        /// 卸载Windows服务
        /// </summary>
        /// <param name="exeFile">exe文件名全路径</param>
        /// <param name="svcName">服务名称</param>
        /// <returns></returns>
        public static bool UninstallService(string exeFile, string svcName)
        {
            if (string.IsNullOrWhiteSpace(exeFile) || string.IsNullOrWhiteSpace(svcName)) { return false; }
            if (!File.Exists(exeFile)) { return false; }
            var fileInfo = new FileInfo(exeFile);
            var serviceInstallInfo = new WindowsServiceInstallInfo(svcName, svcName, "", fileInfo.DirectoryName, fileInfo.Name, "Automatic");
            var installUtil = new WindowsServiceInstallUtil(serviceInstallInfo);
            var succeed = installUtil.Uninstall();
            return succeed;
        }

        /// <summary>
        /// 启动Windows服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="pauseOnRetry">失败重试前是否暂停（5s)</param>
        /// <param name="retries">重试次数</param>
        /// <param name="timeout">尝试启动超时时间，单位：秒</param>
        public static void StartupService(string serviceName, bool pauseOnRetry = true, int retries = 3, int timeout = 30)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) { return; }
            if (retries < 0) { retries = 1; }
            var existsService = ServiceController.GetServices().Where(s => s.ServiceName.Equals(serviceName)).SingleOrDefault();
            if (existsService == null) { return; }
            if (existsService.Status == ServiceControllerStatus.Running) { return; }
            while (retries > 0)
            {
                if (existsService.Status == ServiceControllerStatus.Stopped)
                {
                    try
                    {
                        existsService.Start();
                        existsService.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, timeout));
                        if (existsService.Status == ServiceControllerStatus.Running)
                        {
                            return;
                        }
                    }
                    catch (System.ServiceProcess.TimeoutException ex)
                    {
                        //Logger.Error(string.Format("服务：{0}启动失败，5S后重试。", serviceName), ex);
                    }
                }
                if (pauseOnRetry) { Thread.Sleep(5000); }
                if (existsService.Status == ServiceControllerStatus.Running) { return; }
                retries--;
            }
        }

        /// <summary>
        /// 停止Windows服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="pauseOnRetry"></param>
        /// <param name="retries"></param>
        /// <param name="timeout"></param>
        public static void StopService(string serviceName, bool pauseOnRetry = true, int retries = 3, int timeout = 30)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) { return; }
            if (retries < 0) { retries = 1; }
            var existsService = ServiceController.GetServices().Where(s => s.ServiceName.Equals(serviceName)).SingleOrDefault();
            if (existsService == null) { return; }
            while (retries > 0)
            {
                if (existsService.Status == ServiceControllerStatus.Running)
                {
                    try
                    {
                        existsService.Stop();
                        existsService.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, timeout));
                        if (existsService.Status == ServiceControllerStatus.Stopped)
                        {
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Logger.Error(string.Format("服务：{0}停止失败，5S后重试。", serviceName), ex);
                    }
                }
                if (pauseOnRetry) { Thread.Sleep(5000); }
                if (existsService.Status == ServiceControllerStatus.Stopped) { return; }
                retries--;
            }
        }

        /// <summary>
        /// 强制关闭Windows服务，失败时杀死进程
        /// </summary>
        /// <param name="serviceName"></param>
        public static bool StopServiceMandatory(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) { return false; }
            var existsService = ServiceController.GetServices().Where(s => s.ServiceName.Equals(serviceName)).SingleOrDefault();
            if (existsService == null) { return false; }
            var exeFile = RegistryUtility.ReadRegistryValue(Microsoft.Win32.RegistryHive.LocalMachine, string.Format(@"SYSTEM\CurrentControlSet\Services\{0}", serviceName), "ImagePath", Microsoft.Win32.RegistryView.Default, "");
            var kill = new Func<bool>(() =>
            {
                if (string.IsNullOrWhiteSpace(exeFile)) { return false; }
                var processName = Path.GetFileNameWithoutExtension(exeFile);
                var processes = Process.GetProcessesByName(processName).Where(p => p.Id != Process.GetCurrentProcess().Id);
                foreach (var p in processes)
                {
                    try { p.Kill(); } catch (Exception e) { return false; }
                }
                return true;
            });
            if (existsService.Status == ServiceControllerStatus.Stopped) { return true; }
            if (existsService.Status == ServiceControllerStatus.Running)
            {
                try
                {
                    existsService.Stop();
                    existsService.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    if (existsService.Status == ServiceControllerStatus.Stopped)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return kill();
                }
            }
            return false;
        }

        /// <summary>
        /// 向Windows服务发送自定义命令
        /// </summary>
        /// <param name="command">命令标志。范围：128~256</param>
        /// <param name="serviceName">服务名称</param>
        /// <returns></returns>
        public static bool SendCustomCommand(int command, string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) { return false; }
            var existsService = ServiceController.GetServices().Where(s => s.ServiceName.Equals(serviceName)).SingleOrDefault();
            if (existsService == null) { return false; }
            if (command < 128 || command > 256) { throw new ArgumentException("命令值越界。"); }
            try { existsService.ExecuteCommand(command); return true; } catch (Exception e) { return false; }
        }
    }
}
