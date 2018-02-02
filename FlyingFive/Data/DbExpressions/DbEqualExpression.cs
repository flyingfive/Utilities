using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示DB中的相等比较操作
    /// </summary>
    public class DbEqualExpression : DbBinaryExpression
    {
        /// <summary>
        /// 表示返回结果为True的DB比较操作
        /// </summary>
        public static readonly DbEqualExpression True = new DbEqualExpression(new DbConstantExpression(1), new DbConstantExpression(1));
        /// <summary>
        /// 表示返回结果为False的DB比较操作
        /// </summary>
        public static readonly DbEqualExpression False = new DbEqualExpression(new DbConstantExpression(1), new DbConstantExpression(0));

        public DbEqualExpression(DbExpression left, DbExpression right)
            : this(left, right, null)
        {
        }
        public DbEqualExpression(DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.Equal, UtilConstants.TypeOfBoolean, left, right, method)
        {
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
