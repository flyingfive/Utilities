using FlyingFive.Data.Dynamic;
using FlyingFive.Data.Fakes;
using FlyingFive.Data.Kernel;
using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace FlyingFive.Data
{
    /// <summary>
    /// 提供IDatabaseHelper接口的扩展能力
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 执行原生SQL查询语句
        /// </summary>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <param name="plainSql">原生SQL查询脚本</param>
        /// <param name="instance">包含执行SQL脚本中的参数值对象</param>
        /// <param name="cmdType">命令类型</param>
        /// <returns></returns>
        public static IList<T> SqlQuery<T>(this IDatabaseHelper dbHelper, string plainSql, object instance, CommandType cmdType = CommandType.Text) where T : class, new()
        {
            if (dbHelper == null) { throw new ArgumentNullException("dbHelper"); }
            if (string.IsNullOrWhiteSpace(plainSql)) { throw new ArgumentException("plainSql"); }
            var parameters = dbHelper.CreateParameters(plainSql, instance);
            using (var reader = dbHelper.ExecuteReader(plainSql, cmdType, parameters.ToArray()))
            {
                var list = reader.AsEnumerable<T>().ToList();
                return list;
            }
        }

        /// <summary>
        /// 执行原生SQL命令语句
        /// </summary>
        /// <param name="plainSql">原生SQL命令脚本</param>
        /// <param name="instance">包含执行SQL脚本中的参数值对象</param>
        /// <param name="cmdType">命令类型</param>
        /// <returns></returns>
        public static int ExecuteSqlCommand(this IDatabaseHelper dbHelper, string plainSql, object instance, CommandType cmdType = CommandType.Text)
        {
            if (dbHelper == null) { throw new ArgumentNullException("dbHelper"); }
            if (string.IsNullOrWhiteSpace(plainSql)) { throw new ArgumentException("plainSql"); }
            var parameters = dbHelper.CreateParameters(plainSql, instance);
            var cnt = dbHelper.ExecuteNonQuery(plainSql, cmdType, parameters.ToArray());
            return cnt;
        }

        /// <summary>
        /// 查找SQL语句中的参数列表
        /// </summary>
        /// <param name="dbHelper"></param>
        /// <param name="plainSql"></param>
        /// <returns></returns>
        public static List<String> FindSqlParameters(this IDatabaseHelper dbHelper, string plainSql)
        {
            if (dbHelper == null) { throw new ArgumentNullException("dbHelper"); }
            if (string.IsNullOrWhiteSpace(plainSql)) { throw new ArgumentException("plainSql"); }
            var driverType = (dbHelper as DatabaseHelper).DbConnectionFactory.DriverType;
            switch (driverType)
            {
                case DatabaseDriverType.MsSql: return FakeMsSqlParameter.FindQueryParmeters(plainSql);
                //todo:其它驱动待完善补充...
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 根据参数列表从数据对象中创建查询参数
        /// </summary>
        /// <param name="paramNames"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static List<FakeParameter> CreateParameters(this IDatabaseHelper dbHelper, string plainSql, object instance)
        {
            if (dbHelper == null) { throw new ArgumentNullException("dbHelper"); }
            if (string.IsNullOrWhiteSpace(plainSql)) { throw new ArgumentException("plainSql"); }
            var paramNames = dbHelper.FindSqlParameters(plainSql);
            if (paramNames.Count <= 0) { return new List<FakeParameter>(); }
            if (instance == null)
            {
                throw new FormatException("参数生成失败：没有提供必要的数据对象。");
            }
            var parameters = new List<FakeParameter>();
            if (instance != null)
            {
                var properties = instance.GetType().GetProperties().Where(p => p.CanRead).Select(p => new { p.Name, getter = DelegateGenerator.CreateValueGetter(p) });
                foreach (var pName in paramNames)
                {
                    var prop = properties.Where(p => p.Name.Equals(pName.Substring(1))).SingleOrDefault();
                    if (prop == null) { throw new FormatException(string.Format("参数生成失败:数据对象没有指定的成员[{0}]", pName)); }
                    var pValue = prop.getter(instance);
                    var param = new FakeParameter(pName, pValue);
                    parameters.Add(param);
                }
            }
            return parameters;
        }
    }
}
