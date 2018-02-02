using FlyingFive.Data.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data
{
    public interface IJoiningQuery<T1, T2>
    {
        IJoiningQuery<T1, T2, T3> Join<T3>(DbJoinType joinType, Expression<Func<T1, T2, T3, bool>> on);
        IJoiningQuery<T1, T2, T3> Join<T3>(IQuery<T3> q, DbJoinType joinType, Expression<Func<T1, T2, T3, bool>> on);

        IJoiningQuery<T1, T2, T3> InnerJoin<T3>(Expression<Func<T1, T2, T3, bool>> on);
        IJoiningQuery<T1, T2, T3> LeftJoin<T3>(Expression<Func<T1, T2, T3, bool>> on);
        IJoiningQuery<T1, T2, T3> RightJoin<T3>(Expression<Func<T1, T2, T3, bool>> on);
        IJoiningQuery<T1, T2, T3> FullJoin<T3>(Expression<Func<T1, T2, T3, bool>> on);

        IJoiningQuery<T1, T2, T3> InnerJoin<T3>(IQuery<T3> q, Expression<Func<T1, T2, T3, bool>> on);
        IJoiningQuery<T1, T2, T3> LeftJoin<T3>(IQuery<T3> q, Expression<Func<T1, T2, T3, bool>> on);
        IJoiningQuery<T1, T2, T3> RightJoin<T3>(IQuery<T3> q, Expression<Func<T1, T2, T3, bool>> on);
        IJoiningQuery<T1, T2, T3> FullJoin<T3>(IQuery<T3> q, Expression<Func<T1, T2, T3, bool>> on);
        IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, TResult>> selector);
    }

    public interface IJoiningQuery<T1, T2, T3>
    {
        IJoiningQuery<T1, T2, T3, T4> Join<T4>(DbJoinType joinType, Expression<Func<T1, T2, T3, T4, bool>> on);
        IJoiningQuery<T1, T2, T3, T4> Join<T4>(IQuery<T4> q, DbJoinType joinType, Expression<Func<T1, T2, T3, T4, bool>> on);

        IJoiningQuery<T1, T2, T3, T4> InnerJoin<T4>(Expression<Func<T1, T2, T3, T4, bool>> on);
        IJoiningQuery<T1, T2, T3, T4> LeftJoin<T4>(Expression<Func<T1, T2, T3, T4, bool>> on);
        IJoiningQuery<T1, T2, T3, T4> RightJoin<T4>(Expression<Func<T1, T2, T3, T4, bool>> on);
        IJoiningQuery<T1, T2, T3, T4> FullJoin<T4>(Expression<Func<T1, T2, T3, T4, bool>> on);

        IJoiningQuery<T1, T2, T3, T4> InnerJoin<T4>(IQuery<T4> q, Expression<Func<T1, T2, T3, T4, bool>> on);
        IJoiningQuery<T1, T2, T3, T4> LeftJoin<T4>(IQuery<T4> q, Expression<Func<T1, T2, T3, T4, bool>> on);
        IJoiningQuery<T1, T2, T3, T4> RightJoin<T4>(IQuery<T4> q, Expression<Func<T1, T2, T3, T4, bool>> on);
        IJoiningQuery<T1, T2, T3, T4> FullJoin<T4>(IQuery<T4> q, Expression<Func<T1, T2, T3, T4, bool>> on);
        IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, TResult>> selector);
    }

    public interface IJoiningQuery<T1, T2, T3, T4>
    {
        IJoiningQuery<T1, T2, T3, T4, T5> Join<T5>(DbJoinType joinType, Expression<Func<T1, T2, T3, T4, T5, bool>> on);
        IJoiningQuery<T1, T2, T3, T4, T5> Join<T5>(IQuery<T5> q, DbJoinType joinType, Expression<Func<T1, T2, T3, T4, T5, bool>> on);

        IJoiningQuery<T1, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> on);
        IJoiningQuery<T1, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> on);
        IJoiningQuery<T1, T2, T3, T4, T5> RightJoin<T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> on);
        IJoiningQuery<T1, T2, T3, T4, T5> FullJoin<T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> on);

        IJoiningQuery<T1, T2, T3, T4, T5> InnerJoin<T5>(IQuery<T5> q, Expression<Func<T1, T2, T3, T4, T5, bool>> on);
        IJoiningQuery<T1, T2, T3, T4, T5> LeftJoin<T5>(IQuery<T5> q, Expression<Func<T1, T2, T3, T4, T5, bool>> on);
        IJoiningQuery<T1, T2, T3, T4, T5> RightJoin<T5>(IQuery<T5> q, Expression<Func<T1, T2, T3, T4, T5, bool>> on);
        IJoiningQuery<T1, T2, T3, T4, T5> FullJoin<T5>(IQuery<T5> q, Expression<Func<T1, T2, T3, T4, T5, bool>> on);
        IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> selector);
    }
    //remark:最多支持5个表的连接查询
    public interface IJoiningQuery<T1, T2, T3, T4, T5>
    {
        IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> selector);
    }
}
