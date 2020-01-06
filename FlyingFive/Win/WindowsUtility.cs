using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;
            var process = Process.Start(processStartInfo);
            process.StandardInput.WriteLine(commandText);
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
            process.Close();
        }
    }
}
