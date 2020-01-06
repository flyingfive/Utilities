using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
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
        /// 获取自定义特性
        /// </summary>
        /// <typeparam name="T">特性类型</typeparam>
        /// <param name="member">成员对象</param>
        /// <param name="inherit">是否从继承关系中查找</param>
        /// <param name="throwExceptionOnUndefined">没有找到时是否抛出异常</param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(this MemberInfo member, bool inherit, bool throwExceptionOnUndefined = false) where T : Attribute
        {
            T attribute = default(T);
            var atts = member.GetCustomAttributes(typeof(T), inherit);
            if (atts.Length == 0)
            {
                if (throwExceptionOnUndefined)
                {
                    throw new ArgumentException(string.Format("类型{0}的成员:{1}上没有找到名称为:{2}的标记"
                        , member.DeclaringType.FullName
                        , member.Name
                        , typeof(T).FullName));
                }
                return null;
            }
            attribute = atts[0] as T;
            if (attribute == null && throwExceptionOnUndefined)
            {
                throw new ArgumentException(string.Format("类型{0}的成员:{1}上没有找到名称为:{2}的标记"
                    , member.DeclaringType.FullName
                    , member.Name
                    , typeof(T).FullName));
            }
            return attribute;
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
    }
}
