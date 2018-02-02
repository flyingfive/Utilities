using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Query.QueryExpressions
{
    public class AggregateQueryExpression : QueryExpression
    {
        public MethodInfo Method { get; private set; }
        public ReadOnlyCollection<Expression> Arguments { get; private set; }

        public AggregateQueryExpression(QueryExpression prevExpression, MethodInfo method, IList<Expression> arguments)
            : base(QueryExpressionType.Aggregate, method.ReturnType, prevExpression)
        {
            this.Method = method;
            this.Arguments = new ReadOnlyCollection<Expression>(arguments);
        }



        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
