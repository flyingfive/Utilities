using FlyingFive.Data.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 描述DB中的表访问操作
    /// </summary>
    public class DbTableExpression : DbExpression
    {
        /// <summary>
        /// 访问的DB表
        /// </summary>
        public DbTable Table { get; private set; }
        public DbTableExpression(DbTable table)
            : base(DbExpressionType.Table, UtilConstants.TypeOfVoid)
        {
            this.Table = table;
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
