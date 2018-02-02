using FlyingFive.Data.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示DB中的一次删除操作
    /// </summary>
    public class DbDeleteExpression : DbExpression
    {
        /// <summary>
        /// 删除的表
        /// </summary>
        public DbTable Table { get; private set; }
        /// <summary>
        /// 删除条件
        /// </summary>
        public DbExpression Condition { get; private set; }

        public DbDeleteExpression(DbTable table)
            : this(table, null)
        {
        }

        public DbDeleteExpression(DbTable table, DbExpression condition)
            : base(DbExpressionType.Delete, UtilConstants.TypeOfVoid)
        {
            if (table == null)
            {
                throw new ArgumentNullException("参数: table不能为NULL");
            }
            this.Table = table;
            this.Condition = condition;
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
