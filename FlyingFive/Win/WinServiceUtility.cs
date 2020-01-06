using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using FlyingFive.Win.ServiceInstaller;

namespace FlyingFive.Win
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
            var existsService = ServiceController.GetServices().Where(s => s.ServiceName.Equals(serviceName)).SingleOrDefault();
            return existsService != null;
        }

        /// <summary>
        /// 安装Windows服务
        /// </summary>
        /// <param name="svcName"></param>
        /// <param name="displayName"></param>
        /// <param name="description"></param>
        /// <param name="dependencies">服务依赖项名称，如果存在(多个用,符号分隔)</param>
        /// <returns></returns>
        public static bool InstallService(string svcName, string displayName, string description, string dependencies)
        {
            string fileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, AppDomain.CurrentDomain.SetupInformation.ApplicationName);
            var fileInfo = new FileInfo(fileName);
            var wsInstallInfo = new WindowsServiceInstallInfo(svcName, displayName, description, fileInfo.DirectoryName, fileInfo.Name, "Automatic");
            var wsInstallUtil = new WindowsServiceInstallUtil(wsInstallInfo);
            var isSucceed = wsInstallUtil.Install(dependencies);
            return isSucceed;
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
    }
}
