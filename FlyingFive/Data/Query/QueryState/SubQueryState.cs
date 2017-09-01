using FlyingFive.Data.Query.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Query.QueryState
{
    internal abstract class SubQueryState : QueryStateBase
    {
        protected SubQueryState(ResultElement resultElement)
            : base(resultElement)
        {
        }

        public override IQueryState Accept(WhereExpression exp)
        {
            IQueryState state = this.AsSubQueryState();
            return state.Accept(exp);
        }
        public override IQueryState Accept(OrderExpression exp)
        {
            IQueryState state = this.AsSubQueryState();
            return state.Accept(exp);
        }
        public override IQueryState Accept(SkipExpression exp)
        {
            GeneralQueryState subQueryState = this.AsSubQueryState();

            SkipQueryState state = new SkipQueryState(subQueryState.Result, exp.Count);
            return state;
        }
        public override IQueryState Accept(TakeExpression exp)
        {
            GeneralQueryState subQueryState = this.AsSubQueryState();

            TakeQueryState state = new TakeQueryState(subQueryState.Result, exp.Count);
            return state;
        }
        public override IQueryState Accept(AggregateQueryExpression exp)
        {
            IQueryState subQueryState = this.AsSubQueryState();

            IQueryState state = subQueryState.Accept(exp);
            return state;
        }

        protected void CheckInputCount(int count)
        {
            if (count < 0)
            {
                throw new ArgumentException("The count could not less than 0.");
            }
        }
    }
}
