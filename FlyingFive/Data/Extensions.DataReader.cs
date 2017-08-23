using FlyingFive.Data.Emit;
using FlyingFive.Data.Mapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data
{
    /// <summary>
    /// DataReader扩展
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 将DataReader中的数据转换为对象集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IList<T> ToList<T>(this IDataReader reader) where T : class,new()
        {
            var list = new List<T>();
            var properties = typeof(T).GetProperties().Where(p => p.CanWrite);
            while (reader.Read())
            {
                var obj = Activator.CreateInstance<T>();
                foreach (var prop in properties)
                {
                    var ordinal = reader.GetOrdinal(prop.Name);
                    var type = DynamicClassGenerator.CreateMemberMapperClass(prop);
                    var mapper = Activator.CreateInstance(type) as IMemberMapper;
                    if (mapper == null) { continue; }
                    mapper.Map(obj, reader, ordinal);
                }
                list.Add(obj);
            }
            return list;
        }

        /// <summary>
        /// DataReader方法集合
        /// </summary>
        public static class DataReaderMethods
        {
            /// <summary>
            /// 根据数据类型获取DataReader的指定方法
            /// </summary>
            /// <param name="dataType">要从dataReader得到的数据类型</param>
            /// <returns></returns>
            public static MethodInfo GetReaderMethod(Type dataType)
            {
                MethodInfo method = null;
                var isNullable = false;
                Type underlyingType = null;
                isNullable = dataType.IsNullable(out underlyingType);
                if (isNullable) { dataType = underlyingType; }
                var name = string.Empty;
                if (dataType.IsEnum)
                {
                    name = string.Format("Get{0}Enum", isNullable ? "Nullable" : string.Empty);
                    method = typeof(DataReaderMethods).GetMethod(name).MakeGenericMethod(dataType);
                }
                else
                {
                    var typeCode = Type.GetTypeCode(dataType);
                    switch (typeCode)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Double:
                        case TypeCode.Single:
                        case TypeCode.Decimal:
                        case TypeCode.Boolean:
                        case TypeCode.DateTime:
                        case TypeCode.Byte:
                        case TypeCode.Char:
                        case TypeCode.String:
                            name = string.Format("Get{0}{1}", isNullable ? "Nullable" : string.Empty, typeCode.ToString());
                            method = typeof(DataReaderMethods).GetMethod(name);
                            break;
                        default:
                            if (dataType == typeof(Guid))
                            {
                                name = string.Format("Get{0}Guid", isNullable ? "Nullable" : string.Empty);
                                method = typeof(DataReaderMethods).GetMethod(name);
                            }
                            else if (dataType == typeof(Object))
                            {
                                name = string.Format("GetValue");
                                method = typeof(DataReaderMethods).GetMethod(name);
                            }
                            else
                            {
                                name = string.Format("Get{0}TValue", isNullable ? "Nullable" : string.Empty);
                                method = typeof(DataReaderMethods).GetMethod(name).MakeGenericMethod(dataType);
                            }
                            break;
                    }
                }
                return method;
            }

            #region DataReader Methods
            public static short GetInt16(IDataReader reader, int ordinal)
            {
                return reader.GetInt16(ordinal);
            }

            public static short? GetNullableInt16(IDataReader reader, int ordinal)
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                return reader.GetInt16(ordinal);
            }

            public static int GetInt32(IDataReader reader, int ordinal)
            {
                return reader.GetInt32(ordinal);
            }

            public static int? GetNullableInt32(IDataReader reader, int ordinal)
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                return reader.GetInt32(ordinal);
            }

            public static long GetInt64(IDataReader reader, int ordinal)
            {
                return reader.GetInt64(ordinal);
            }

            public static long? GetNullableInt64(IDataReader reader, int ordinal)
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                return reader.GetInt64(ordinal);
            }

            public static decimal GetDecimal(IDataReader reader, int ordinal)
            {
                return reader.GetDecimal(ordinal);
            }

            public static decimal? GetNullableDecimal(IDataReader reader, int ordinal)
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                return reader.GetDecimal(ordinal);
            }

            public static double GetDouble(IDataReader reader, int ordinal)
            {
                return reader.GetDouble(ordinal);
            }

            public static double? GetNullableDouble(IDataReader reader, int ordinal)
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                return reader.GetDouble(ordinal);
            }

            public static float GetSingle(IDataReader reader, int ordinal)
            {
                return reader.GetFloat(ordinal);
            }

            public static float? GetNullableSingle(IDataReader reader, int ordinal)
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                return reader.GetFloat(ordinal);
            }

            public static bool GetBoolean(IDataReader reader, int ordinal)
            {
                return reader.GetBoolean(ordinal);
            }

            public static bool? GetNullableBoolean(IDataReader reader, int ordinal)
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                return reader.GetBoolean(ordinal);
            }

            public static DateTime GetDateTime(IDataReader reader, int ordinal)
            {
                return reader.GetDateTime(ordinal);
            }

            public static DateTime? GetNullableDateTime(IDataReader reader, int ordinal)
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                return reader.GetDateTime(ordinal);
            }

            public static Guid GetGuid(IDataReader reader, int ordinal)
            {
                return reader.GetGuid(ordinal);
            }

            public static Guid? GetNullableGuid(IDataReader reader, int ordinal)
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                return reader.GetGuid(ordinal);
            }

            public static byte GetByte(IDataReader reader, int ordinal)
            {
                return reader.GetByte(ordinal);
            }

            public static byte? GetNullableByte(IDataReader reader, int ordinal)
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                return reader.GetByte(ordinal);
            }

            public static char GetChar(IDataReader reader, int ordinal)
            {
                return reader.GetChar(ordinal);
            }

            public static char? GetNullableChar(IDataReader reader, int ordinal)
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                return reader.GetChar(ordinal);
            }

            public static string GetString(IDataReader reader, int ordinal)
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                return reader.GetString(ordinal);
            }

            public static object GetValue(IDataReader reader, int ordinal)
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                object val = reader.GetValue(ordinal);
                return val;
            }

            /// <summary>
            /// 从DataReader中获取枚举值
            /// </summary>
            /// <typeparam name="TEnum"></typeparam>
            /// <param name="reader"></param>
            /// <param name="ordinal"></param>
            /// <returns></returns>
            public static TEnum GetEnum<TEnum>(IDataReader reader, int ordinal) where TEnum : struct
            {
                Type fieldType = reader.GetFieldType(ordinal);

                object value;
                if (fieldType == typeof(short))
                    value = reader.GetInt16(ordinal);
                else
                    value = reader.GetInt32(ordinal);
                return (TEnum)Enum.ToObject(typeof(TEnum), value);
            }

            public static TEnum? GetNullableEnum<TEnum>(IDataReader reader, int ordinal) where TEnum : struct
            {
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }
                return GetEnum<TEnum>(reader, ordinal);
            }

            public static T GetTValue<T>(IDataReader reader, int ordinal)
            {
                object val = GetValue(reader, ordinal);
                try
                {
                    return (T)val;
                }
                catch (NullReferenceException ex)
                {
                    throw new InvalidCastException("The column value could not be null.");
                }
            }

            public static T? GetNullableTValue<T>(IDataReader reader, int ordinal) where T : struct
            {
                object val = reader.GetValue(ordinal);
                if (val == DBNull.Value)
                {
                    return null;
                }
                return new Nullable<T>((T)val);
            }
            #endregion
        }
    }
}
