using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.Caching;
using System.Web;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;

namespace MyDBAssistant.Data
{
    /// <summary>
    /// DataReader转换工具
    /// </summary>
    public static class DataReaderConverter
    {
        private static Hashtable _cachedConverters = null;
        private static bool _enableConverterCaching = false;            //是否启用转换器缓存

        static DataReaderConverter()
        {
            _cachedConverters = new Hashtable();
        }

        private const string LinqBinary = "System.Data.Linq.Binary";
        private static readonly MethodInfo enumParse = typeof(Enum).GetMethod("Parse", new Type[] { typeof(Type), typeof(string), typeof(bool) });
        private static readonly MethodInfo getItem = typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetIndexParameters().Any() && p.GetIndexParameters()[0].ParameterType == typeof(int))
                    .Select(p => p.GetGetMethod()).First();

        /// <summary>
        /// DataReader转换成对象集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="reader">一个dataReader</param>
        /// <returns></returns>
        public static IEnumerable<T> ToList<T>(IDataReader reader)
        {
            var converter = GetDeserializer(typeof(T), reader, 0, -1, false);
            while (reader.Read())
            {
                var next = converter(reader);
                yield return (T)next;
            }
        }

        /// <summary>
        /// DataReader转换成对象集合
        /// </summary>
        /// <param name="reader">一个dataReader</param>
        /// <param name="type">对象类型</param>
        /// <returns></returns>
        public static IEnumerable<Object> ToList(IDataReader reader, Type type)
        {
            var converter = GetDeserializer(type, reader, 0, -1, false);
            while (reader.Read())
            {
                var next = converter(reader);
                yield return next;
            }
        }

        /// <summary>
        /// 设置IDataReader转换器缓存功能
        /// 警告:启用缓存后在不同DataTable结构转换为同一类型对象时可能会产生问题
        /// </summary>
        /// <param name="enable">是否启用缓存</param>
        public static void SetConverterCache(bool enable)
        {
            _enableConverterCaching = enable;
            if (!enable)
            {
                _cachedConverters.Clear();
            }
        }

        /// <summary>
        /// 移除缓存中指定类型的对象转换器
        /// </summary>
        /// <param name="type">对象类型</param>
        public static void RemoveCachedConverter(Type type)
        {
            if (_cachedConverters.ContainsKey(type))
            {
                _cachedConverters.Remove(type);
            }
        }

        private static Func<IDataReader, object> GetDeserializer(Type type, IDataReader reader, int startBound, int length, bool returnNullIfFirstMissing)
        {
#if !CSHARP30
            // dynamic is passed in as Object ... by c# design
            if (type == typeof(object) || type == typeof(FastExpando))
            {
                return GetDynamicDeserializer(reader, startBound, length, returnNullIfFirstMissing);
            }
#endif
            if (_cachedConverters.ContainsKey(type))
            {
                return _cachedConverters[type] as Func<IDataReader, object>;
            }
            else
            {
                var converter = GetTypeDeserializer(type, reader, startBound, length, returnNullIfFirstMissing);
                if (_enableConverterCaching)
                {
                    _cachedConverters.Add(type, converter);
                }
                return converter;
            }
            //if (!(typeMap.ContainsKey(type) || type.FullName == LinqBinary))
            //{
            //    return GetTypeDeserializer(type, reader, startBound, length, returnNullIfFirstMissing);
            //}
            //return GetStructDeserializer(type, startBound);
        }

        private static Func<IDataReader, object> GetDynamicDeserializer(IDataRecord reader, int startBound, int length, bool returnNullIfFirstMissing)
        {
            var fieldCount = reader.FieldCount;
            if (length == -1)
            {
                length = fieldCount - startBound;
            }

            if (fieldCount <= startBound)
            {
                throw new ArgumentException("When using the multi-mapping APIs ensure you set the splitOn param if you have keys other than Id", "splitOn");
            }

            return
                 r =>
                 {
                     IDictionary<string, object> row = new Dictionary<string, object>(length);
                     for (var i = startBound; i < startBound + length; i++)
                     {
                         var tmp = r.GetValue(i);
                         tmp = tmp == DBNull.Value ? null : tmp;
                         row[r.GetName(i)] = tmp;
                         if (returnNullIfFirstMissing && i == startBound && tmp == null)
                         {
                             return null;
                         }
                     }
                     //we know this is an object so it will not box
                     return FastExpando.Attach(row);
                 };
        }

