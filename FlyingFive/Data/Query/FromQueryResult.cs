using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Query.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Query
{
    public class FromQueryResult
    {
        public DbFromTableExpression FromTable { get; set; }
        public IMappingObjectExpression MappingObjectExpression { get; set; }
    }

    public class JoinQueryResult
    {
        public IMappingObjectExpression MappingObjectExpression { get; set; }
        public DbJoinTableExpression JoinTable { get; set; }
        //public DbExpression LeftKeySelector { get; set; }
        //public DbExpression RightKeySelector { get; set; }
    }
}
