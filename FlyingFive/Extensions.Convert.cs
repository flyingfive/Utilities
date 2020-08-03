using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FlyingFive
{
    /// <summary>
    /// 提供对象的类型转换功能
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 安全类型转换，失败时使用给定的默认值
        /// </summary>
        /// <typeparam name="T">转换目标类型</typeparam>
        /// <param name="srcValue">源值</param>
        /// <param name="defaultValue">失败时使用的默认值</param>
        /// <returns></returns>
        public static T TryConvertSafety<T>(this object srcValue, T defaultValue = default(T))
        {
            if (srcValue == null || DBNull.Value.Equals(srcValue))
            {
                return defaultValue;
            }
            try
            {
                T destValue = (T)TryConvert(srcValue, typeof(T));
                return destValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 安全类型转换，返回值包装为Object，失败时使用给定的默认值
        /// </summary>
        /// <param name="srcValue">源值</param>
        /// <param name="destnationType">转换目标类型</param>
        /// <param name="defaultVal">失败时使用的默认值</param>
        /// <returns></returns>
        public static object TryConvertSafety(this object srcValue, Type destnationType, object defaultVal = null)
        {
            try
            {
                return TryConvert(srcValue, destnationType);
            }
            catch
            {
                return defaultVal;
            }
        }

        /// <summary>
        /// 指定类型转换
        /// </summary>
        /// <typeparam name="T">转换目标类型</typeparam>
        /// <param name="srcValue">源值</param>
        /// <returns></returns>
        public static T TryConvert<T>(this object srcValue)
        {
            T destValue = (T)TryConvert(srcValue, typeof(T));
            return destValue;
        }


        /// <summary>
        /// 类型转换，包装为object
        /// </summary>
        /// <param name="srcValue">源值</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns></returns>
        public static object TryConvert(this object srcValue, Type destinationType)
        {
            if (srcValue == null)
            {
                if (destinationType.IsValueType)
                {
                    throw new InvalidCastException(string.Format("null不能转换为值类型：{0}", destinationType.FullName));
                }
                return null;
            }
            var sourceType = srcValue.GetType();
            if (sourceType == destinationType) { return srcValue; }
            var destinationConverter = TypeDescriptor.GetConverter(destinationType);
            var sourceConverter = TypeDescriptor.GetConverter(sourceType);
            if (destinationType.IsNullableType())
            {
                var underlyingType = destinationType.GetUnderlyingType();
                if (sourceType == underlyingType) { return srcValue; }
                var underlyingTypeConverter = TypeDescriptor.GetConverter(underlyingType);
                if (underlyingTypeConverter != null && underlyingTypeConverter.IsValid(srcValue))
                {
                    return underlyingTypeConverter.ConvertFrom(srcValue);
                }
                if (sourceType == typeof(String) && underlyingType == typeof(Boolean))
                {
                    return srcValue.ToString().IsTrue();
                }
            }
            if (destinationConverter != null && destinationConverter.CanConvertFrom(sourceType))
            {
                if (destinationConverter.IsValid(srcValue))
                {
                    return destinationConverter.ConvertFrom(srcValue);
                }
                else
                {
                    if (sourceType == typeof(String))
                    {
                        if (destinationType == typeof(Boolean))
                        {
                            return srcValue.ToString().IsTrue();
                        }
                        if (destinationType.IsNumericType())
                        {
                            return srcValue.ToString().ToNumeric();
                        }
                    }
                    throw new InvalidCastException(string.Format("值:{0}不能从类型{1}转换为:{2}", srcValue.ToString(), sourceType.FullName, destinationType.FullName));
                }
            }
            if (sourceConverter != null && sourceConverter.CanConvertTo(destinationType))
            {
                var result = sourceConverter.ConvertTo(null, CultureInfo.CurrentCulture, srcValue, destinationType);
                return result;
            }
            if (destinationType.IsEnum)
            {
                if (srcValue is string)
                {
                    var result = Enum.Parse(destinationType, srcValue.ToString());
                    return result;
                }
                if (srcValue is int)
                {
                    var result = Enum.ToObject(destinationType, (int)srcValue);
                    return result;
                }
            }
            if (!destinationType.IsAssignableFrom(sourceType))
            {
                if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    if (Convert.IsDBNull(srcValue))
                    {
                        return null;
                    }
                    else
                    {
                        destinationType = destinationType.GetGenericArguments()[0];
                        destinationConverter = TypeDescriptor.GetConverter(destinationType);
                        if (destinationConverter != null && destinationConverter.CanConvertFrom(sourceType))
                        {
                            var target = destinationConverter.ConvertFrom(null, CultureInfo.CurrentCulture, srcValue);
                            return target;
                        }
                        if (sourceConverter != null && sourceConverter.CanConvertTo(destinationType))
                        {
                            var target = sourceConverter.ConvertTo(null, CultureInfo.CurrentCulture, srcValue, destinationType);
                            return target;
                        }
                    }
                }
                if (destinationType.IsArray)
                {
                    if (srcValue == DBNull.Value) { return null; }
                }
                var result = Convert.ChangeType(srcValue, destinationType);
                return result;
            }
            return srcValue;
        }
    }
}
