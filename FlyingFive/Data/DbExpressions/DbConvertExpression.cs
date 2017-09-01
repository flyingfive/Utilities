using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// DB转换表现
    /// </summary>
    public class DbConvertExpression : DbExpression
    {
        /// <summary>
        /// 转换操作对象
        /// </summary>
        public DbExpression Operand { get; private set; }

        public DbConvertExpression(Type type, DbExpression operand)
            : base(DbExpressionType.Convert, type)
        {
            this.Operand = operand;
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
