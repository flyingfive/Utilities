using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Query.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Query.QueryState
{
    internal sealed class LimitQueryState : SubQueryState
    {
        int _skipCount;
        int _takeCount;
        public LimitQueryState(ResultElement resultElement, int skipCount, int takeCount)
            : base(resultElement)
        {
            this.SkipCount = skipCount;
            this.TakeCount = takeCount;
        }

        public int SkipCount
        {
            get
            {
                return this._skipCount;
            }
            set
            {
                this.CheckInputCount(value);
                this._skipCount = value;
            }
        }
        public int TakeCount
        {
            get
            {
                return this._takeCount;
            }
            set
            {
                this.CheckInputCount(value);
                this._takeCount = value;
            }
        }


        public override IQueryState Accept(TakeExpression exp)
        {
            if (exp.Count < this.TakeCount)
                this.TakeCount = exp.Count;

            return this;
        }
        public override IQueryState CreateQueryState(ResultElement result)
        {
            return new LimitQueryState(result, this.SkipCount, this.TakeCount);
        }

        public override DbSqlQueryExpression CreateSqlQuery()
        {
            DbSqlQueryExpression sqlQuery = base.CreateSqlQuery();

            sqlQuery.TakeCount = this.TakeCount;
            sqlQuery.SkipCount = this.SkipCount;

            return sqlQuery;
        }
    }
}
