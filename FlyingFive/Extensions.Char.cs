using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive
{
    /// <summary>
    /// 提供Char类型的扩展能力
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 指示字符是否中文字符
        /// </summary>
        /// <param name="ch">字符内容</param>
        /// <returns></returns>
        public static bool IsChinese(this char ch)
        {
            bool flag = System.Text.RegularExpressions.Regex.IsMatch(ch.ToString(), @"[\u4e00-\u9fbb]+$");
            return flag;
        }

        /// <summary>
        /// 指示字符是否双字节
        /// </summary>
        /// <param name="ch">字符内容</param>
        /// <returns></returns>
        public static bool IsDoubleByte(this char ch)
        {
            bool flag = System.Text.RegularExpressions.Regex.IsMatch(ch.ToString(), @"[^\x00-\xff]");
            return flag;
        }

        /// <summary>
        /// 字符全角转半角
        /// </summary>
        /// <param name="input">字符内容</param>
        /// <returns></returns>
        public static char ToDBCChar(this char input)
        {
            if (input == 12288)
            {
                input = (char)32;
            }
            if (input > 65280 && input < 65375)
                input = (char)(input - 65248);
            return input;
        }
    }
}
