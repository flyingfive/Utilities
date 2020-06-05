using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data
{
    /// <summary>
    /// 异常辅助工具
    /// </summary>
    public static class UtilExceptions
    {
        /// <summary>
        /// NULL检查
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="paramName"></param>
        public static void CheckNull(object obj, string paramName = null)
        {
            if (obj == null)
                throw new ArgumentNullException(paramName);
        }

        /// <summary>
        /// 不支持的方法
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static NotSupportedException NotSupportedMethod(MethodInfo method)
        {
            return new NotSupportedException(string.Format("不支持的方法 '{0}'.", ToMethodString(method)));
        }

        private static string ToMethodString(MethodInfo method)
        {
            StringBuilder sb = new StringBuilder();
            ParameterInfo[] parameters = method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo p = parameters[i];

                if (i > 0)
                    sb.Append(",");

                string s = null;
                if (p.IsOut)
                    s = "out ";

                sb.AppendFormat("{0}{1} {2}", s, p.ParameterType.Name, p.Name);
            }

            return string.Format("{0}.{1}({2})", method.DeclaringType.Name, method.Name, sb.ToString());
        }

        /// <summary>
        /// 追加错误信息
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string AppendErrorMsg(IDataReader reader, int ordinal, Exception ex)
        {
            string msg = null;
            if (reader.IsDBNull(ordinal))
            {
                msg = string.Format("Please make sure that the member of the column '{0}'({1},{2},{3}) map is nullable.", reader.GetName(ordinal), ordinal.ToString(), reader.GetDataTypeName(ordinal), reader.GetFieldType(ordinal).FullName);
            }
            else if (ex is InvalidCastException)
            {
                msg = string.Format("Please make sure that the member of the column '{0}'({1},{2},{3}) map is the correct type.", reader.GetName(ordinal), ordinal.ToString(), reader.GetDataTypeName(ordinal), reader.GetFieldType(ordinal).FullName);
            }
            else
                msg = string.Format("An error occurred while mapping the column '{0}'({1},{2},{3}). For details please see the inner exception.", reader.GetName(ordinal), ordinal.ToString(), reader.GetDataTypeName(ordinal), reader.GetFieldType(ordinal).FullName);
            return msg;
        }
    }

    /// <summary>
    /// 数据库驱动类型
    /// </summary>
    [Flags]
    public enum DatabaseDriverType
    {
        /// <summary>
        /// MS SQLServer
        /// </summary>
        MsSql = 1 << 0,
        /// <summary>
        /// MySql
        /// </summary>
        MySql = 1 << 1,
        /// <summary>
        /// Oracle
        /// </summary>
        Oracle = 1 << 2,
        /// <summary>
        /// SQLite
        /// </summary>
        SQLite = 1 << 3,
        //OleDB = 1 << 4
    }
}
