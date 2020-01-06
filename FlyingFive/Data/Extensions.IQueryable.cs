using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data
{
    public static partial class Extensions
    {
        /// <summary>
        /// 为IQueryable接口设置排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">查询对象</param>
        /// <param name="sort">排序名称</param>
        /// <param name="order">排序规则:asc/ desc</param>
        /// <returns></returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string sort, string order)
        {
            return query.OrderBy(sort, string.Equals(order, "asc", StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// 为IQueryable设置排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">查询对象</param>
        /// <param name="sort">排序名称</param>
        /// <param name="asc">排列为升序</param>
        /// <returns></returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string sort, bool asc)
        {
            if (string.IsNullOrEmpty(sort)) { throw new Exception("必须指定排序字段!"); }

            var sortProperty = typeof(T).GetProperty(sort, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (sortProperty == null) { throw new Exception("查询对象中不存在排序字段" + sort + "。"); }

            var param = Expression.Parameter(typeof(T), "t");
            Expression body = param;
            if (Nullable.GetUnderlyingType(body.Type) != null)
            {
                body = Expression.Property(body, "Value");
            }
            body = Expression.MakeMemberAccess(body, sortProperty);
            var keySelectorLambda = Expression.Lambda(body, param);
            var queryMethod = asc ? "OrderBy" : "OrderByDescending";
            query = query.Provider.CreateQuery<T>(Expression.Call(typeof(Queryable), queryMethod, new Type[] { typeof(T), body.Type }, query.Expression, Expression.Quote(keySelectorLambda)));
            return query;
        }
    }
}
