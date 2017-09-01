using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示排序动作
    /// </summary>
    public class DbOrdering
    {
        /// <summary>
        /// 排序逻辑
        /// </summary>
        public DbExpression Expression { get; private set; }
        /// <summary>
        /// 排序类型
        /// </summary>
        public DbOrderType OrderType { get; private set; }
        
        public DbOrdering(DbExpression expression, DbOrderType orderType)
        {
            this.Expression = expression;
            this.OrderType = orderType;
        }
    }

    /// <summary>
    /// 排序类型
    /// </summary>
    public enum DbOrderType
    {
        /// <summary>
        /// 升序
        /// </summary>
        Asc,
        /// <summary>
        /// 降序
        /// </summary>
        Desc
    }
}