        /// <summary>
        /// 生成从dataReader反序列化成对象的委托方法
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="reader">一个dataReader</param>
        /// <param name="startBound">绑定字段开始索引</param>
        /// <param name="length">绑定字段的长度</param>
        /// <param name="returnNullIfFirstMissing"></param>
        /// <returns></returns>
        public static Func<IDataReader, object> GetTypeDeserializer(
            //#if CSHARP30
            //            Type type, IDataReader reader, int startBound, int length, bool returnNullIfFirstMissing
            //#else
Type type, IDataReader reader, int startBound = 0, int length = -1, bool returnNullIfFirstMissing = false
            //#endif
)
        {
            var dm = new DynamicMethod(string.Format("Deserialize{0}", Guid.NewGuid()), typeof(object), new[] { typeof(IDataReader) }, true);

            var il = dm.GetILGenerator();
            il.DeclareLocal(typeof(int));
            il.DeclareLocal(type);
            bool haveEnumLocal = false;
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_0);
            var properties = GetSettableProps(type);
            var fields = GetSettableFields(type);
            if (length == -1)
            {
                length = reader.FieldCount - startBound;
            }

            if (reader.FieldCount <= startBound)
            {
                throw new ArgumentException("When using the multi-mapping APIs ensure you set the splitOn param if you have keys other than Id", "splitOn");
            }

            var names = new List<string>();

            for (int i = startBound; i < startBound + length; i++)
            {
                names.Add(reader.GetName(i));
            }

            var setters = (
                            from n in names
                            let prop = properties.FirstOrDefault(p => string.Equals(p.Name, n, StringComparison.Ordinal)) // property case sensitive first
                                  ?? properties.FirstOrDefault(p => string.Equals(p.Name, n, StringComparison.OrdinalIgnoreCase)) // property case insensitive second
                            let field = prop != null ? null : (fields.FirstOrDefault(p => string.Equals(p.Name, n, StringComparison.Ordinal)) // field case sensitive third
                                ?? fields.FirstOrDefault(p => string.Equals(p.Name, n, StringComparison.OrdinalIgnoreCase))) // field case insensitive fourth
                            select new { Name = n, Property = prop, Field = field }
                          ).ToList();

            int index = startBound;

