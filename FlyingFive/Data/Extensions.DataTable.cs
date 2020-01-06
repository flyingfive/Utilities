using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FlyingFive.Data.CodeDom;

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
                var sourceCodeCreater = new SourceCodeCreater(className, fields);
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
    }
}
