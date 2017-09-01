using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Infrastructure
{
    /// <summary>
    /// DB上下文服务供应器,提供DB上下文中所需要的功能环境
    /// </summary>
    public interface IDbContextServiceProvider
    {
        /// <summary>
        /// 为上下文创建一个DB连接
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection();
        /// <summary>
        /// 为上下文创建一个表达式翻译器
        /// </summary>
        /// <returns></returns>
        IDbExpressionTranslator CreateDbExpressionTranslator();
    }
}
