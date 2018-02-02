using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Query.QueryExpressions
{
    public class RootQueryExpression : QueryExpression
    {
        public RootQueryExpression(Type elementType, string explicitTable)
            : base(QueryExpressionType.Root, elementType, null)
        {
            this.ExplicitTable = explicitTable;
        }
        public string ExplicitTable { get; private set; }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
