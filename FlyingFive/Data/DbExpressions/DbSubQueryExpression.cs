using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// DB子查询操作
    /// </summary>
    public class DbSubQueryExpression : DbExpression
    {
        /// <summary>
        /// 子查询表达式
        /// </summary>
        public DbSqlQueryExpression SqlQuery { get; private set; }

        public DbSubQueryExpression(DbSqlQueryExpression sqlQuery)
            : base(DbExpressionType.SubQuery, UtilConstants.TypeOfVoid)
        {
            this.SqlQuery = sqlQuery;
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

    }
}
