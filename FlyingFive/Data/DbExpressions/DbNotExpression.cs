using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示DB取反操作
    /// </summary>
    public class DbNotExpression : DbExpression
    {
        /// <summary>
        /// 操作表达式
        /// </summary>
        public DbExpression Operand { get; private set; }

        public DbNotExpression(DbExpression exp)
            : base(DbExpressionType.Not, UtilConstants.TypeOfBoolean)
        {
            this.Operand = exp;
        }


        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
