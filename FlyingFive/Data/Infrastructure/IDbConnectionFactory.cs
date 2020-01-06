using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Infrastructure
{
    /// <summary>
    /// 表示数据库连接工厂
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// 创建一个数据库连接
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection();
    }
}
