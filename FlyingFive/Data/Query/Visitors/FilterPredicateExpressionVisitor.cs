using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Query.Mapping;
using FlyingFive.Data.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data.Query.Visitors
{
    internal class FilterPredicateExpressionVisitor : ExpressionVisitor<DbExpression>
    {
        public static DbExpression ParseFilterPredicate(LambdaExpression lambda, List<IMappingObjectExpression> moeList)
        {
            return GeneralExpressionVisitor.ParseLambda(ExpressionVisitorBase.ReBuildFilterPredicate(lambda), moeList);
        }
    }
}
