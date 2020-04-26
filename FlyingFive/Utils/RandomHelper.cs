using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Utils
{
    public class RandomHelper
    {
        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="requirePositive">是否要求正数值</param>
        /// <returns></returns>
        public static long RandomNumber(bool requirePositive = true)
        {
            using (var rand = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[16];
                rand.GetBytes(bytes);
                var number = BitConverter.ToInt64(bytes, 0);
                if (requirePositive)
                {
                    while (number < 0)
                    {
                        rand.GetBytes(bytes);
                        number = BitConverter.ToInt64(bytes, 0);
                    }
                }
                return number;
            }
        }

        /// <summary>
        /// 获取随机字符串
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            if (length < 2 || length > 128) { throw new ArgumentOutOfRangeException("参数length范围在2~128之间"); }
            var byteLength = length / 2;
            var text = "";
            using (var rand = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[byteLength];
                rand.GetBytes(bytes);
                text = BitConverter.ToString(bytes).Replace("-", "");
            }
            //要求奇数位长度
            if (length % 2 != 0)
            {
                var x = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                Func<int> getIndex = () =>
                {
                    var num = Convert.ToDouble(RandomNumber(true));
                    var n = 0;
                    while (num > int.MaxValue)
                    {
                        num = Math.Floor(Math.Sqrt(num));
                    }
                    n = Convert.ToInt32(num);
                    return n;
                };
                var idx = new Random(getIndex()).Next(0, x.Length - 1);
                text = string.Concat(text, x.Substring(idx, 1));
            }
            return text;
        }
    }
}
