using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FlyingFive.Data.Dynamic;

namespace FlyingFive.Data
{
    public static partial class Extensions
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<string, Type> _dynamicTypeCache = new System.Collections.Concurrent.ConcurrentDictionary<string, Type>();

        /// <summary>
        /// 将DataTable转成动态对象集合（对象类型根据Table数据结构生成，名称随机）
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<object> ToDynamicObjectList(this DataTable data)
        {
            if (data == null) { throw new ArgumentNullException("参数data不能为null"); }
            if (data.Columns.Count == 0) { return new List<object>(); }
            var duplicateColumns = data.Columns.OfType<DataColumn>().GroupBy(c => c.ColumnName).Where(g => g.Count() > 1).Select(g => g.Key);
            if (duplicateColumns.Count() > 0)
            {
                throw new InvalidOperationException(string.Format("无效的操作：动态类型转换不支持重复的成员名称：{0}", string.Join(",", duplicateColumns)));
            }
            using (var reader = data.CreateDataReader())
            {
                return reader.ToDynamicObjectList();
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
