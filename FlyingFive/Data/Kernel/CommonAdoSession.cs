using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using FlyingFive.Data.Interception;
using FlyingFive.Data.Fakes;

namespace FlyingFive.Data.Kernel
{
    /// <summary>
    /// 与驱动无关的Ado.NET处理DB的通用会话
    /// </summary>
    public class CommonAdoSession : IDisposable
    {
        private bool _disposed = false;

        private IDbConnection _dbConnection = null;
        /// <summary>
        /// 内部事务对象
        /// </summary>
        internal IDbTransaction Transaction { get; private set; }
        /// <summary>
        /// 会话是否在事务处理中
        /// </summary>
        public bool IsInTransaction { get; private set; }
        /// <summary>
        /// 该会话处理DB操作的超时时间,单位:秒
        /// </summary>
        public int CommandTimeout { get; set; }

        /// <summary>
        /// 该会话启用的拦截器列表
        /// </summary>
        public IList<IDbCommandInterceptor> DbCommandInterceptors { get; private set; }

        public CommonAdoSession(IDbConnection dbConnection)
        {
            this.CommandTimeout = 30;
            this._dbConnection = dbConnection;
            this.DbCommandInterceptors = new List<IDbCommandInterceptor>();
        }

        /// <summary>
        /// 执行非查询语句,返回受影响行数
        /// </summary>
        /// <param name="cmdText">命令语句</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="parameters">参数列表</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string cmdText, CommandType cmdType, params FakeParameter[] parameters)
        {
            this.CheckDisposed();
            IDbCommand command = null;
            try
            {
                List<OutputParameter> outputParameters = null;
                command = this.PrepareCommand(cmdText, parameters, cmdType, out outputParameters);

                var dbCommandInterceptionContext = new DbCommandInterceptionContext<int>();
                var globalInterceptors = GlobalDbInterception.GetInterceptors();

                this.Activate();
                this.OnNonQueryExecuting(command, dbCommandInterceptionContext, globalInterceptors);
                int rowsAffected = -1;
                try
                {
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    dbCommandInterceptionContext.Exception = ex;
                    dbCommandInterceptionContext.Result = rowsAffected;
                    this.OnNonQueryExecuted(command, dbCommandInterceptionContext, globalInterceptors);
                    throw WrapException(ex);
                }
                dbCommandInterceptionContext.Result = rowsAffected;
                this.OnNonQueryExecuted(command, dbCommandInterceptionContext, globalInterceptors);
                OutputParameter.CallMapValue(outputParameters);
                return rowsAffected;
            }
            finally
            {
                this.Complete();
                if (command != null) { command.Dispose(); }
            }
        }

        /// <summary>
        /// 执行查询语句,获取单一值
        /// </summary>
        /// <param name="cmdText">命令语句</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="parameters">参数列表</param>
        /// <returns></returns>
        public object ExecuteScalar(string cmdText, CommandType cmdType, params FakeParameter[] parameters)
        {
            this.CheckDisposed();
            IDbCommand command = null;
            try
            {
                List<OutputParameter> outputParameters = null;
                command = this.PrepareCommand(cmdText, parameters, cmdType, out outputParameters);
                var dbCommandInterceptionContext = new DbCommandInterceptionContext<object>();
                var globalInterceptors = GlobalDbInterception.GetInterceptors();
                this.Activate();
                this.OnScalarExecuting(command, dbCommandInterceptionContext, globalInterceptors);

                object ret = null;
                try
                {
                    ret = command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    dbCommandInterceptionContext.Exception = ex;
                    this.OnScalarExecuted(command, dbCommandInterceptionContext, globalInterceptors);
                    throw WrapException(ex);
                }

                dbCommandInterceptionContext.Result = ret;
                this.OnScalarExecuted(command, dbCommandInterceptionContext, globalInterceptors);
                OutputParameter.CallMapValue(outputParameters);
                return ret;
            }
            finally
            {
                this.Complete();
                if (command != null) { command.Dispose(); }
            }
        }

        /// <summary>
        /// 执行查询语句,获取一个DataReader读取器
        /// </summary>
        /// <param name="cmdText">命令语句</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="parameters">参数列表</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string cmdText, CommandType cmdType, FakeParameter[] parameters)
        {
            return this.ExecuteReader(cmdText, cmdType, CommandBehavior.Default, parameters);
        }

