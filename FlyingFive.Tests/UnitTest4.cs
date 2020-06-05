using System;
using System.Linq;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using FlyingFive.Tests.Entities;
using System.Collections.Generic;
using FlyingFive.Data;
using FlyingFive.Utils;
using FlyingFive.Data.Drivers.SqlServer;
using System.Data;
using FlyingFive.Data.Interception;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using FlyingFive.DynamicProxy;
using System.Numerics;
using System.ComponentModel;
using FlyingFive.Comparing;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.ServiceProcess;
using System.Management;

namespace FlyingFive.Tests
{
    [TestClass]
    public class UnitTest4
    {
        //下面一种方法可以获取远程的MAC地址 
        [DllImport("Iphlpapi.dll")]
        static extern int SendARP(Int32 DestIP, Int32 SrcIP, ref Int64 MacAddr, ref Int32 PhyAddrLen);
        [DllImport("Ws2_32.dll")]
        static extern Int32 inet_addr(string ipaddr);

        public static string getMacAddr_Remote(string RemoteIP)
        {
            StringBuilder macAddress = new StringBuilder();
            try
            {
                Int32 remote = inet_addr(RemoteIP);
                Int64 macInfo = new Int64();
                Int32 length = 6;
                var aa = SendARP(remote, 0, ref macInfo, ref length);
                string temp = Convert.ToString(macInfo, 16).PadLeft(12, '0').ToUpper();
                int x = 12;
                for (int i = 0; i < 6; i++)
                {
                    if (i == 5)
                    {
                        macAddress.Append(temp.Substring(x - 2, 2));
                    }
                    else
                    {
                        macAddress.Append(temp.Substring(x - 2, 2) + "-");
                    }
                    x -= 2;
                }
                return macAddress.ToString();
            }
            catch
            {
                return macAddress.ToString();
            }
        }

        [TestMethod]
        public void TestGetMac()
        {
            var mac = getMacAddr_Remote("173.31.15.53");
        }

        [TestMethod]
        public void TestDisk()
        {
            var dx= DriveInfo.GetDrives();
            var root = Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory);
            var disk = AtapiDevice.GetHddInfo(0);
            var disk2 = AtapiDevice.GetHddInfo(1);
            var drivers = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed).ToList();
            //Environment.ExpandEnvironmentVariables("%systemdrive%");
            Environment.GetLogicalDrives();
            
        }
    }
}
