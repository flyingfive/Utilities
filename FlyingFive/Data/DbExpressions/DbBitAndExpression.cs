using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// DB按位且操作
    /// </summary>
    public class DbBitAndExpression : DbBinaryExpression
    {
        public DbBitAndExpression(Type type, DbExpression left, DbExpression right)
            : base(DbExpressionType.BitAnd, type, left, right)
        {
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
