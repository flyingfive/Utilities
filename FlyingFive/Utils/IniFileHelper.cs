using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FlyingFive.Utils
{
    /// <summary>
    /// ini文件处理辅助工具
    /// </summary>
    public class IniFileHelper
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string strSection, string strKey, string strVal, string strFilePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string strSection, string strKey, string strDef, StringBuilder retVal, int iSize, string strfilePath);


        //对于系统本地参数使用这两个函数读取，如果将来使用XML或其它类型取代Ini时只需要重写这两个函数.
        /// <summary>
        /// 写ini文件操作的函数,原GetProfileString函数.
        /// </summary>
        /// <param name="section">功能分组</param>
        /// <param name="key">属性</param>
        /// <param name="value">值</param>
        /// <param name="filePath">文件路径</param>
        public static long SetLocalSysParam(string section, string key, string value, string filePath)
        {
            return WritePrivateProfileString(section, key, value, filePath);
        }


        /// <summary>
        /// 读ini文件操作的函数,原ProflieString函数.
        /// </summary>
        /// <param name="section">功能分组</param>
        /// <param name="key">属性</param>
        /// <param name="def">默认值</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static string GetLocalSysParam(string section, string key, string def, string filePath)
        {
            if (!System.IO.File.Exists(filePath)) { return def; }
            StringBuilder param = new StringBuilder(Int16.MaxValue);
            int i = GetPrivateProfileString(section, key, def, param, Int16.MaxValue, filePath);
            return param.ToString();
        }
    }
}
