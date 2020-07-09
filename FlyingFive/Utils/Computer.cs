using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using FlyingFive.Windows;

namespace FlyingFive.Utils
{
    /// <summary>
    /// 表示计算机硬件相关信息
    /// </summary>
    public class Computer
    {
        /// <summary>
        /// CPU ID
        /// </summary>
        public string CPU_ID { get; private set; }
        /// <summary>
        /// MAC地址(如有多个则从win32 wmi 信息中获取第一个)
        /// </summary>
        public string MacAddress { get; private set; }
        /// <summary>
        /// 硬盘ID
        /// </summary>
        public string Disk_ID { get; private set; }
        /// <summary>
        /// IP地址(如有多个则从win32 wmi 信息中获取第一个)
        /// </summary>
        public string IpAddress { get; private set; }
        /// <summary>
        /// 登录操作系统的用户名
        /// </summary>
        public string LoginOperationUserName { get; private set; }
        /// <summary>
        /// 电脑名称
        /// </summary>
        public string ComputerName { get; private set; }
        /// <summary>
        /// 系统类型
        /// </summary>
        public string SystemType { get; private set; }
        /// <summary>
        /// 总物理内存,单位：M 
        /// </summary>
        public string TotalPhysicalMemory { get; private set; }

        private static Computer _instance = null;
        private static object _locker = new object();
        /// <summary>
        /// 获取单一实例
        /// </summary>
        public static Computer Instance
        {
            get
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        lock (_locker)
                        {
                            _instance = new Computer();
                        }
                    }
                }
                return _instance;
            }
        }

        private Computer()
        {
            CPU_ID = GetCpuID();
            MacAddress = GetMacAddress();
            Disk_ID = GetDisk_ID();
            IpAddress = GetIpAddress();
            LoginOperationUserName = GetOperationSystemLoginUserName();
            SystemType = GetSystemType();
            TotalPhysicalMemory = (long.Parse(GetTotalPhysicalMemory()) / 1024.0 / 1024).ToString();
            ComputerName = GetComputerName();
        }

        /// <summary>
        /// 刷新(重新获取)计算机硬件信息
        /// </summary>
        public void Refresh()
        {
            CPU_ID = GetCpuID();
            MacAddress = GetMacAddress();
            Disk_ID = GetDisk_ID();
            IpAddress = GetIpAddress();
            LoginOperationUserName = GetOperationSystemLoginUserName();
            SystemType = GetSystemType();
            TotalPhysicalMemory = (long.Parse(GetTotalPhysicalMemory()) / 1024.0).ToString();
            ComputerName = GetComputerName();
        }

        /// <summary>
        /// 检查端口是否被使用
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns></returns>
        public static bool CheckNetPortUsed(int port)
        {
            bool flag = false;
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();//获取所有的监听连接
            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        /// <summary>
        /// 获取CPUID
        /// </summary>
        /// <returns></returns>
        public static string GetCpuID()
        {
            //获取CPU序列号代码 
            string cpuInfo = "";//cpu序列号 
            using (ManagementClass mc = new ManagementClass("Win32_Processor"))
            {
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    var val = mo.Properties["ProcessorId"].Value;
                    cpuInfo = val == null ? string.Empty : val.ToString();
                    break;
                }
                moc.Dispose();
            }
            return cpuInfo;
        }

        /// <summary>
        /// 获取网卡的MAC地址
        /// </summary>
        /// <returns></returns>
        public static string GetMacAddress()
        {
            string mac = "";
            using (ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                        break;
                    }
                }
                moc.Dispose();
            }
            return mac;
        }

        /// <summary>
        /// 获取IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIpAddress()
        {
            string ip = "";
            using (ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        Array ar = (System.Array)(mo.Properties["IpAddress"].Value);
                        ip = ar.GetValue(0).ToString();
                        break;
                    }
                }
                moc.Dispose();
            }
            return ip;
        }

        /// <summary>
        /// 取固定安装的硬盘ID(如有多块则取第一个)
        /// </summary>
        /// <returns></returns>
        public static string GetDisk_ID()
        {
            string diskId = "";
            using (ManagementClass mc = new ManagementClass("Win32_DiskDrive"))
            {
                ManagementObjectCollection moc = mc.GetInstances();
                var firstDisk = moc.Cast<ManagementBaseObject>().Where(m =>
                {
                    var propertyData = m.Properties["DeviceID"];
                    var deviceId = propertyData != null && propertyData.Value != null ? propertyData.Value.ToString() : string.Empty;
                    propertyData = m.Properties["MediaType"];
                    var mediaType = propertyData != null && propertyData.Value != null ? propertyData.Value.ToString().Replace(" ", string.Empty).Replace("\t", string.Empty) : string.Empty;
                    return deviceId.EndsWith("PHYSICALDRIVE0", StringComparison.CurrentCultureIgnoreCase) && mediaType.Equals("FixedHardDiskMedia", StringComparison.CurrentCultureIgnoreCase);
                }).FirstOrDefault();
                var properties = firstDisk.Properties.Cast<PropertyData>().ToList();
                if (!properties.Any(x => x.Name.Equals("SerialNumber")))
                {
                    return string.Empty;
                }
                diskId = firstDisk.Properties["SerialNumber"].Value.ToString().Trim();
            }
            return diskId;
        }

        /// <summary>
        /// 操作系统的登录用户名
        /// </summary>
        /// <returns></returns>
        public static string GetOperationSystemLoginUserName()
        {
            return Environment.UserName;
        }


        /// <summary>
        /// PC类型
        /// </summary>
        /// <returns></returns>
        public static string GetSystemType()
        {
            string typeName = "";
            using (ManagementClass mc = new ManagementClass("Win32_ComputerSystem"))
            {
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    typeName = mo["SystemType"].ToString();
                    break;
                }
                moc.Dispose();
            }
            return typeName;
        }

        /// <summary>
        /// 物理内存
        /// </summary>
        /// <returns></returns> 
        public static string GetTotalPhysicalMemory()
        {
            string memory = "";
            using (ManagementClass mc = new ManagementClass("Win32_ComputerSystem"))
            {
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    memory = mo["TotalPhysicalMemory"].ToString();
                    break;
                }
                moc.Dispose();
            }
            return memory;
        }

        public static string GetMemoryAvailable()
        {
            var availablebytes = "";
            using (ManagementClass mos = new ManagementClass("Win32_OperatingSystem"))
            {
                foreach (ManagementObject mo in mos.GetInstances())
                {
                    if (mo["FreePhysicalMemory"] != null)
                    {
                        availablebytes = (1024.0 * long.Parse((mo["FreePhysicalMemory"].ToString()))).ToString();
                        break;
                    }
                }
            }
            return availablebytes;
        }


        /// <summary>
        /// 计算机名称
        /// </summary>
        /// <returns></returns>
        private static string GetComputerName()
        {
            return System.Environment.MachineName;
            //return System.Net.Dns.GetHostName();//System.Environment.GetEnvironmentVariable("ComputerName");
        }

        /// <summary>
        /// 检测电脑是否有ineternet连接
        /// </summary>
        /// <param name="domain">检查域名或IP地址</param>
        /// <param name="timeout">检查超时毫秒</param>
        /// <returns></returns>
        public static bool CheckInternetConnection(string domain = "wwww.baidu.com", int timeout = 3000)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(domain)) { domain = "www.baidu.com"; }
                if (timeout <= 0) { timeout = 3000; }
                using (var ping = new Ping())
                {
                    var reply = ping.Send(domain, timeout);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 释放内存压力
        /// </summary>
        /// <param name="usePagefile">是否转换内存数据到物理磁盘上的交换文件</param>
        public static void FreeMemory(bool usePagefile = false)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (usePagefile)
                {
                    Win32Api.SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
                }
            }
        }
    }
}
