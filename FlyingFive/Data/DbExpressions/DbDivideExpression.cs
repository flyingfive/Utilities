using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示DB中的除法操作
    /// </summary>
    public class DbDivideExpression : DbBinaryExpression
    {
        public DbDivideExpression(Type type, DbExpression left, DbExpression right)
            : this(type, left, right, null)
        {

        }
        public DbDivideExpression(Type type, DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.Divide, type, left, right, method)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
