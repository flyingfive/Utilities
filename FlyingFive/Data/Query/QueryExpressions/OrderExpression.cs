using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data.Query.QueryExpressions
{
    public class OrderExpression : QueryExpression
    {
        public OrderExpression(QueryExpressionType expressionType, Type elementType, QueryExpression prevExpression, LambdaExpression keySelector)
            : base(expressionType, elementType, prevExpression)
        {
            this.KeySelector = keySelector;
        }
        public LambdaExpression KeySelector { get; private set; }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
