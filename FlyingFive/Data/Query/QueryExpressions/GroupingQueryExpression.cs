using FlyingFive.Data.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data.Query.QueryExpressions
{
    public class GroupingQueryExpression : QueryExpression
    {
        //List<LambdaExpression> _groupKeySelectors = new List<LambdaExpression>();
        //List<LambdaExpression> _havingPredicates = new List<LambdaExpression>();
        //List<GroupingQueryOrdering> _orderings = new List<GroupingQueryOrdering>();
        public GroupingQueryExpression(Type elementType, QueryExpression prevExpression, LambdaExpression selector)
            : base(QueryExpressionType.GroupingQuery, elementType, prevExpression)
        {
            this.Selector = selector;
            this.GroupKeySelectors = new List<LambdaExpression>();
            this.HavingPredicates = new List<LambdaExpression>();
            this.Orderings = new List<GroupingQueryOrdering>();
        }

        public List<LambdaExpression> GroupKeySelectors { get; private set; }
        public List<LambdaExpression> HavingPredicates { get; private set; }
        public List<GroupingQueryOrdering> Orderings { get; private set; }
        public LambdaExpression Selector { get; private set; }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class GroupingQueryOrdering
    {
        public LambdaExpression KeySelector { get; private set; }
        public DbOrderType OrderType { get; private set; }
        public GroupingQueryOrdering(LambdaExpression keySelector, DbOrderType orderType)
        {
            this.KeySelector = keySelector;
            this.OrderType = orderType;
        }
    }
}
