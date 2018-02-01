using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data
{
    /// <summary>
    /// 表示DB上下文
    /// </summary>
    public interface IDbContext : IDisposable
    {
        /// <summary>
        /// 该DB上下文上的DB会话
        /// </summary>
        IDbSession Session { get; }
        /// <summary>
        /// 根据主键查询实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="key"></param>
        /// <param name="tracking">是否开始跟踪</param>
        /// <returns></returns>
        TEntity QueryByKey<TEntity>(object key, bool tracking = false);
        /// <summary>
        /// 插入实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        TEntity Insert<TEntity>(TEntity entity);
        /// <summary>
        /// 更新实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        int Update<TEntity>(TEntity entity);
        /// <summary>
        /// 更新实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="condition">条件表达式</param>
        /// <param name="body"></param>
        /// <returns></returns>
        int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> body);
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        int Delete<TEntity>(TEntity entity);
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="condition">条件表达式</param>
        /// <returns></returns>
        int Delete<TEntity>(Expression<Func<TEntity, bool>> condition);
        /// <summary>
        /// 根据主键删除实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        int DeleteByKey<TEntity>(object key);
    }
}
