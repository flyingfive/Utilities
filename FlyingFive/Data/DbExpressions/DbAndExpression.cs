using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// DB并且操作
    /// </summary>
    public class DbAndExpression : DbBinaryExpression
    {
        public DbAndExpression(DbExpression left, DbExpression right)
            : this(left, right, null)
        {

        }
        public DbAndExpression(DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.And, UtilConstants.TypeOfBoolean, left, right, method)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
