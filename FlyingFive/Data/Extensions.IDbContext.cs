﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data
{
    public static partial class Extensions
    {
        /// <summary>
        /// 原生sql语句查询
        /// </summary>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <param name="plainSql">原生sql语句</param>
        /// <param name="instance">包含执行sql语句中的参数值对象</param>
        /// <param name="cmdType">命令类型</param>
        /// <returns></returns>
        public static IEnumerable<T> SqlQuery<T>(this IDbContext context, string plainSql, object instance, CommandType cmdType = CommandType.Text)
        {
            var parameters = FindParameters(plainSql, instance);
            var list = context.SqlQuery<T>(plainSql, cmdType, parameters.ToArray());
            return list;
        }
        /// <summary>
        /// 原生sql语句查询
        /// </summary>
        /// <param name="plainSql">原生sql语句</param>
        /// <param name="instance">包含执行sql语句中的参数值对象</param>
        /// <param name="cmdType">命令类型</param>
        /// <returns></returns>
        public static int SqlQuery(this IDbContext context, string plainSql, object instance, CommandType cmdType = CommandType.Text)
        {
            var parameters = FindParameters(plainSql, instance);
            var cnt = context.Session.ExecuteNonQuery(plainSql, cmdType, parameters.ToArray());
            return cnt;
        }

        private static IList<FakeParameter> FindParameters(string plainSql, object instance)
        {
            var paramNames = FakeMsSqlParameter.FindQueryParmeters(plainSql);
            var parameters = new List<FakeParameter>();
            if (paramNames.Count > 0 && instance == null) { throw new FormatException("没有为sql查询提供必需的参数实例"); }
            if (instance != null)
            {
                var properties = instance.GetType().GetProperties().Where(p => p.CanRead);
                foreach (var pName in paramNames)
                {
                    var prop = properties.Where(p => p.Name.Equals(pName)).SingleOrDefault();
                    if (prop == null) { throw new FormatException(string.Format("没有为sql查询提供必需的参数:{0}", pName)); }
                    var pValue = prop.GetValue(instance, null);
                    var param = new FakeParameter(pName, pValue);
                    parameters.Add(param);
                }
            }
            return parameters;
        }
    }
}
