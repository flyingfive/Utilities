using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示DB中的减法操作
    /// </summary>
    public class DbSubtractExpression : DbBinaryExpression
    {
        public DbSubtractExpression(Type type, DbExpression left, DbExpression right)
            : this(type, left, right, null)
        {

        }
        public DbSubtractExpression(Type type, DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.Subtract, type, left, right, method)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
