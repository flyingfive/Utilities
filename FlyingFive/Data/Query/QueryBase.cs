using FlyingFive.Data.Query.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Query
{
    public abstract class QueryBase
    {
        public abstract QueryExpression QueryExpression { get; }
        public abstract bool TrackEntity { get; }
    }
}
