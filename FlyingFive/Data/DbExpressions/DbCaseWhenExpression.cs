using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// DB中的SWITCH CASE条件判断操作
    /// </summary>
    public class DbCaseWhenExpression : DbExpression
    {
        public ReadOnlyCollection<WhenThenExpressionPair> WhenThenPairs { get; private set; }
        public DbExpression Else { get; private set; }
        public DbCaseWhenExpression(Type type, IList<WhenThenExpressionPair> whenThenPairs, DbExpression elseExp)
            : base(DbExpressionType.CaseWhen, type)
        {
            this.WhenThenPairs = new ReadOnlyCollection<WhenThenExpressionPair>(whenThenPairs);
            this.Else = elseExp;
        }


        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        /// <summary>
        /// DB条件结构
        /// </summary>
        public struct WhenThenExpressionPair
        {
            private DbExpression _when;
            private DbExpression _then;
            /// <summary>
            /// 条件判断部分操作
            /// </summary>
            public DbExpression When { get { return _when; } }
            /// <summary>
            /// 条件成立时的操作
            /// </summary>
            public DbExpression Then { get { return _then; } }
            public WhenThenExpressionPair(DbExpression when, DbExpression then)
            {
                this._when = when;
                this._then = then;
            }

        }
    }
}