        /// <summary>
        /// 执行查询语句,获取一个DataReader读取器
        /// </summary>
        /// <param name="cmdText">命令语句</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="behavior">命令执行行为</param>
        /// <param name="parameters">参数列表</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string cmdText, CommandType cmdType, CommandBehavior behavior, FakeParameter[] parameters)
        {
            this.CheckDisposed();
            List<OutputParameter> outputParameters = null;
            var command = this.PrepareCommand(cmdText, parameters, cmdType, out outputParameters);

            var dbCommandInterceptionContext = new DbCommandInterceptionContext<IDataReader>();
            var globalInterceptors = GlobalDbInterception.GetInterceptors();
            this.Activate();
            this.OnReaderExecuting(command, dbCommandInterceptionContext, globalInterceptors);

            IDataReader reader;
            try
            {
                reader = new FakeDataReader(this, command.ExecuteReader(behavior), command, outputParameters);
            }
            catch (Exception ex)
            {
                dbCommandInterceptionContext.Exception = ex;
                this.OnReaderExecuted(command, dbCommandInterceptionContext, globalInterceptors);
                throw WrapException(ex);
            }
            dbCommandInterceptionContext.Result = reader;
            this.OnReaderExecuted(command, dbCommandInterceptionContext, globalInterceptors);
            return reader;
        }


        public void Dispose()
        {
            if (this._disposed) { return; }
            if (this.Transaction != null && this.IsInTransaction)
            {
                try
                {
                    this.Transaction.Rollback();
                }
                catch
                {
                }
                this.ReleaseTransaction();
            }
            if (this._dbConnection != null)
            {
                this._dbConnection.Dispose();
            }
            this._disposed = true;
        }

        #region 事务处理

        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="il"></param>
        public void BeginTransaction(IsolationLevel? il = null)
        {
            this.Activate();
            if (il.HasValue)
            {
                this.Transaction = this._dbConnection.BeginTransaction(il.Value);
            }
            else
            {
                this.Transaction = this._dbConnection.BeginTransaction();
            }
            this.IsInTransaction = true;
        }

        /// <summary>
        /// 提交当前会话中的事务
        /// </summary>
        public void CommitTransaction()
        {
            if (!this.IsInTransaction || this.Transaction == null)
            {
                throw new DataAccessException("当前会话没有开启事务.");
            }
            this.Transaction.Commit();
            this.ReleaseTransaction();
            this.Complete();
        }

        /// <summary>
        /// 回滚当前会话中的事务
        /// </summary>
        public void RollbackTransaction()
        {
            if (!this.IsInTransaction || this.Transaction == null)
            {
                throw new DataAccessException("当前会话没有开启事务.");
            }
            this.Transaction.Rollback();
            this.ReleaseTransaction();
            this.Complete();
        }

        /// <summary>
        /// 释放会话中存在的事务
        /// </summary>
        private void ReleaseTransaction()
        {
            this.Transaction.Dispose();
            this.Transaction = null;
            this.IsInTransaction = false;
        }

        #endregion

        private IDbCommand PrepareCommand(string cmdText, FakeParameter[] parameters, CommandType cmdType, out List<OutputParameter> outputParameters)
        {
            outputParameters = null;
            IDbCommand command = this._dbConnection.CreateCommand();
            command.CommandText = cmdText;
            command.CommandType = cmdType;
            command.CommandTimeout = this.CommandTimeout;
            if (this.IsInTransaction && this.Transaction != null)
            {
                command.Transaction = this.Transaction;
            }
            if (parameters == null || parameters.Length <= 0) { return command; }
            foreach (var fakeParameter in parameters)
            {
                if (fakeParameter == null) { continue; }
                if (fakeParameter.ExplicitParameter != null)
                {
                    command.Parameters.Add(fakeParameter.ExplicitParameter);
                    continue;
                }
                var dbParameter = command.CreateParameter();
                dbParameter.ParameterName = fakeParameter.Name;
                Type parameterType;
                if (fakeParameter.Value == null || fakeParameter.Value == DBNull.Value)
                {
                    dbParameter.Value = DBNull.Value;
                    //todo:有问题
                    parameterType = fakeParameter.DataType;
                }
                else
                {
                    dbParameter.Value = fakeParameter.Value;
                    //todo:有问题
                    parameterType = fakeParameter.Value.GetType();
                }

                if (fakeParameter.Precision.HasValue) { dbParameter.Precision = fakeParameter.Precision.Value; }
                if (fakeParameter.Scale.HasValue) { dbParameter.Scale = fakeParameter.Scale.Value; }
                if (fakeParameter.Size.HasValue) { dbParameter.Size = fakeParameter.Size.Value; }
                if (fakeParameter.DbType.HasValue)
                {
                    dbParameter.DbType = fakeParameter.DbType.Value;
                }
                else
                {
                    DbType? dbType = SupportedMappingTypes.GetDbType(parameterType);
                    if (dbType.HasValue) { dbParameter.DbType = dbType.Value; }
                }

                OutputParameter outputParameter = null;
                const int defaultSizeOfStringOutputParameter = 2000;
                dbParameter.Direction = fakeParameter.ParameterDirection;
                if (fakeParameter.ParameterDirection == ParameterDirection.Output)
                {
                    //纯作为输出参数时,先将伪装参数的值清除
                    fakeParameter.Value = null;
                    //字符串参数必需设置Size属性,如果没有则使用默认值,如果默认值不够,需要外部另外显示指定
                    if (!fakeParameter.Size.HasValue && fakeParameter.DataType == typeof(String))
                    {
                        dbParameter.Size = defaultSizeOfStringOutputParameter;
                    }
                    outputParameter = new OutputParameter(fakeParameter, dbParameter);
                }
                if (fakeParameter.ParameterDirection == ParameterDirection.InputOutput)
                {
                    if (!fakeParameter.Size.HasValue && fakeParameter.DataType == typeof(String))
                    {
                        dbParameter.Size = defaultSizeOfStringOutputParameter;
                    }
                    outputParameter = new OutputParameter(fakeParameter, dbParameter);
                }
                command.Parameters.Add(dbParameter);
                if (outputParameter != null)
                {
                    if (outputParameters == null) { outputParameters = new List<OutputParameter>(); }
                    outputParameters.Add(outputParameter);
                }
            }
            return command;
        }

