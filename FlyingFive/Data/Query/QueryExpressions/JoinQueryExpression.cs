using FlyingFive.Data.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data.Query.QueryExpressions
{
    public class JoinQueryExpression : QueryExpression
    {
        public JoinQueryExpression(Type elementType, QueryExpression prevExpression, List<JoiningQueryInfo> joinedQueries, LambdaExpression selector)
            : base(QueryExpressionType.JoinQuery, elementType, prevExpression)
        {
            this.JoinedQueries = new List<JoiningQueryInfo>(joinedQueries.Count);
            this.JoinedQueries.AddRange(joinedQueries);
            this.Selector = selector;
        }

        public List<JoiningQueryInfo> JoinedQueries { get; private set; }
        public LambdaExpression Selector { get; private set;  }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class JoiningQueryInfo
    {
        public JoiningQueryInfo(QueryBase query, DbJoinType joinType, LambdaExpression condition)
        {
            this.Query = query;
            this.JoinType = joinType;
            this.Condition = condition;
        }
        public QueryBase Query { get; set; }
        public DbJoinType JoinType { get; set; }
        public LambdaExpression Condition { get; set; }
    }
}
