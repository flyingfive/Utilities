using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Query.QueryExpressions
{
    public class TakeExpression : QueryExpression
    {
        public TakeExpression(Type elementType, QueryExpression prevExpression, int count)
            : base(QueryExpressionType.Take, elementType, prevExpression)
        {
            this.CheckInputCount(count);
            this.Count = count;
        }

        public int Count { get; private set; }

        private void CheckInputCount(int count)
        {
            if (count < 0)
            {
                throw new ArgumentException("count 小于 0");
            }
        }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
