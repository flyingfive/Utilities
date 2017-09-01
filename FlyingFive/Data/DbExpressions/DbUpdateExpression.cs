using FlyingFive.Data.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示DB中的一次更新操作
    /// </summary>
    public class DbUpdateExpression : DbExpression
    {
        /// <summary>
        /// 更新的表
        /// </summary>
        public DbTable Table { get; private set; }
        /// <summary>
        /// 更新的字段列表
        /// </summary>
        public Dictionary<DbColumn, DbExpression> UpdateColumns { get; private set; }
        /// <summary>
        /// 更新条件
        /// </summary>
        public DbExpression Condition { get; private set; }

        public DbUpdateExpression(DbTable table)
            : this(table, null)
        {
        }
        public DbUpdateExpression(DbTable table, DbExpression condition)
            : base(DbExpressionType.Update, UtilConstants.TypeOfVoid)
        {
            if (table == null) { throw new ArgumentNullException("参数: table不能为NULL"); }

            this.Table = table;
            this.Condition = condition;
            this.UpdateColumns = new Dictionary<DbColumn, DbExpression>();
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
