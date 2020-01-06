using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive
{
    /// <summary>
    /// 反射相关
    /// </summary>
    public static partial class Extensions
    {
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
        /// 获取枚举值的特性描述
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(this Enum @enum) where T : Attribute
        {
            var enumText = @enum.ToString();
            var field = @enum.GetType().GetField(enumText);
            var attribute = field.GetCustomAttribute<T>(false);
            return attribute;
        }

		/// <summary>
        /// 获取枚举值的Description描述说明
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetEnumDescription(this Enum enumValue)
        {
            var enumText = enumValue.ToString();
            var attribute = enumValue.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
            return attribute == null ? enumText : attribute.Description;
        }

    }
}
