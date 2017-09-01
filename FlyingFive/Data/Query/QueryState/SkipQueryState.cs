using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Query.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Query.QueryState
{
    internal sealed class SkipQueryState : SubQueryState
    {
        int _count;
        public SkipQueryState(ResultElement resultElement, int count)
            : base(resultElement)
        {
            this.Count = count;
        }

        public int Count
        {
            get
            {
                return this._count;
            }
            set
            {
                this.CheckInputCount(value);
                this._count = value;
            }
        }

        public override IQueryState Accept(SkipExpression exp)
        {
            if (exp.Count < 1)
            {
                return this;
            }

            this.Count += exp.Count;

            return this;
        }
        public override IQueryState Accept(TakeExpression exp)
        {
            var state = new LimitQueryState(this.Result, this.Count, exp.Count);
            return state;
        }
        public override IQueryState CreateQueryState(ResultElement result)
        {
            return new SkipQueryState(result, this.Count);
        }

        public override DbSqlQueryExpression CreateSqlQuery()
        {
            DbSqlQueryExpression sqlQuery = base.CreateSqlQuery();
            sqlQuery.TakeCount = null;
            sqlQuery.SkipCount = this.Count;
            return sqlQuery;
        }
    }
}
