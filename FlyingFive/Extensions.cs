﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
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
        /// 判断值类型是否为空或默认值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static bool IsNullOrDefault<T>(this T? value) where T : struct
        {
            if (!value.HasValue) { return true; }
            bool flag = default(T).Equals(value.GetValueOrDefault());
            return flag;
        }

        /// <summary>
        /// 根据时间的有序GUID
        /// 注意：需要网卡，否则只能保证本机环境中唯一
        /// </summary>
        /// <param name="guid">GUID对象</param>
        /// <returns></returns>
        public static Guid SequentialGUID(this Guid guid)
        {
            if (guid == Guid.Empty) { guid = Guid.NewGuid(); }
            byte[] guidArray = guid.ToByteArray();
            var baseDate = new DateTime(1900, 1, 1);
            DateTime now = DateTime.Now;
            var days = new TimeSpan(now.Ticks - baseDate.Ticks);
            TimeSpan msecs = now.TimeOfDay;
            byte[] daysArray = BitConverter.GetBytes(days.Days);
            byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);
            Guid newId = new Guid(guidArray);
            return newId;
        }

        /// <summary> 
        /// 将小数值按指定的小数位数截断(非四舍五入)
        /// </summary> 
        /// <param name="d">要截断的小数</param> 
        /// <param name="precision">要截断的小数位数(大于等于0，小于等于28)</param> 
        /// <returns></returns> 
        public static decimal TruncateDec(this decimal d, int precision)
        {
            if (precision < 0 || precision > 28) { return d; }
            decimal sp = Convert.ToDecimal(Math.Pow(10, precision));
            if (d < 0)
            {
                return Math.Truncate(d) + Math.Ceiling((d - Math.Truncate(d)) * sp) / sp;
            }
            else
            {
                return Math.Truncate(d) + Math.Floor((d - Math.Truncate(d)) * sp) / sp;
            }
        }

        /// <summary>
        /// 传统四舍五入
        /// </summary>
        /// <param name="val"></param>
        /// <param name="decimals">保留小数位数</param>
        /// <returns></returns>
        public static decimal Round(this decimal val, int decimals)
        {
            return Math.Round(val, decimals, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 传统四舍五入
        /// </summary>
        /// <param name="val"></param>
        /// <param name="decimals">保留小数位数</param>
        /// <returns></returns>
        public static double Round(this double val, int decimals)
        {
            return Math.Round(val, decimals, MidpointRounding.AwayFromZero);
        }

        ///// <summary>
        ///// 字符串转换为decimal 
        ///// </summary>
        ///// <param name="obj">输入字符串</param>
        ///// <param name="defDec">转换失败时的默认值</param>
        ///// <returns></returns>
        //public static decimal TryDecimal(this object obj, decimal defDec = 0M)
        //{
        //    decimal decVal = 0M;
        //    if (obj == null || obj == DBNull.Value || obj.Equals(string.Empty))
        //        return defDec;
        //    obj = obj.ToString();
        //    if (decimal.TryParse(obj.ToString(), out decVal))
        //        return decVal;
        //    else
        //        return defDec;
        //}

        ///// <summary>
        ///// 任意对象转换成字符串
        ///// </summary>
        ///// <param name="obj">一个对象</param>
        ///// <param name="trimEmpty">是否去空格</param>
        ///// <returns></returns>
        //public static string TryString(this object obj, bool trimEmpty = false)
        //{
        //    if (obj == null || DBNull.Value.Equals(obj)) { return string.Empty; }
        //    var str = obj.ToString();
        //    if (trimEmpty) { str = str.Trim(); }
        //    return str;
        //}

        /// <summary>
        /// 获取枚举字段的Description描述标记说明
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum enumValue)
        {
            var attr = enumValue.GetCustomAttribute<DescriptionAttribute>();
            return attr == null ? string.Empty : attr.Description;
        }

        /// <summary>
        /// 获取枚举字段的特性标记
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static T GetEnumAttribute<T>(this Enum @enum) where T : System.Attribute
        {
            var field = @enum.GetType().GetField(@enum.ToString());
            T attr = field.GetCustomAttribute<T>(false);
            return attr;
        }
    }
}
