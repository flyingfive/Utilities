using FlyingFive.Data.Fakes;
using FlyingFive.Data.Interception;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data
{
    /// <summary>
    /// DB辅助工具接口
    /// </summary>
    public interface IDatabaseHelper : IDisposable
    {
        /// <summary>
        /// DB工具是否在事务处理过程中
        /// </summary>
        bool IsInTransaction { get; }
        /// <summary>
        /// DB辅助工具中当前正在执行的内部事务
        /// </summary>
        IDbTransaction UnderlyingTransaction { get; }
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
        int ExecuteNonQuery(string cmdText, CommandType cmdType = CommandType.Text, params FakeParameter[] parameters);
        /// <summary>
        /// 执行单一查询语句
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="cmdType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object ExecuteScalar(string cmdText, CommandType cmdType = CommandType.Text, params FakeParameter[] parameters);
        /// <summary>
        /// 执行DataReader查询语句
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="cmdType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IDataReader ExecuteReader(string cmdText, CommandType cmdType = CommandType.Text, params FakeParameter[] parameters);
        /// <summary>
        /// 开始事务
        /// </summary>
        IDbTransaction BeginTransaction();
        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="il"></param>
        IDbTransaction BeginTransaction(IsolationLevel il);
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
