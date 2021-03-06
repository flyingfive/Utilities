﻿using FlyingFive.Data.Dynamic;
using FlyingFive.Data.Mapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="reader">dataReader数据读取器</param>
        /// <returns></returns>
        public static IEnumerable<T> AsEnumerable<T>(this IDataReader reader) where T : class, new()
        {
            //var fakeReader = reader as Fakes.FakeDataReader;
            //if (fakeReader == null)
            //{
            //    fakeReader = new Fakes.FakeDataReader(reader);
            //}
            var mappings = typeof(T).GetProperties().Where(p => p.CanWrite && p.PropertyType.IsSystemType())
                .Select(prop => new MappingData() { Ordinal = reader.GetOrdinal(prop.Name), Property = prop, Mapper = MemberMapperHelper.CreateMemberMapper(prop) });
            while (reader.Read())
            {
                var obj = Activator.CreateInstance<T>();
                MapData(obj, reader, mappings);
                yield return obj;
            }
        }

        /// <summary>
        /// 将DataReader中的数据转换为对象集合
        /// </summary>
        /// <param name="reader">dataReader数据读取器</param>
        /// <param name="dataType">数据类型</param>
        /// <returns></returns>
        public static IEnumerable<object> AsEnumerable(this IDataReader reader, Type dataType)
        {
            //var fakeReader = reader as Fakes.FakeDataReader;
            //if (fakeReader == null)
            //{
            //    fakeReader = new Fakes.FakeDataReader(reader);
            //}
            var mappings = dataType.GetProperties().Where(p => p.CanWrite && p.PropertyType.IsSystemType())
                .Select(prop => new MappingData() { Ordinal = reader.GetOrdinal(prop.Name), Property = prop, Mapper = MemberMapperHelper.CreateMemberMapper(prop) });
            while (reader.Read())
            {
                var obj = Activator.CreateInstance(dataType);
                MapData(obj, reader, mappings);
                yield return obj;
            }
        }
        private static void MapData(object instance, IDataReader reader, IEnumerable<MappingData> mappings)
        {
            foreach (var item in mappings)
            {
                if (item.Mapper == null || item.Ordinal < 0) { continue; }
                item.Mapper.Map(instance, reader, item.Ordinal);
            }
        }

        /// <summary>
        /// 将DataReader转成动态对象集合（对象类型根据DataReader数据结构生成，名称随机）
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<object> ToDynamicObjectList(this IDataReader reader)
        {
            var schema = reader.GetSchemaTable();
            var name = new StringBuilder();
            var fields = new Dictionary<string, Type>();
            foreach (DataRow r in schema.Rows)
            {
                var allowDbNull = Convert.ToBoolean(r["AllowDBNull"]);
                var ordinal = Convert.ToInt32(r["ColumnOrdinal"]);
                var dataType = reader.GetFieldType(ordinal);
                var fieldName = reader.GetName(ordinal);
                name.AppendFormat("{0}@{1}{2}&", fieldName, dataType.Name, allowDbNull && dataType.IsValueType ? "?" : "");
                if (allowDbNull && dataType.IsValueType)
                {
                    dataType = typeof(Nullable<>).MakeGenericType(dataType);
                }
                if (fields.ContainsKey(fieldName))
                {
                    throw new InvalidOperationException(string.Format("无效的操作：动态类型转换不支持重复的成员名称：{0}", fieldName));
                }
                fields.Add(fieldName, dataType);
            }
            var code = name.ToString().GetHashCode().ToString();
            var key = string.Format("reader_dynamic_type_{0}", code);
            var modelType = _dynamicTypeCache.GetOrAdd(key, (str) =>
            {
                var className = string.Format("DynamicDataReaderModel_{0}_{1}", code, Guid.NewGuid().ToString("D").Split(new char[] { '-' }).Last());
                var sourceCodeCreater = new CSharpSourceCodeCreater(className, fields);
                var type = sourceCodeCreater.BuildCSharpType();
                return type;
            });
            var list = reader.AsEnumerable(modelType).ToList();
            return list;
        }

        /// <summary>
        /// 将泛型集合转换成IDataReader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IDataReader AsDataReader<T>(this IEnumerable<T> collection) where T : class
        {
            return new ListDataReader<T>(collection);
        }

        /// <summary>
        /// 将泛型集合转换成DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> collection) where T : class
        {
            var data = new DataTable();
            data.Locale = System.Globalization.CultureInfo.CurrentCulture;
            data.TableName = typeof(T).FullName;
            using (var dr = new ListDataReader<T>(collection))
            {
                data.Load(dr);
                return data;
            }
        }

        /// <summary>
        /// 映射数据结构
        /// </summary>
        internal class MappingData
        {
            /// <summary>
            /// 索引位置
            /// </summary>
            public int Ordinal { get; set; }
            /// <summary>
            /// 映射属性
            /// </summary>
            public PropertyInfo Property { get; set; }
            /// <summary>
            /// 映射器
            /// </summary>
            public IMemberMapper Mapper { get; set; }
            /// <summary>
            /// 属性访问器（获取某实例对象上对应属性的值）
            /// </summary>
            public Func<object, object> ValueAccessor { get; set; }
        }
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
            isNullable = dataType.IsNullableType(ref underlyingType);
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
                        name = string.Format("Get{0}{1}", isNullable ? "Nullable" : string.Empty, typeCode.ToString());
                        method = typeof(DataReaderMethods).GetMethod(name);
                        break;
                    case TypeCode.String:
                        method = typeof(DataReaderMethods).GetMethod("GetString");
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
            if (reader.IsDBNull(ordinal))
            {
                throw WrapValueTypeNullReferenceException(reader, ordinal);
            }
            try { return reader.GetInt16(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static short? GetNullableInt16(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            try { return reader.GetInt16(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static int GetInt32(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                throw WrapValueTypeNullReferenceException(reader, ordinal);
            }
            try { return reader.GetInt32(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static int? GetNullableInt32(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            try { return reader.GetInt32(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static long GetInt64(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                throw WrapValueTypeNullReferenceException(reader, ordinal);
            }
            try { return reader.GetInt64(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static long? GetNullableInt64(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            try { return reader.GetInt64(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static decimal GetDecimal(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                throw WrapValueTypeNullReferenceException(reader, ordinal);
            }
            try { return reader.GetDecimal(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static decimal? GetNullableDecimal(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            try { return reader.GetDecimal(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static double GetDouble(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                throw WrapValueTypeNullReferenceException(reader, ordinal);
            }
            try { return reader.GetDouble(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static double? GetNullableDouble(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            try { return reader.GetDouble(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static float GetSingle(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                throw WrapValueTypeNullReferenceException(reader, ordinal);
            }
            try { return reader.GetFloat(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static float? GetNullableSingle(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            try { return reader.GetFloat(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static bool GetBoolean(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                throw WrapValueTypeNullReferenceException(reader, ordinal);
            }
            try { return reader.GetBoolean(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static bool? GetNullableBoolean(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            try { return reader.GetBoolean(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static DateTime GetDateTime(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                throw WrapValueTypeNullReferenceException(reader, ordinal);
            }
            try { return reader.GetDateTime(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static DateTime? GetNullableDateTime(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            try { return reader.GetDateTime(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static Guid GetGuid(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                throw WrapValueTypeNullReferenceException(reader, ordinal);
            }
            try { return reader.GetGuid(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static Guid? GetNullableGuid(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            try { return reader.GetGuid(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static byte GetByte(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                throw WrapValueTypeNullReferenceException(reader, ordinal);
            }
            try { return reader.GetByte(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static byte? GetNullableByte(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            try { return reader.GetByte(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static char GetChar(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                throw WrapValueTypeNullReferenceException(reader, ordinal);
            }
            try { return reader.GetChar(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static char? GetNullableChar(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            try { return reader.GetChar(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
        }

        public static string GetString(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            try { return reader.GetString(ordinal); } catch (InvalidCastException ex) { throw WrapInvalidValueException(ordinal, reader, ex); }
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
            if (reader.IsDBNull(ordinal))
            {
                throw WrapValueTypeNullReferenceException(reader, ordinal);
            }
            Type fieldType = reader.GetFieldType(ordinal);
            try
            {
                object value;
                if (fieldType == typeof(short))
                {
                    value = reader.GetInt16(ordinal);
                }
                else
                {
                    value = reader.GetInt32(ordinal);
                }
                return (TEnum)Enum.ToObject(typeof(TEnum), value);
            }
            catch (InvalidCastException ex)
            {
                throw WrapInvalidValueException(ordinal, reader, ex);
            }
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
                throw new InvalidCastException(string.Format("获取类型{0}在索引{1}(字段:{2})处不能为NULL值", typeof(T).FullName, ordinal, reader.GetName(ordinal)));
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


        /// <summary>
        /// 包装值类型空引用异常
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        private static Exception WrapValueTypeNullReferenceException(IDataReader reader, int ordinal)
        {
            var fieldName = reader.GetName(ordinal);
            var callMethod = new StackTrace().GetFrame(1).GetMethod();
            var returnType = callMethod.DeclaringType.GetMethod(callMethod.Name).ReturnType;
            return new DataAccessException(string.Format("对于值类型{0}在索引位置{1}[{2}]处返回了NULL值", returnType.FullName, ordinal, fieldName));
        }

        /// <summary>
        /// 包装类型转换错误
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="reader"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private static Exception WrapInvalidValueException(int ordinal, IDataReader reader, Exception exception)
        {
            var callMethod = new StackTrace().GetFrame(1).GetMethod();
            var returnType = callMethod.DeclaringType.GetMethod(callMethod.Name).ReturnType;
            var isNullableType = returnType.IsNullableType(ref returnType);
            var typeName = isNullableType ? string.Format("Nullable<{0}>", returnType.FullName) : returnType.FullName;
            var dataType = reader.GetDataTypeName(ordinal);
            var fieldName = reader.GetName(ordinal);
            throw new DataAccessException(string.Format("转换无效:所需类型{0}在索引位置{1}[{2}]处的数据源类型为{3}", typeName, ordinal, fieldName, dataType), exception);
        }
    }
}
