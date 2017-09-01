using FlyingFive.Data.Interception;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data
{
    /// <summary>
    /// 表示一个DB会话
    /// </summary>
    public interface IDbSession : IDisposable
    {
        /// <summary>
        /// 该会话上的DB上下文
        /// </summary>
        DbContext DbContext { get; }
        /// <summary>
        /// 会话是否在事务处理中
        /// </summary>
        bool IsInTransaction { get; }
        /// <summary>
        /// 该会话下的数据库操作超时时间(单位：秒)
        /// </summary>
        int CommandTimeout { get; set; }
        /// <summary>
        /// 执行非查询语句
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="cmdType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecuteNonQuery(string cmdText, CommandType cmdType, params FakeParameter[] parameters);
        /// <summary>
        /// 执行单一查询语句
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="cmdType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object ExecuteScalar(string cmdText, CommandType cmdType, params FakeParameter[] parameters);
        /// <summary>
        /// 执行DataReader查询语句
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="cmdType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IDataReader ExecuteReader(string cmdText, CommandType cmdType, params FakeParameter[] parameters);
        /// <summary>
        /// 开始事务
        /// </summary>
        void BeginTransaction();
        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="il"></param>
        void BeginTransaction(IsolationLevel il);
        /// <summary>
        /// 提交事务
        /// </summary>
        void CommitTransaction();
        /// <summary>
        /// 回滚撤销事务
        /// </summary>
        void RollbackTransaction();
        /// <summary>
        /// 为该会话添加一个命令拦截器
        /// </summary>
        /// <param name="interceptor"></param>
        void AddInterceptor(IDbCommandInterceptor interceptor);
        /// <summary>
        /// 从本会话移除一个拦截器
        /// </summary>
        /// <param name="interceptor"></param>
        void RemoveInterceptor(IDbCommandInterceptor interceptor);
    }
}
