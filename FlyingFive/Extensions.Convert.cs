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
        /// 类型转换(如果为null则转换为T类型的默认值)
        /// </summary>
        /// <typeparam name="T">要转换的目标类型</typeparam>
        /// <param name="convertibleValue">要转换的值</param>
        /// <param name="useDefaultValWhenFailure">转换失败时是否使用默认值</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns></returns>
        public static T TryConvert<T>(this IConvertible convertibleValue, bool useDefaultValWhenFailure = true, T defaultVal = default(T))
        {
            try
            {
                if (null == convertibleValue)
                {
                    if (useDefaultValWhenFailure) { return defaultVal; }
                    throw new InvalidCastException(string.Format("不能从null转换成类型：", typeof(T).FullName));
                }
                if (!typeof(T).IsGenericType)
                {
                    return (T)Convert.ChangeType(convertibleValue, typeof(T));
                }
                else
                {
                    Type genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(Nullable<>))
                    {
                        return (T)Convert.ChangeType(convertibleValue, Nullable.GetUnderlyingType(typeof(T)));
                    }
                }
            }
            catch (Exception ex)
            {
                if (useDefaultValWhenFailure) { return defaultVal; }
                throw new InvalidCastException("数据转换失败", ex);
            }
            throw new InvalidCastException(string.Format("类型无法无法从: \"{0}\" 转换为: \"{1}\".", convertibleValue.GetType().FullName, typeof(T).FullName));
        }

        /// <summary>
        /// 将任意值转换为指定类型(如果为null则转换为T类型的默认值)
        /// </summary>
        /// <typeparam name="T">转换的目标类型</typeparam>
        /// <param name="srcValue">任意源值</param>
        /// <param name="defaultVal">转换失败时的默认值</param>
        /// <returns></returns>
        public static T TryConvert<T>(this object srcValue, T defaultVal = default(T))
        {
            try
            {
                if (null == srcValue) { return defaultVal; }
                T destVal = (T)Convert.ChangeType(srcValue, typeof(T));
                return destVal;
            }
            catch
            {
                return defaultVal;
            }
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="srcValue">源值</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns></returns>
        public static object TryConvert(this object srcValue, Type destinationType)
        {
            if (srcValue == null) { return null; }
            var sourceType = srcValue.GetType();
            if (sourceType == destinationType) { return srcValue; }
            var destinationConverter = TypeDescriptor.GetConverter(destinationType);
            var sourceConverter = TypeDescriptor.GetConverter(sourceType);
            if (destinationConverter != null && destinationConverter.CanConvertFrom(sourceType))
            {
                if (destinationConverter.IsValid(srcValue))
                {
                    return destinationConverter.ConvertFrom(srcValue);
                }
                else
                {
                    if (sourceType == typeof(String) && destinationType == typeof(Boolean))
                    {
                        return srcValue.ToString().IsTrue();
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
