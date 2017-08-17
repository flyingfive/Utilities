using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using MyDBAssistant.Data;

namespace MyDBAssistant.Schema
{
    /// <summary>
    /// 表示数据库连接字符串
    /// </summary>
    public class ConnectionString
    {
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DataBase { get; set; }
        /// <summary>
        /// 登陆用户
        /// </summary>
        public string LoginUser { get; set; }
        /// <summary>
        /// 登陆密码
        /// </summary>
        public string LoginPwd { get; set; }
        /// <summary>
        /// 数据库实例版本
        /// </summary>
        public MsSqlVersion Version { get; private set; }

        /// <summary>
        /// 验证连接字符串是否有效
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            bool flag = !string.IsNullOrWhiteSpace(Server) &&
                !string.IsNullOrWhiteSpace(DataBase) &&
                !string.IsNullOrWhiteSpace(LoginUser);
            if (flag)
            {
                //var helper = new MsSqlHelper(this.ToString().Replace(DataBase, "master"));                
                var connectionString = this.ToString().Replace(DataBase, "master");
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT @@VERSION";
                    connection.Open();
                    var ver = Convert.ToString(command.ExecuteScalar());//helper.ExecuteQueryAsSingle("SELECT @@VERSION", CommandType.Text));
                    if (ver.Replace(" ", "").StartsWith("MicrosoftSQLServer2012", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Version = MsSqlVersion.MSSQL2012;
                    }
                    if (ver.Replace(" ", "").StartsWith("MicrosoftSQLServer2008", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Version = MsSqlVersion.MSSQL2008;
                    }
                    if (ver.Replace(" ", "").StartsWith("MicrosoftSQLServer2005", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Version = MsSqlVersion.MSSQL2005;
                    }
                    if (ver.Replace(" ", "").StartsWith("MicrosoftSQLServer2000", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Version = MsSqlVersion.MSSQL2000;
                    }
                }

            }
            return flag;
        }

        /// <summary>
        /// 数据库是否存在
        /// </summary>
        /// <returns></returns>
        public bool HasDataBaseExists()
        {
            string masterConnectionString = this.ToString().Replace(DataBase, "master");
            //MsSqlHelper helper = new MsSqlHelper(masterConnectionString);
            using (var connection = new SqlConnection(masterConnectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ISNULL(COUNT(0), 0) FROM sysdatabases WHERE name = @name";
                command.CommandType = CommandType.Text;
                command.Parameters.Clear();
                command.Parameters.AddRange(new DbParameter[] { new SqlParameter("@name", DataBase) });
                connection.Open();
                var count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            //string sql = "SELECT ISNULL(COUNT(0), 0) FROM sysdatabases WHERE name = @name";
            //int count = Convert.ToInt32(helper.ExecuteQueryAsSingle(sql, System.Data.CommandType.Text, new DbParameter[] { new SqlParameter("@name", DataBase) }));
            //return count > 0;
        }

        /// <summary>
        /// 连接字符串形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string connectionString = string.Format(@"Data Source={0};Initial Catalog={1};User ID={2};Password={3};",
                Server, DataBase, LoginUser, LoginPwd);
            return connectionString;
        }
    }

}
