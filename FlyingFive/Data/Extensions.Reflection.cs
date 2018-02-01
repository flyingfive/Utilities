using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data
{
    public static partial class Extensions
    {
        /// <summary>
        /// 获取成员(属性或字段)的数据类型
        /// </summary>
        /// <param name="propertyOrField"></param>
        /// <returns></returns>
        public static Type GetMemberType(this MemberInfo propertyOrField)
        {
            if (propertyOrField.MemberType == MemberTypes.Property)
                return ((PropertyInfo)propertyOrField).PropertyType;
            if (propertyOrField.MemberType == MemberTypes.Field)
                return ((FieldInfo)propertyOrField).FieldType;
            throw new ArgumentException("不支持字段或属性外的其它类型成员");
        }
    }
}
