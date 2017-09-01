using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示一次DB查询操作
    /// </summary>
    public class DbSqlQueryExpression : DbExpression
    {
        public DbSqlQueryExpression()
            : base(DbExpressionType.SqlQuery, UtilConstants.TypeOfVoid)
        {
            this.ColumnSegments = new List<DbColumnSegment>();
            this.GroupSegments = new List<DbExpression>();
            this.Orderings = new List<DbOrdering>();
        }
        /// <summary>
        /// 分页逻辑中的获取记录条数
        /// </summary>
        public int? TakeCount { get; set; }
        /// <summary>
        /// 分页逻辑中的跳过记录条数
        /// </summary>
        public int? SkipCount { get; set; }
        /// <summary>
        /// 查询中SELECT的字段列表
        /// </summary>
        public List<DbColumnSegment> ColumnSegments { get; private set; }
        /// <summary>
        /// 查询的FROM主表
        /// </summary>
        public DbFromTableExpression Table { get; set; }
        /// <summary>
        /// 查询中的WHERE条件逻辑
        /// </summary>
        public DbExpression Condition { get; set; }
        /// <summary>
        /// 查询中的GROUP分组字段列表
        /// </summary>
        public List<DbExpression> GroupSegments { get; private set; }
        /// <summary>
        /// 查询中的分组后HAVING条件逻辑
        /// </summary>
        public DbExpression HavingCondition { get; set; }
        /// <summary>
        /// 查询中的排序逻辑
        /// </summary>
        public List<DbOrdering> Orderings { get; private set; }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }


    /// <summary>
    /// 表示查询中的字段部分
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Alias = {Alias}")]
    public class DbColumnSegment
    {
        /// <summary>
        /// T.Name 部分
        /// </summary>
        public DbExpression Body { get; private set; }
        /// <summary>
        /// 列别名
        /// </summary>
        public string Alias { get; private set; }

        public DbColumnSegment(DbExpression body, string alias)
        {
            this.Body = body;
            this.Alias = alias;
        }

    }
}
