using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 描述一次DB二元符号操作
    /// </summary>
    public abstract class DbBinaryExpression : DbExpression
    {
        /// <summary>
        /// 符号左边操作
        /// </summary>
        public DbExpression Left { get; private set; }
        /// <summary>
        /// 符号右边操作
        /// </summary>
        public DbExpression Right { get; private set; }
        /// <summary>
        /// 操作方法
        /// </summary>
        public MethodInfo Method { get; private set; }
        protected DbBinaryExpression(DbExpressionType nodeType, Type type, DbExpression left, DbExpression right)
            : this(nodeType, type, left, right, null)
        {
        }

        protected DbBinaryExpression(DbExpressionType nodeType, Type type, DbExpression left, DbExpression right, MethodInfo method)
            : base(nodeType, type)
        {
            this.Left = left;
            this.Right = right;
            this.Method = method;
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
