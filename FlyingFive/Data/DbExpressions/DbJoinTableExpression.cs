using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示DB查询中的JOIN连接操作
    /// </summary>
    public class DbJoinTableExpression : DbMainTableExpression
    {
        /// <summary>
        /// 连接查询类型
        /// </summary>
        public DbJoinType JoinType { get; private set; }
        /// <summary>
        /// 连接条件
        /// </summary>
        public DbExpression Condition { get; private set; }

        public DbJoinTableExpression(DbJoinType joinType, DbTableSegment table, DbExpression condition)
            : base(DbExpressionType.JoinTable, table)
        {
            this.JoinType = joinType;
            this.Condition = condition;
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    /// <summary>
    /// 连接查询类型
    /// </summary>
    public enum DbJoinType
    {
        InnerJoin,
        LeftJoin,
        RightJoin,
        FullJoin
    }
}
