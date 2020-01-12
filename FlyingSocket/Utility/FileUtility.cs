using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FlyingSocket.Utility
{
    public class FileUtility
    {
        /// <summary>
        /// 检查文件是否在使用中
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool CheckFileInUse(string fileName)
        {
            bool inUse = true;
            FileStream fs = null;
            try
            {
                try
                {
                    fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                    inUse = false;
                }
                catch
                {
                    inUse = true;
                }
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
            return inUse;
        }
    }

    public class MemoryUtility
    {

        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        private static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);

        /// <summary>
        /// 释放内存
        /// </summary>
        /// <param name="size_of_mb">最大内存数，超过此值进行释放内存</param>
        public static void FreeMemory(int size_of_mb = 50)
        {
            if (size_of_mb < 1) { size_of_mb = 50; }
            var proc = Process.GetCurrentProcess();
            var usedMemory = proc.PrivateMemorySize64;
            if (usedMemory > 1024 * 1024 * size_of_mb)          //控制一下内存
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
                }
            }
        }
    }
}
