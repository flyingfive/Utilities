using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Query.Mapping;
using FlyingFive.Data.Query.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data.Query.QueryState
{
    public interface IQueryState
    {
        MappingData GenerateMappingData();

        FromQueryResult ToFromQueryResult();
        JoinQueryResult ToJoinQueryResult(DbJoinType joinType, LambdaExpression conditionExpression, DbFromTableExpression fromTable, List<IMappingObjectExpression> moeList, string tableAlias);

        IQueryState Accept(WhereExpression exp);
        IQueryState Accept(OrderExpression exp);
        IQueryState Accept(SelectExpression exp);
        IQueryState Accept(SkipExpression exp);
        IQueryState Accept(TakeExpression exp);
        IQueryState Accept(AggregateQueryExpression exp);
        IQueryState Accept(GroupingQueryExpression exp);
    }
}
