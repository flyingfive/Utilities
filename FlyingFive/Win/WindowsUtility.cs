using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace FlyingFive.Win
{
    /// <summary>
    /// windows工具
    /// </summary>
    public class WindowsUtility
    {
        /// <summary>
        /// 运行Dos命令
        /// </summary>
        /// <param name="commandText">命令文本</param>
        public static void RunDosCommand(string commandText)
        {
            var start = new ProcessStartInfo();
            start.FileName = "cmd.exe";
            start.UseShellExecute = false;
            start.RedirectStandardInput = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.CreateNoWindow = true;
            var process = Process.Start(start);
            process.StandardInput.WriteLine(commandText);
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
            process.Close();
        }

        /// <summary>
        /// 批量执行执行批处理命令
        /// </summary>
        /// <param name="commands">命令合集</param>
        /// <param name="autoExit">执行完命令合集后是否退出</param>
        public static void RunBats(string[] commands, bool autoExit = true)
        {
            if (commands == null || commands.Length < 0) { return; }
            var text = string.Join(Environment.NewLine, commands);
            if (autoExit) { text = string.Concat(text, Environment.NewLine, "exit"); }
            var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            if (!Directory.Exists(baseDir)) { Directory.CreateDirectory(baseDir); }
            var fileName = Path.Combine(baseDir, "batch.bat");
            System.IO.File.WriteAllText(fileName, text, Encoding.ASCII);
            var start = new ProcessStartInfo("cmd.exe") { WorkingDirectory = baseDir, FileName = "batch.bat", CreateNoWindow = true, UseShellExecute = false };
            Process.Start(start);
        }

        /// <summary>
        /// 检测电脑是否有inetert连接
        /// </summary>
        /// <returns></returns>
        public static bool CheckInternet()
        {
            try
            {
                var timeout = 3000;
                using (var ping = new Ping())
                {
                    var reply = ping.Send("www.baidu.com", timeout);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
