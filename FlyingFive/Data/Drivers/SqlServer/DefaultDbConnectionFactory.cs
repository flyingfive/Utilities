using FlyingFive.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    /// <summary>
    /// 默认的MsSql数据库连接工厂
    /// </summary>
    public class SqlServerDbConnectionFactory : IDbConnectionFactory
    {
        private string _connectionString = string.Empty;
        public SqlServerDbConnectionFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) { throw new ArgumentException("参数: connectionString无效!"); }
            this._connectionString = connectionString;
        }
        public IDbConnection CreateConnection()
        {
            var connection = new SqlConnection(this._connectionString);
            return connection;
        }
    }
}