        /// <summary>
        /// 激活会话(打开连接)
        /// </summary>
        private void Activate()
        {
            this.CheckDisposed();
            if (this._dbConnection.State == ConnectionState.Broken)
            {
                this._dbConnection.Close();
            }
            if (this._dbConnection.State == ConnectionState.Closed)
            {
                this._dbConnection.Open();
            }
        }

        /// <summary>
        /// 表示会话中一次DB操作完成(非事务中关闭连接)
        /// </summary>
        public void Complete()
        {
            //if (!this.IsInTransaction)
            //{
            //    if (this._dbConnection.State == ConnectionState.Open)
            //    {
            //        this._dbConnection.Close();
            //    }
            //}
        }

        #region DbInterception
        private void OnReaderExecuting(IDbCommand cmd, DbCommandInterceptionContext<IDataReader> dbCommandInterceptionContext, IDbCommandInterceptor[] globalInterceptors)
        {
            this.ExecuteDbCommandInterceptors((dbCommandInterceptor) =>
            {
                dbCommandInterceptor.ReaderExecuting(cmd, dbCommandInterceptionContext);
            }, globalInterceptors);
        }
        private void OnReaderExecuted(IDbCommand cmd, DbCommandInterceptionContext<IDataReader> dbCommandInterceptionContext, IDbCommandInterceptor[] globalInterceptors)
        {
            this.ExecuteDbCommandInterceptors((dbCommandInterceptor) =>
            {
                dbCommandInterceptor.ReaderExecuted(cmd, dbCommandInterceptionContext);
            }, globalInterceptors);
        }
        private void OnNonQueryExecuting(IDbCommand cmd, DbCommandInterceptionContext<int> dbCommandInterceptionContext, IDbCommandInterceptor[] globalInterceptors)
        {
            this.ExecuteDbCommandInterceptors((dbCommandInterceptor) =>
            {
                dbCommandInterceptor.NonQueryExecuting(cmd, dbCommandInterceptionContext);
            }, globalInterceptors);
        }
        private void OnNonQueryExecuted(IDbCommand cmd, DbCommandInterceptionContext<int> dbCommandInterceptionContext, IDbCommandInterceptor[] globalInterceptors)
        {
            this.ExecuteDbCommandInterceptors((dbCommandInterceptor) =>
            {
                dbCommandInterceptor.NonQueryExecuted(cmd, dbCommandInterceptionContext);
            }, globalInterceptors);
        }
        private void OnScalarExecuting(IDbCommand cmd, DbCommandInterceptionContext<object> dbCommandInterceptionContext, IDbCommandInterceptor[] globalInterceptors)
        {
            this.ExecuteDbCommandInterceptors((dbCommandInterceptor) =>
            {
                dbCommandInterceptor.ScalarExecuting(cmd, dbCommandInterceptionContext);
            }, globalInterceptors);
        }
        private void OnScalarExecuted(IDbCommand cmd, DbCommandInterceptionContext<object> dbCommandInterceptionContext, IDbCommandInterceptor[] globalInterceptors)
        {
            this.ExecuteDbCommandInterceptors((dbCommandInterceptor) =>
            {
                dbCommandInterceptor.ScalarExecuted(cmd, dbCommandInterceptionContext);
            }, globalInterceptors);
        }

        private void ExecuteDbCommandInterceptors(Action<IDbCommandInterceptor> act, IDbCommandInterceptor[] globalInterceptors)
        {
            for (int i = 0; i < globalInterceptors.Length; i++)
            {
                act(globalInterceptors[i]);
            }
            if (this.DbCommandInterceptors != null)
            {
                for (int i = 0; i < this.DbCommandInterceptors.Count; i++)
                {
                    act(this.DbCommandInterceptors[i]);
                }
            }
        }
        #endregion

        public void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(string.Format("无法访问已释放的对象:{0}", this.GetType().FullName));
            }
        }

        public static Exception WrapException(Exception ex, string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                return new DataAccessException(ex);
            }
            else
            {
                return new DataAccessException(message, ex);
            }
        }
    }
}
