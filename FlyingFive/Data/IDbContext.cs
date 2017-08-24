using System;
using System.Collections.Generic;
using System.Data;
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
        /// 查询实体对象
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        //IQuery<TEntity> Query<TEntity>();
        /// <summary>
        /// 根据主键查询实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="key"></param>
        /// <param name="tracking">是否开始跟踪</param>
        /// <returns></returns>
        TEntity QueryByKey<TEntity>(object key, bool tracking = false);
        /// <summary>
        /// 原生sql语句查询
        /// </summary>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <param name="plainSql">原生sql语句</param>
        /// <param name="cmdType"></param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        IEnumerable<T> SqlQuery<T>(string plainSql, CommandType cmdType = CommandType.Text, params FakeParameter[] parameters);
        /// <summary>
        /// 插入实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        TEntity Insert<TEntity>(TEntity entity);
        /// <summary>
        /// 插入实体,返回主键
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="body"></param>
        /// <returns>PrimaryKey</returns>
        //object Insert<TEntity>(Expression<Func<TEntity>> body);
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
        /// <summary>
        /// 跟踪实体
        /// </summary>
        /// <param name="entity"></param>
        void TrackEntity(object entity);
    }
}
