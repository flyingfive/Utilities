using FlyingFive.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.SqlServer
{
    public class MsSqlContextProvider : IDbContextProvider
    {
        private IDbConnectionFactory _dbConnectionFactory = null;
        private MsSqlContext _msSqlContext = null;

        public MsSqlContextProvider(IDbConnectionFactory dbConnectionFactory, MsSqlContext msSqlContext)
        {
            if (msSqlContext == null) { throw new ArgumentNullException("参数: msSqlContext不能为null"); }
            if (dbConnectionFactory == null) { throw new ArgumentNullException("参数: dbConnectionFactory不能为null"); }
            this._dbConnectionFactory = dbConnectionFactory;
            this._msSqlContext = msSqlContext;
        }

        public IDbConnection CreateConnection()
        {
            return _dbConnectionFactory.CreateConnection();
        }
    }
}
