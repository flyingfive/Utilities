using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data.Query.QueryExpressions
{
    public class SelectExpression : QueryExpression
    {
        public SelectExpression(Type elementType, QueryExpression prevExpression, LambdaExpression selector)
            : base(QueryExpressionType.Select, elementType, prevExpression)
        {
            this.Selector = selector;
        }
        public LambdaExpression Selector
        {
            get;
            private set;
        }
        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