            if (type.IsValueType)
            {
                il.Emit(OpCodes.Ldloca_S, (byte)1);
                il.Emit(OpCodes.Initobj, type);
            }
            else
            {
                il.Emit(OpCodes.Newobj, type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null));
                il.Emit(OpCodes.Stloc_1);
            }
            il.BeginExceptionBlock();
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Ldloca_S, (byte)1);// [target]
            }
            else
            {
                il.Emit(OpCodes.Ldloc_1);// [target]
            }

            // stack is now [target]

            bool first = true;
            var allDone = il.DefineLabel();
            foreach (var item in setters)
            {
                if (item.Property != null || item.Field != null)
                {
                    il.Emit(OpCodes.Dup); // stack is now [target][target]
                    Label isDbNullLabel = il.DefineLabel();
                    Label finishLabel = il.DefineLabel();

                    il.Emit(OpCodes.Ldarg_0); // stack is now [target][target][reader]
                    EmitInt32(il, index); // stack is now [target][target][reader][index]
                    il.Emit(OpCodes.Dup);// stack is now [target][target][reader][index][index]
                    il.Emit(OpCodes.Stloc_0);// stack is now [target][target][reader][index]
                    il.Emit(OpCodes.Callvirt, getItem); // stack is now [target][target][value-as-object]

                    Type memberType = item.Property != null ? item.Property.Type : item.Field.FieldType;

                    if (memberType == typeof(char) || memberType == typeof(char?))
                    {
                        il.EmitCall(OpCodes.Call, typeof(DataReaderConverter).GetMethod(
                            memberType == typeof(char) ? "ReadChar" : "ReadNullableChar", BindingFlags.Static | BindingFlags.Public), null); // stack is now [target][target][typed-value]
                    }
                    //else if (memberType == typeof(bool) || memberType == typeof(bool?))
                    //{
                    //    il.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod(
                    //        memberType == typeof(bool) ? "ReadBoolean" : "ReadNullableBoolean", BindingFlags.Static | BindingFlags.Public), null); // stack is now [target][target][typed-value]
                    //}
                    else
                    {
                        il.Emit(OpCodes.Dup); // stack is now [target][target][value][value]
                        il.Emit(OpCodes.Isinst, typeof(DBNull)); // stack is now [target][target][value-as-object][DBNull or null]
                        il.Emit(OpCodes.Brtrue_S, isDbNullLabel); // stack is now [target][target][value-as-object]

                        // unbox nullable enums as the primitive, i.e. byte etc

                        var nullUnderlyingType = Nullable.GetUnderlyingType(memberType);
                        var unboxType = nullUnderlyingType != null && nullUnderlyingType.IsEnum ? nullUnderlyingType : memberType;

                        if (unboxType.IsEnum)
                        {
                            if (!haveEnumLocal)
                            {
                                il.DeclareLocal(typeof(string));
                                haveEnumLocal = true;
                            }

                            Label isNotString = il.DefineLabel();
                            il.Emit(OpCodes.Dup); // stack is now [target][target][value][value]
                            il.Emit(OpCodes.Isinst, typeof(string)); // stack is now [target][target][value-as-object][string or null]
                            il.Emit(OpCodes.Dup);// stack is now [target][target][value-as-object][string or null][string or null]
                            il.Emit(OpCodes.Stloc_2); // stack is now [target][target][value-as-object][string or null]
                            il.Emit(OpCodes.Brfalse_S, isNotString); // stack is now [target][target][value-as-object]

                            il.Emit(OpCodes.Pop); // stack is now [target][target]


                            il.Emit(OpCodes.Ldtoken, unboxType); // stack is now [target][target][enum-type-token]
                            il.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), null);// stack is now [target][target][enum-type]
                            il.Emit(OpCodes.Ldloc_2); // stack is now [target][target][enum-type][string]
                            il.Emit(OpCodes.Ldc_I4_1); // stack is now [target][target][enum-type][string][true]
                            il.EmitCall(OpCodes.Call, enumParse, null); // stack is now [target][target][enum-as-object]

                            il.Emit(OpCodes.Unbox_Any, unboxType); // stack is now [target][target][typed-value]

                            if (nullUnderlyingType != null)
                            {
                                il.Emit(OpCodes.Newobj, memberType.GetConstructor(new[] { nullUnderlyingType }));
                            }
                            if (item.Property != null)
                            {
                                il.Emit(OpCodes.Callvirt, item.Property.Setter); // stack is now [target]
                            }
                            else
                            {
                                il.Emit(OpCodes.Stfld, item.Field); // stack is now [target]
                            }
                            il.Emit(OpCodes.Br_S, finishLabel);


                            il.MarkLabel(isNotString);
                        }
                        if (memberType.FullName == LinqBinary)
                        {
                            il.Emit(OpCodes.Unbox_Any, typeof(byte[])); // stack is now [target][target][byte-array]
                            il.Emit(OpCodes.Newobj, memberType.GetConstructor(new Type[] { typeof(byte[]) }));// stack is now [target][target][binary]
                        }
                        else if (memberType == typeof(bool) || memberType == typeof(bool?))
                        {
                            il.EmitCall(OpCodes.Call, typeof(Convert).GetMethod("ToBoolean", new Type[] { typeof(object) }), null);// stack is now [target][target][typed-value]
                        }
                        else
                        {
                            il.EmitCall(OpCodes.Call, typeof(DataReaderConverter).GetMethod("ConvertObj", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(memberType), null);// stack is now [target][target][typed-value]
                            //il.Emit(OpCodes.Unbox_Any, unboxType); // stack is now [target][target][typed-value]
                        }
                        if (nullUnderlyingType != null && (nullUnderlyingType.IsEnum || nullUnderlyingType == typeof(bool)))
                        {
                            il.Emit(OpCodes.Newobj, memberType.GetConstructor(new[] { nullUnderlyingType }));
                        }
                    }
                    if (item.Property != null)
                    {
                        if (type.IsValueType)
                        {
                            il.Emit(OpCodes.Call, item.Property.Setter); // stack is now [target]
                        }
                        else
                        {
                            il.Emit(OpCodes.Callvirt, item.Property.Setter); // stack is now [target]
                        }
                    }
                    else
                    {
                        il.Emit(OpCodes.Stfld, item.Field); // stack is now [target]
                    }

                    il.Emit(OpCodes.Br_S, finishLabel); // stack is now [target]

                    il.MarkLabel(isDbNullLabel); // incoming stack: [target][target][value]

                    il.Emit(OpCodes.Pop); // stack is now [target][target]
                    il.Emit(OpCodes.Pop); // stack is now [target]

                    if (first && returnNullIfFirstMissing)
                    {
                        il.Emit(OpCodes.Pop);
                        il.Emit(OpCodes.Ldnull); // stack is now [null]
                        il.Emit(OpCodes.Stloc_1);
                        il.Emit(OpCodes.Br, allDone);
                    }

                    il.MarkLabel(finishLabel);
                    first = false;
                }
                //first = false;
                index += 1;
            }
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Pop);
            }
            else
            {
                il.Emit(OpCodes.Stloc_1); // stack is empty
            }
            il.MarkLabel(allDone);
            il.BeginCatchBlock(typeof(Exception)); // stack is Exception
            il.Emit(OpCodes.Ldloc_0); // stack is Exception, index
            il.Emit(OpCodes.Ldarg_0); // stack is Exception, index, reader
            il.EmitCall(OpCodes.Call, typeof(DataReaderConverter).GetMethod("ThrowDataException", BindingFlags.NonPublic | BindingFlags.Static), null);
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ldloc_1); // stack is [rval]
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
            il.Emit(OpCodes.Ret);

            return (Func<IDataReader, object>)dm.CreateDelegate(typeof(Func<IDataReader, object>));
        }

        //        private static Func<IDataReader, object> GetStructDeserializer(Type type, int index)
        //        {
        //            // no point using special per-type handling here; it boils down to the same, plus not all are supported anyway (see: SqlDataReader.GetChar - not supported!)
        //#pragma warning disable 618
        //            if (type == typeof(char))
        //            { // this *does* need special handling, though
        //                return r => DataReaderConverter.ReadChar(r.GetValue(index));
        //            }
        //            if (type == typeof(char?))
        //            {
        //                return r => DataReaderConverter.ReadNullableChar(r.GetValue(index));
        //            }
        //            if (type.FullName == LinqBinary)
        //            {
        //                return r => Activator.CreateInstance(type, r.GetValue(index));
        //            }
        //#pragma warning restore 618
        //            if (type == typeof(bool))
        //            {
        //                return r =>
        //                {
        //                    var val = r.GetValue(index);
        //                    return val == DBNull.Value ? false : (val.GetType() == type ? val : Convert.ToBoolean(val));
        //                };
        //            }
        //            if (type == typeof(bool?))
        //            {
        //                return r =>
        //                {
        //                    var val = r.GetValue(index);
        //                    return val == DBNull.Value ? null : (val.GetType() == type ? val : Convert.ToBoolean(val));
        //                };
        //            }
        //            return r =>
        //            {
        //                var val = r.GetValue(index);
        //                return val is DBNull ? null : val;
        //            };
        //        }

        ///// <summary>
        ///// Internal use only
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        //[Obsolete("This method is for internal usage only", false)]
        //private static char ReadChar(object value)
        //{
        //    if (value == null || value is DBNull) throw new ArgumentNullException("value");
        //    string s = value as string;
        //    if (s == null || s.Length != 1) throw new ArgumentException("A single-character was expected", "value");
        //    return s[0];
        //}

        ///// <summary>
        ///// Internal use only
        ///// </summary>
        //[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        //[Obsolete("This method is for internal usage only", false)]
        //private static char? ReadNullableChar(object value)
        //{
        //    if (value == null || value is DBNull) return null;
        //    string s = value as string;
        //    if (s == null || s.Length != 1) throw new ArgumentException("A single-character was expected", "value");
        //    return s[0];
        //}

        private static T ConvertObj<T>(dynamic obj)
        {
            return (T)obj;
        }

        static List<PropInfo> GetSettableProps(Type t)
        {
            return t
                  .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                  .Select(p => new PropInfo
                  {
                      Name = p.Name,
                      Setter = p.DeclaringType == t ? p.GetSetMethod(true) : p.DeclaringType.GetProperty(p.Name).GetSetMethod(true),
                      Type = p.PropertyType
                  })
                  .Where(info => info.Setter != null)
                  .ToList();
        }

        static List<FieldInfo> GetSettableFields(Type t)
        {
            return t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
        }

        private static void EmitInt32(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
        }
        /// <summary>
        /// Throws a data exception, only used internally
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="index"></param>
        /// <param name="reader"></param>
        private static void ThrowDataException(Exception ex, int index, IDataReader reader)
        {
            string name = "(n/a)", value = "(n/a)";
            if (reader != null && index >= 0 && index < reader.FieldCount)
            {
                name = reader.GetName(index);
                object val = reader.GetValue(index);
                if (val == null || val is DBNull)
                {
                    value = "<null>";
                }
                else
                {
                    value = Convert.ToString(val) + " - " + Type.GetTypeCode(val.GetType());
                }
            }
            throw new DataException(string.Format("Error parsing column {0} ({1}={2})", index, name, value), ex);
        }

        private class PropInfo
        {
            public string Name { get; set; }
            public MethodInfo Setter { get; set; }
            public Type Type { get; set; }
        }
    }

    public class FastExpando : System.Dynamic.DynamicObject, IDictionary<string, object>
    {
        IDictionary<string, object> data;

        public IDictionary<string, object> Data
        {
            get { return data; }
            set { data = value; }
        }

        public static FastExpando Attach(IDictionary<string, object> data)
        {
            return new FastExpando { data = data };
        }

        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
        {
            data[binder.Name] = value;
            return true;
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            return data.TryGetValue(binder.Name, out result);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return data.Keys;
        }

        #region IDictionary<string,object> Members

        void IDictionary<string, object>.Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return data.ContainsKey(key);
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get { return data.Keys; }
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            return data.TryGetValue(key, out value);
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get { return data.Values; }
        }

        object IDictionary<string, object>.this[string key]
        {
            get
            {
                return data[key];
            }
            set
            {
                if (!data.ContainsKey(key))
                {
                    throw new NotImplementedException();
                }
                data[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,object>> Members

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return data.Contains(item);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            data.CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<string, object>>.Count
        {
            get { return data.Count; }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,object>> Members

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        #endregion
    }

}
