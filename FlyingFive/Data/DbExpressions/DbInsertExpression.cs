using FlyingFive.Data.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// DB插入语句表现
    /// </summary>
    public class DbInsertExpression : DbExpression
    {
        /// <summary>
        /// 插入的表
        /// </summary>
        public DbTable Table { get; private set; }
        /// <summary>
        /// 插入的字段列表
        /// </summary>
        public Dictionary<DbColumn, DbExpression> InsertColumns { get; private set; }
        public DbInsertExpression(DbTable table)
            : base(DbExpressionType.Insert, typeof(void))
        {
            if (table == null) { throw new ArgumentNullException("参数: table不能为NULL"); }
            this.Table = table;
            this.InsertColumns = new Dictionary<DbColumn, DbExpression>();
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
