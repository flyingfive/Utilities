using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Query.QueryState
{
    internal class AggregateQueryState : QueryStateBase, IQueryState
    {
        public AggregateQueryState(ResultElement resultElement)
            : base(resultElement)
        {
        }
    }
}
