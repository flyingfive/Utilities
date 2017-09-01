using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 查询中的主表操作
    /// </summary>
    public abstract class DbMainTableExpression : DbExpression
    {
        /// <summary>
        /// 查询的主表部分
        /// </summary>
        public DbTableSegment Table { get; private set; }
        /// <summary>
        /// JOIN连接查询部分
        /// </summary>
        public List<DbJoinTableExpression> JoinTables { get; private set; }

        protected DbMainTableExpression(DbExpressionType nodeType, DbTableSegment table)
            : base(nodeType)
        {
            this.Table = table;
            this.JoinTables = new List<DbJoinTableExpression>();
        }
    }

    /// <summary>
    /// 查询中FROM子句的表访问操作
    /// </summary>
    public class DbFromTableExpression : DbMainTableExpression
    {
        public DbFromTableExpression(DbTableSegment table)
            : base(DbExpressionType.FromTable, table)
        {
        }
        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }


    /// <summary>
    /// 表示查询语句中的表部分
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Alias = {Alias}")]
    public class DbTableSegment
    {
        /// <summary>
        /// 查询中的表访问操作
        /// </summary>
        public DbExpression Body { get; private set; }
        /// <summary>
        /// 访问该表所产生的别名
        /// </summary>
        public string Alias { get; private set; }

        public DbTableSegment(DbExpression body, string alias)
        {
            this.Body = body;
            this.Alias = alias;
        }

    }
}
