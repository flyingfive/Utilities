using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FlyingFive.Data.CodeDom;
using FlyingFive.Data.Emit;

namespace FlyingFive.Data
{
    public static partial class Extensions
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<string, Type> _dynamicTypeCache = new System.Collections.Concurrent.ConcurrentDictionary<string, Type>();

        /// <summary>
        /// 将DataTable转成匿名对象集合（对象类型根据Table数据结构生成，名称随机）
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<object> ToList(this DataTable data)
        {
            var name = string.Join("&", data.Columns.OfType<DataColumn>().Select(c => string.Format("{0}@{1}{2}", c.ColumnName, c.DataType.ToString(), c.AllowDBNull ? "?" : "")));
            var key = name.GetHashCode().ToString();
            var modelType = _dynamicTypeCache.GetOrAdd(key, (str) =>
            {
                var className = string.Format("DynamicDataModel_{0}", Guid.NewGuid().ToString("D").Split(new char[] { '-' }).Last());
                var fields = new Dictionary<string, Type>();
                foreach (DataColumn dc in data.Columns)
                {
                    var dataType = dc.DataType;
                    if (dc.AllowDBNull && dc.DataType.IsValueType)
                    {
                        dataType = typeof(Nullable<>).MakeGenericType(dc.DataType);
                    }
                    if (fields.ContainsKey(dc.ColumnName))
                    {
                        throw new InvalidOperationException(string.Format("无效的操作：动态类型转换不支持重复的成员名称：{0}", dc.ColumnName));
                    }
                    fields.Add(dc.ColumnName, dataType);
                }
                var sourceCodeCreater = new CSharpSourceCodeCreater(className, fields);
                var type = sourceCodeCreater.BuildCSharpType();
                return type;
            });
            using (var reader = data.CreateDataReader())
            {
                var list = reader.AsEnumerable(modelType).ToList();
                return list;
            }
        }

        /// <summary>
        /// 将DataTable转成具体类型的对象集合
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">数据表</param>
        /// <returns></returns>
        public static List<T> ToList<T>(this DataTable data) where T : class, new()
        {
            using (var reader = new Fakes.FakeDataReader(data.CreateDataReader()))
            {
                var list = reader.AsEnumerable<T>().ToList();
                return list;
            }
        }

        //private static Hashtable _allModelTableSchema = null;

        ///// <summary>
        ///// 获取指定类型的DataTable结构
        ///// </summary>
        ///// <param name="modelType">模型类型</param>
        ///// <returns></returns>
        //private static DataTable GetTableSchemaOfModel(this Type modelType)
        //{
        //    DataTable schema = null;
        //    if (_allModelTableSchema == null) { _allModelTableSchema = new Hashtable(); }
        //    var properties = modelType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        //    if (_allModelTableSchema.ContainsKey(modelType.FullName))
        //    {
        //        schema = _allModelTableSchema[modelType.FullName] as DataTable;
        //    }
        //    else
        //    {
        //        schema = new DataTable(modelType.Name);
        //        foreach (var prop in properties)
        //        {
        //            if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>)) { continue; }
        //            var column = new DataColumn(prop.Name);
        //            column.DataType = prop.PropertyType.IsGenericType ? prop.PropertyType.GetGenericArguments().First() : prop.PropertyType;
        //            if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        //            {
        //                column.AllowDBNull = true;
        //            }
        //            schema.Columns.Add(column);
        //        }
        //        _allModelTableSchema.Add(modelType.FullName, schema);
        //    }
        //    return schema.Clone();
        //}

        ///// <summary>
        ///// 将泛型list集合转换为DataTable
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="list"></param>
        ///// <returns></returns>
        //public static DataTable ToDataTable<T>(this IList<T> list)
        //{
        //    if (list == null) { return null; }
        //    var type = typeof(T);
        //    if (type == typeof(Object) && list.Count() > 0 && list.FirstOrDefault() != null)
        //    {
        //        type = list.FirstOrDefault().GetType();
        //    }
        //    var table = GetTableSchemaOfModel(type);
        //    var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        //    var items = table.Columns.OfType<DataColumn>()
        //        .Where(dc => properties.Any(p => string.Equals(p.Name, dc.ColumnName, StringComparison.CurrentCultureIgnoreCase)))
        //        .Select(dc => new { column = dc, prop = properties.SingleOrDefault(p => string.Equals(p.Name, dc.ColumnName, StringComparison.CurrentCultureIgnoreCase)) })
        //        .ToList().ToDictionary(x => x.column, x => new { prop = x.prop, getter = DelegateGenerator.CreateValueGetter(x.prop) });
        //    foreach (T item in list)
        //    {
        //        var row = table.NewRow();
        //        foreach (DataColumn column in items.Keys)
        //        {
        //            var val = items[column].getter(item);
        //            row[column.ColumnName] = val == null ? DBNull.Value : val;
        //        }
        //        table.Rows.Add(row);
        //    }
        //    return table;
        //}
    }
}
