using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Query.QueryState
{
    public class GeneralQueryState : QueryStateBase, IQueryState
    {
        public GeneralQueryState(ResultElement resultElement)
            : base(resultElement)
        {
        }

        public override FromQueryResult ToFromQueryResult()
        {
            if (this.Result.Condition == null)
            {
                FromQueryResult result = new FromQueryResult();
                result.FromTable = this.Result.FromTable;
                result.MappingObjectExpression = this.Result.MappingObjectExpression;
                return result;
            }

            return base.ToFromQueryResult();
        }

    }
}
