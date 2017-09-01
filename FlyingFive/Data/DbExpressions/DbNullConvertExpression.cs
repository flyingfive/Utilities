using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示DB中的NULL转换操作
    /// </summary>
    public class DbNullConvertExpression : DbExpression
    {
        /// <summary>
        /// NULL值检查操作
        /// </summary>
        public DbExpression CheckExpression { get; private set; }
        /// <summary>
        /// DB中的NULL值替换操作
        /// </summary>
        public DbExpression ReplacementValue { get; private set; }

        public DbNullConvertExpression(DbExpression checkExpression, DbExpression replacementValue)
            : base(DbExpressionType.NullConvert, replacementValue.Type)
        {
            if (checkExpression == null) { throw new ArgumentNullException("参数: checkExpression不能为NULL"); }
            if (replacementValue == null) { throw new ArgumentNullException("参数: replacementValue不能为NULL"); }

            this.CheckExpression = checkExpression;
            this.ReplacementValue = replacementValue;
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
