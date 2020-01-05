using FlyingFive.Data.Fakes;
using FlyingFive.Data.Infrastructure;
using FlyingFive.Data.Interception;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Kernel
{
    public abstract class DbSession : IDbSession, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public IDbConnectionFactory DbConnectionFactory { get; private set; }

        private CommonAdoSession _commonSession = null;
        /// <summary>
        /// 通用会话
        /// </summary>
        public CommonAdoSession CommonSession
        {
            get
            {
                this.CheckDisposed();
                if (this._commonSession == null)
                    this._commonSession = new CommonAdoSession(this.DbConnectionFactory.CreateConnection());
                return this._commonSession;
            }
        }

        public bool IsInTransaction { get { return this.CommonSession.IsInTransaction; } }

        public int CommandTimeout { get { return this.CommonSession.CommandTimeout; } set { this.CommonSession.CommandTimeout = value; } }

        public DbSession(IDbConnectionFactory connectionFactory)
        {
            UtilExceptions.CheckNull(connectionFactory);
            this.DbConnectionFactory = connectionFactory;
        }

        public int ExecuteNonQuery(string cmdText, CommandType cmdType = CommandType.Text, params FakeParameter[] parameters)
        {
            return this.CommonSession.ExecuteNonQuery(cmdText, CommandType.Text, parameters);
        }

        public object ExecuteScalar(string cmdText, CommandType cmdType = CommandType.Text, params FakeParameter[] parameters)
        {
            return this.CommonSession.ExecuteScalar(cmdText, cmdType, parameters);
        }

        public IDataReader ExecuteReader(string cmdText, CommandType cmdType = CommandType.Text, params FakeParameter[] parameters)
        {
            return this.CommonSession.ExecuteReader(cmdText, cmdType, parameters);
        }

        public void BeginTransaction()
        {
            this.CommonSession.BeginTransaction(null);
        }

        public void BeginTransaction(IsolationLevel il)
        {
            this.CommonSession.BeginTransaction(il);
        }

        public void CommitTransaction()
        {
            this.CommonSession.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            this.CommonSession.RollbackTransaction();
        }

        public void AddInterceptor(IDbCommandInterceptor interceptor)
        {
            UtilExceptions.CheckNull(interceptor, "interceptor");
            this.CommonSession.DbCommandInterceptors.Add(interceptor);
        }

        public void RemoveInterceptor(IDbCommandInterceptor interceptor)
        {
            UtilExceptions.CheckNull(interceptor, "interceptor");
            this.CommonSession.DbCommandInterceptors.Remove(interceptor);
        }

        private bool _disposed = false;
        protected void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(string.Format("无法访问已释放的对象:{0}", this.GetType().FullName));
            }
        }

        public void Dispose()
        {
            if (this._disposed) { return; }
            this.CommonSession.Dispose();
            this._disposed = true;
        }
    }
}
