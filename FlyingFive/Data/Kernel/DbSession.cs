using FlyingFive.Data.Interception;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Kernel
{
    public class DbSession : IDbSession
    {
        public DbContext DbContext { get; private set; }

        public bool IsInTransaction { get { return this.DbContext.CommonSession.IsInTransaction; } }

        public int CommandTimeout { get { return this.DbContext.CommonSession.CommandTimeout; } set { this.DbContext.CommonSession.CommandTimeout = value; } }

        public DbSession(DbContext dbContext)
        {
            UtilExceptions.CheckNull(dbContext);
            this.DbContext = dbContext;
        }

        public int ExecuteNonQuery(string cmdText, CommandType cmdType, params FakeParameter[] parameters)
        {
            return this.DbContext.CommonSession.ExecuteNonQuery(cmdText, CommandType.Text, parameters);
        }

        public object ExecuteScalar(string cmdText, CommandType cmdType, params FakeParameter[] parameters)
        {
            return this.DbContext.CommonSession.ExecuteScalar(cmdText, cmdType, parameters);
        }

        public IDataReader ExecuteReader(string cmdText, CommandType cmdType, params FakeParameter[] parameters)
        {
            return this.DbContext.CommonSession.ExecuteReader(cmdText, cmdType, parameters);
        }

        public void BeginTransaction()
        {
            this.DbContext.CommonSession.BeginTransaction(null);
        }

        public void BeginTransaction(IsolationLevel il)
        {
            this.DbContext.CommonSession.BeginTransaction(il);
        }

        public void CommitTransaction()
        {
            this.DbContext.CommonSession.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            this.DbContext.CommonSession.RollbackTransaction();
        }

        public void AddInterceptor(IDbCommandInterceptor interceptor)
        {
            UtilExceptions.CheckNull(interceptor, "interceptor");
            this.DbContext.CommonSession.DbCommandInterceptors.Add(interceptor);
        }

        public void RemoveInterceptor(IDbCommandInterceptor interceptor)
        {
            UtilExceptions.CheckNull(interceptor, "interceptor");
            this.DbContext.CommonSession.DbCommandInterceptors.Remove(interceptor);
        }

        public void Dispose()
        {
            this.DbContext.Dispose();
        }
    }
}
