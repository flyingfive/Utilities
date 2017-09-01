using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示DB中的加法操作
    /// </summary>
    public class DbAddExpression : DbBinaryExpression
    {
        public DbAddExpression(Type type, DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.Add, type, left, right, method)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
