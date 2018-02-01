using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Interception
{
    /// <summary>
    /// 表示System.Data.IDbCommand对象的拦截器
    /// </summary>
    public interface IDbCommandInterceptor
    {
        /// <summary>
        /// 执行command对象的DataReader方法前
        /// </summary>
        /// <param name="command">执行命令的System.Data.IDbCommand对象</param>
        /// <param name="interceptionContext">拦截器信息</param>
        void DataAdapterExecuting(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext);
        /// <summary>
        /// 执行command对象的DataReader方法后
        /// </summary>
        /// <param name="command">执行命令的System.Data.IDbCommand对象</param>
        /// <param name="interceptionContext">拦截器信息</param>
        void DataAdapterExecuted(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext);
        /// <summary>
        /// 执行command对象的DataReader方法前
        /// </summary>
        /// <param name="command">执行命令的System.Data.IDbCommand对象</param>
        /// <param name="interceptionContext">拦截器信息</param>
        void ReaderExecuting(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext);
        /// <summary>
        /// 执行command对象的DataReader方法后
        /// </summary>
        /// <param name="command">执行命令的System.Data.IDbCommand对象</param>
        /// <param name="interceptionContext">拦截器信息</param>
        void ReaderExecuted(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext);

        /// <summary>
        /// 执行command对象的NonQuery方法前
        /// </summary>
        /// <param name="command">执行命令的System.Data.IDbCommand对象</param>
        /// <param name="interceptionContext">拦截器信息</param>
        void NonQueryExecuting(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext);
        /// <summary>
        /// 执行command对象的NonQuery方法后
        /// </summary>
        /// <param name="command">执行命令的System.Data.IDbCommand对象</param>
        /// <param name="interceptionContext">拦截器信息</param>
        void NonQueryExecuted(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext);

        /// <summary>
        /// 执行command对象的Scalar方法前
        /// </summary>
        /// <param name="command">执行命令的System.Data.IDbCommand对象</param>
        /// <param name="interceptionContext">拦截器信息</param>
        void ScalarExecuting(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext);
        /// <summary>
        /// 执行command对象的Scalar方法后
        /// </summary>
        /// <param name="command">执行命令的System.Data.IDbCommand对象</param>
        /// <param name="interceptionContext">拦截器信息</param>
        void ScalarExecuted(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext);
    }
}
