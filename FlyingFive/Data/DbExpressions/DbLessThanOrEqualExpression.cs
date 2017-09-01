using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示DB中的小于等于比较操作
    /// </summary>
    public class DbLessThanOrEqualExpression : DbBinaryExpression
    {
        public DbLessThanOrEqualExpression(DbExpression left, DbExpression right)
            : this(left, right, null)
        {

        }
        public DbLessThanOrEqualExpression(DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.LessThanOrEqual, UtilConstants.TypeOfBoolean, left, right, method)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
