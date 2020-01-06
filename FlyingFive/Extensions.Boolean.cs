using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive
{
    public static partial class Extensions
    {
        /// <summary>
        /// 转换成位表示符
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static string AsBitString(this bool flag)
        {
            return flag ? "1" : "0";
        }
    }
}
