using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace MyDBAssistant.Data
{
    /// <summary>
    /// 表示批量执行SQL语句的模型
    /// </summary>
    [Serializable]
    public class SqlCommandModel
    {
        /// <summary>
        /// sql语句
        /// </summary>
        public string Sql { get; set; }
        /// <summary>
        /// 参数集合
        /// </summary>
        public IList<DbParameter> Parameters { get; set; }
        /// <summary>
        /// 命令执行超时时间(默认30秒)
        /// </summary>
        public int CommandTimeout { get; set; }
        /// <summary>
        /// 命令类型,默认CommandType.Text
        /// </summary>
        public CommandType CommandType { get; set; }

        /// <summary>
        /// 默认构造
        /// </summary>
        public SqlCommandModel()
        {
            CommandType = System.Data.CommandType.Text;
            CommandTimeout = 30;
        }

        /// <summary>
        /// 按SQL语句初始化实例
        /// </summary>
        /// <param name="sql">SQL语句</param>
        public SqlCommandModel(string sql)
            : this()
        {
            this.Sql = sql;
        }

        /// <summary>
        /// 按SQL语句和参数列表初始化实例
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="paras">参数列表</param>
        public SqlCommandModel(string sql, IList<DbParameter> paras)
            : this(sql)
        {
            this.Parameters = paras;
        }
    }
}
