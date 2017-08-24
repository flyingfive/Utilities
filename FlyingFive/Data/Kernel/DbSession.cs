using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Kernel
{
    public class DbSession : IDbSession
    {
        public IDbContext DbContext { get; private set; }

        public bool IsInTransaction { get; private set; }

        public int CommandTimeout { get; set; }

        public DbSession(DbContext dbContext)
        {
            this.DbContext = dbContext;
        }

        public int ExecuteNonQuery(string cmdText, CommandType cmdType, params FakeParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar(string cmdText, CommandType cmdType, params FakeParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public IDataReader ExecuteReader(string cmdText, CommandType cmdType, params FakeParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public void BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public void BeginTransaction(IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        public void CommitTransaction()
        {
            throw new NotImplementedException();
        }

        public void RollbackTransaction()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
