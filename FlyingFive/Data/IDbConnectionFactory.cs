using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data
{
    /// <summary>
    /// 表示数据库连接工厂
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// DB驱动类型
        /// </summary>
        DatabaseDriverType DriverType { get; }
        /// <summary>
        /// 创建DB连接的字符串
        /// </summary>
        string ConnectionString { get; }
        /// <summary>
        /// 创建一个数据库连接
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection();
    }
}
