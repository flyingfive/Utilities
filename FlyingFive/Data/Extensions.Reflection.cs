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

        /// <summary>
        /// 反射方式为对象成员赋值
        /// </summary>
        /// <param name="propertyOrField">属性或字段成员</param>
        /// <param name="obj">对象</param>
        /// <param name="value">更新的值</param>
        public static void SetMemberValue(this MemberInfo propertyOrField, object obj, object value)
        {
            if (propertyOrField.MemberType == MemberTypes.Property)
                ((PropertyInfo)propertyOrField).SetValue(obj, value, null);
            else if (propertyOrField.MemberType == MemberTypes.Field)
                ((FieldInfo)propertyOrField).SetValue(obj, value);
            throw new ArgumentException();
        }

        /// <summary>
        /// 反射方式获取对象成员上的值
        /// </summary>
        /// <param name="propertyOrField">属性或字段成员</param>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static object GetMemberValue(this MemberInfo propertyOrField, object obj)
        {
            if (propertyOrField.MemberType == MemberTypes.Property)
                return ((PropertyInfo)propertyOrField).GetValue(obj, null);
            else if (propertyOrField.MemberType == MemberTypes.Field)
                return ((FieldInfo)propertyOrField).GetValue(obj);
            throw new ArgumentException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyOrField"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MemberInfo AsReflectedMemberOf(this MemberInfo propertyOrField, Type type)
        {
            if (propertyOrField.ReflectedType != type)
            {
                MemberInfo tempMember = null;
                if (propertyOrField.MemberType == MemberTypes.Property)
                {
                    tempMember = type.GetProperty(propertyOrField.Name);
                }
                else if (propertyOrField.MemberType == MemberTypes.Field)
                {
                    tempMember = type.GetField(propertyOrField.Name);
                }

                if (tempMember != null)
                    propertyOrField = tempMember;
            }

            return propertyOrField;
        }
    }
}
