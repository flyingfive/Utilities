using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示DB中的不等比较操作
    /// </summary>
    public class DbNotEqualExpression : DbBinaryExpression
    {
        public DbNotEqualExpression(DbExpression left, DbExpression right)
            : this(left, right, null)
        {

        }
        public DbNotEqualExpression(DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.NotEqual, UtilConstants.TypeOfBoolean, left, right, method)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
