using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        /// 计算Hash值
        /// </summary>
        /// <param name="text"></param>
        /// <param name="name">Hash算法名称</param>
        /// <returns></returns>
        public static string ComputeHash(this string text, string name = "SHA512")
        {
            if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentException("参数：name错误"); }
            var supportedHashName = new string[] { "SHA256", "SHA512", "SHA384", "MD5", "HMACMD5", "HMACSHA256", "HMACSHA384", "HMACSHA512", "MACTripleDES", "RIPEMD160" };
            name = name.ToUpper();
            if (!supportedHashName.Contains(name)) { throw new NotSupportedException(string.Format("不支持的Hash算法：{0}", name)); }
            var computer = System.Security.Cryptography.HashAlgorithm.Create(name);
            if (computer == null) { throw new NotSupportedException(string.Format("不支持的Hash算法：{0}", name)); }
            var buffer = computer.ComputeHash(Encoding.UTF8.GetBytes(text));
            var hash = BitConverter.ToString(buffer).Replace("-", string.Empty);
            return hash;
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
        /// 字符串是否为真([1\Y\T\TRUE].PS:不区分大小写)
        /// </summary>
        /// <param name="str">字符串内容</param>
        /// <returns></returns>
        public static bool IsTrue(this string str)
        {
            if (string.IsNullOrEmpty(str)) { return false; }
            var flag = str.Equals("Y", StringComparison.CurrentCultureIgnoreCase) ||            //y or n
                str.Equals("T", StringComparison.CurrentCultureIgnoreCase) ||                   //t or f
                str.Equals("1", StringComparison.CurrentCultureIgnoreCase) ||                   //1 or 0
                str.Equals(bool.TrueString, StringComparison.CurrentCultureIgnoreCase);         //true or false
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
        /// <param name="text">字符串内容</param>
        /// <param name="format">日期格式</param>
        /// <returns></returns>
        public static bool IsDateTime(this string text, string format = "yyy-MM-dd HH:mm:ss")
        {
            if (string.IsNullOrWhiteSpace(text)) { return false; }
            var dt = DateTime.MinValue;
            var culture = System.Globalization.CultureInfo.GetCultureInfo("zh-CN");
            var flag = DateTime.TryParseExact(text, format, culture, System.Globalization.DateTimeStyles.AssumeLocal, out dt);
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

        /// <summary>
        /// 截取字符串左边指定长度
        /// </summary>
        /// <param name="text">字符串文本</param>
        /// <param name="length">截取长度</param>
        /// <returns></returns>
        public static string Left(this string text, int length)
        {
            if (string.IsNullOrEmpty(text) || length < 1) { return string.Empty; }
            if (text.Length < length) { return text; }
            return text.Substring(0, length);
        }
    }
}
