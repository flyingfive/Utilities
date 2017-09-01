using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data.Query.QueryExpressions
{
    public class WhereExpression : QueryExpression
    {
        public WhereExpression(QueryExpression prevExpression, Type elementType, LambdaExpression predicate)
            : base(QueryExpressionType.Where, elementType, prevExpression)
        {
            this.Predicate = predicate;
        }
        public LambdaExpression Predicate { get; private set; }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
