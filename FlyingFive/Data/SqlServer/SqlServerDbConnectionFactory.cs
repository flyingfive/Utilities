using FlyingFive.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.SqlServer
{
    /// <summary>
    /// SqlServer数据库连接工厂
    /// </summary>
    public class SqlServerDbConnectionFactory : IDbConnectionFactory
    {
        private string _connectionString = null;

        public SqlServerDbConnectionFactory(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("参数: connectionString错误");
            }
            this._connectionString = connectionString;
        }

        public SqlServerDbConnectionFactory(string dataSource, string catalog, string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(dataSource)) { throw new ArgumentException("参数: dataSource错误"); }
            if (string.IsNullOrWhiteSpace(catalog)) { throw new ArgumentException("参数: catalog错误"); }
            if (string.IsNullOrWhiteSpace(userId)) { throw new ArgumentException("参数: userId错误"); }
            if (string.IsNullOrWhiteSpace(password)) { throw new ArgumentException("参数: password错误"); }
            var builder = new SqlConnectionStringBuilder();
            builder.DataSource = dataSource;
            builder.InitialCatalog = catalog;
            builder.UserID = userId;
            builder.MultipleActiveResultSets = true;
            builder.ConnectTimeout = 15;
            this._connectionString = builder.ToString();
        }

        public IDbConnection CreateConnection()
        {
            var connection = new SqlConnection(this._connectionString);
            return connection;
        }

        /// <summary>
        /// 从配置文件创建一个SqlServer数据库连接工厂
        /// </summary>
        /// <param name="configName">配置节点名称</param>
        /// <returns></returns>
        public static IDbConnectionFactory Create(string configName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[configName].ConnectionString;
            return new SqlServerDbConnectionFactory(connectionString);
        }
    }
}
