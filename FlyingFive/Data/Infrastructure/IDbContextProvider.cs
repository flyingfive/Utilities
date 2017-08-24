using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Infrastructure
{
    /// <summary>
    /// 表示DB上下文提供者
    /// </summary>
    public interface IDbContextProvider
    {
        /// <summary>
        /// 为DB上下文环境提供一个数据库连接
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection();
        //IDbExpressionTranslator CreateDbExpressionTranslator();
    }
}
