using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data
{
    public interface IOrderedQuery<T> : IQuery<T>
    {
        IOrderedQuery<T> ThenBy<K>(Expression<Func<T, K>> keySelector);
        IOrderedQuery<T> ThenByDesc<K>(Expression<Func<T, K>> keySelector);
    }
}
