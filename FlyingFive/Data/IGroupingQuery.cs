using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data
{
    public interface IGroupingQuery<T>
    {
        IGroupingQuery<T> AndBy<K>(Expression<Func<T, K>> keySelector);
        IGroupingQuery<T> Having(Expression<Func<T, bool>> predicate);
        IOrderedGroupingQuery<T> OrderBy<K>(Expression<Func<T, K>> keySelector);
        IOrderedGroupingQuery<T> OrderByDesc<K>(Expression<Func<T, K>> keySelector);
        IQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> selector);
    }

    public interface IOrderedGroupingQuery<T> : IGroupingQuery<T>
    {
        IOrderedGroupingQuery<T> ThenBy<K>(Expression<Func<T, K>> keySelector);
        IOrderedGroupingQuery<T> ThenByDesc<K>(Expression<Func<T, K>> keySelector);
    }
}
