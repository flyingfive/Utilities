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
    public class SqlServerConnectionFactory : IDbConnectionFactory
    {
        public string ConnectionString { get; private set; }
        public SqlServerConnectionFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) { throw new ArgumentException("参数: connectionString无效!"); }
            this.ConnectionString = connectionString;
        }
        public IDbConnection CreateConnection()
        {
            var connection = new SqlConnection(this.ConnectionString);
            return connection;
        }
    }
}
