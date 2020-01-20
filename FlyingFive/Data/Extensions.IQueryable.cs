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
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string sort, string order, string sort2 = "", string order2 = "")
        {
            return query.OrderBy(sort, string.Equals(order, "asc", StringComparison.CurrentCultureIgnoreCase)
                , sort2, string.Equals(order2, "asc", StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// 为IQueryable设置排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">查询对象</param>
        /// <param name="sort">排序名称</param>
        /// <param name="asc">排列为升序（如果存在）</param>
        /// <param name="sort2">第二排序名称</param>
        /// <param name="asc2">第二排列为升序</param>
        /// <returns></returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string sort, bool asc, string sort2 = "", bool asc2 = true)
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
            var lambda = Expression.Lambda(body, param);
            var methodName = asc ? "OrderBy" : "OrderByDescending";
            query = query.Provider.CreateQuery<T>(Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(T), body.Type }, query.Expression, Expression.Quote(lambda)));
            if (!string.IsNullOrWhiteSpace(sort2))
            {
                var sortProperty2 = typeof(T).GetProperty(sort2, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (sortProperty2 == null) { throw new Exception("查询对象中不存在排序字段" + sort2 + "。"); }

                var param2 = Expression.Parameter(typeof(T), "t");
                Expression body2 = param2;
                if (Nullable.GetUnderlyingType(body2.Type) != null)
                {
                    body2 = Expression.Property(body2, "Value");
                }
                body2 = Expression.MakeMemberAccess(body2, sortProperty2);
                var lambda2 = Expression.Lambda(body2, param2);

                var methodName2 = asc2 ? "ThenBy" : "ThenByDescending";
                query = query.Provider.CreateQuery<T>(Expression.Call(typeof(Queryable), methodName2, new Type[] { typeof(T), body2.Type }, query.Expression, Expression.Quote(lambda2)));
            }
            return query;
        }
    }
}
