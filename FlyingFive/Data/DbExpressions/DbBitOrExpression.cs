using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// DB按位或操作
    /// </summary>
    public class DbBitOrExpression : DbBinaryExpression
    {
        public DbBitOrExpression(Type type, DbExpression left, DbExpression right)
            : base(DbExpressionType.BitOr, type, left, right)
        {
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
