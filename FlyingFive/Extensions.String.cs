using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FlyingFive
{
    /// <summary>
    /// 提供.NET类型扩展行为
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns></returns>
        public static string MD5(this string text)
        {
            if (text == null) { return string.Empty; }
            byte[] encryptedBytes = Encoding.UTF8.GetBytes(text);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(encryptedBytes);
            var result = BitConverter.ToString(output).Replace("-", "");
            return result;
        }

        /// <summary>  
        /// 计算文件MD5值（大文件请合理调整bufferSize参数）
        /// </summary>  
        /// <param name="filePath">文件地址</param>  
        /// <param name="bufferSize">自定义缓冲区大小,单位：K，默认1M</param>  
        /// <returns>MD5Hash</returns>  
        public static string MD5File(this string filePath, int bufferSize = 1024)
        {
            if (!File.Exists(filePath)) { throw new ArgumentException(string.Format("文件<{0}>, 不存在", filePath)); }
            if (bufferSize <= 0) { bufferSize = 1024; }
            bufferSize = 1024 * bufferSize;             //自定义缓冲区大小
            byte[] buffer = new byte[bufferSize];
            using (Stream inputStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var hashAlgorithm = new MD5CryptoServiceProvider())
            {
                int readLength = 0;//每次读取长度  
                var output = new byte[bufferSize];
                while ((readLength = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //计算MD5  
                    hashAlgorithm.TransformBlock(buffer, 0, readLength, output, 0);
                }
                //完成最后计算，必须调用(由于上一部循环已经完成所有运算，所以调用此方法时后面的两个参数都为0)  
                hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
                string md5 = BitConverter.ToString(hashAlgorithm.Hash);
                hashAlgorithm.Clear();
                inputStream.Close();
                md5 = md5.Replace("-", "");
                return md5;
            }
        }

        /// <summary>
        /// 字符串转全角的函数(SBC case)
        /// </summary>
        /// <param name="input">字符串</param>
        /// <returns></returns>
        public static string ToSBC(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return string.Empty; }
            //半角转全角：
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }

        /// <summary>
        ///  字符串转半角的函数(SBC case)
        /// </summary>
        /// <param name="input">输入</param>
        /// <returns></returns>
        public static string ToDBC(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return string.Empty; }
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }

        /// <summary>
        /// 字符串是否包含中文
        /// </summary>
        /// <param name="text">字符串文本</param>
        /// <returns></returns>
        public static bool ContainsChinese(this string text)
        {
            if (string.IsNullOrEmpty(text)) { return false; }
            foreach (var item in text.ToCharArray())
            {
                if (item.IsChinese())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 指示字符串是否包括双字节字符
        /// </summary>
        /// <param name="text">字符串文本</param>
        /// <returns></returns>
        public static bool ContainsDoubleByte(this string text)
        {
            if (string.IsNullOrEmpty(text)) { return false; }
            foreach (var ch in text.ToCharArray())
            {
                if (ch.IsDoubleByte())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 指示字符串是能转换成整数
        /// </summary>
        /// <param name="text">字符串内容</param>
        /// <param name="allowMinus">是否允许负数</param>
        /// <returns></returns>
        public static bool IsInt(this string text, bool allowMinus)
        {
            if (string.IsNullOrEmpty(text)) { return false; }
            var flag = false;
            if (allowMinus)
            {
                flag = System.Text.RegularExpressions.Regex.IsMatch(text, @"^-?\d+$");
            }
            else
            {
                flag = System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+$");
            }
            return flag;
        }

        /// <summary>
        /// 指示字符串是能转换成Decimal
        /// </summary>
        /// <param name="text">字符串内容</param>
        /// <param name="allowMinus">是否允许负数</param>
        /// <returns></returns>
        public static bool IsDecimal(this string text, bool allowMinus)
        {
            if (string.IsNullOrEmpty(text)) { return false; }
            var flag = false;
            if (allowMinus)
            {
                flag = System.Text.RegularExpressions.Regex.IsMatch(text, @"^(-?\d+)(\.\d+)?$");
            }
            else
            {
                flag = System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+(\.\d+)?$");
            }
            return flag;
        }

        /// <summary>
        /// 指示字符串是否html内容
        /// </summary>
        /// <param name="text">字符串内容</param>
        /// <returns></returns>
        public static bool IsHtml(this string text)
        {
            if (string.IsNullOrEmpty(text)) { return false; }
            var flag = System.Text.RegularExpressions.Regex.IsMatch(text, @"^<(.*)>.*<\/\1>|<(.*) \/>$");
            return flag;
        }

        /// <summary>
        /// 指示字符串内容是否邮箱地址
        /// </summary>
        /// <param name="text">字符串内容</param>
        /// <returns></returns>
        public static bool IsEmail(this string text)
        {
            if (string.IsNullOrEmpty(text)) { return false; }
            var flag = System.Text.RegularExpressions.Regex.IsMatch(text, @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$");
            return flag;
        }

        /// <summary>
        /// 字符串是否为真([1\Y\true].PS:不区分大小写)
        /// </summary>
        /// <param name="str">字符串内容</param>
        /// <returns></returns>
        public static bool IsTrue(this string str)
        {
            if (string.IsNullOrEmpty(str)) { return false; }
            if (Boolean.TrueString.Equals(str, StringComparison.CurrentCultureIgnoreCase)) { return true; }
            if (Boolean.FalseString.Equals(str, StringComparison.CurrentCultureIgnoreCase)) { return false; }
            var flag = str.Equals("y", StringComparison.CurrentCultureIgnoreCase) ||
                str.Equals("1", StringComparison.CurrentCultureIgnoreCase);
            return flag;
        }

        /// <summary>
        /// 字符串处理
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TrimEmpty(this string str)
        {
            if (string.IsNullOrEmpty(str)) { return string.Empty; }
            return str.Trim();
        }

        /// <summary>
        /// 判断字符串是否日期类型
        /// </summary>
        /// <param name="str">字符串内容</param>
        /// <returns></returns>
        public static bool IsDateTime(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) { return false; }
            var dt = DateTime.MinValue;
            var flag = DateTime.TryParse(str, out dt);
            return flag;
        }

        /// <summary>
        /// 获取Url参数
        /// </summary>
        /// <param name="url">url地址</param>
        /// <returns></returns>
        public static NameValueCollection GetUrlArguments(this string url)
        {
            return new Uri(url).GetUrlArguments();
        }

    }
}
