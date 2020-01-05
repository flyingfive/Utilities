using FlyingFive.Data.Infrastructure;
using FlyingFive.Data.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    public class MsSqlHelper : DbSession, IDbSession
    {
        public MsSqlHelper(string connectionString)
            : this(new SqlServerDbConnectionFactory(connectionString))
        {
        }

        public MsSqlHelper(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
        {
            //this.PagingMode = PagingMode.ROW_NUMBER;
            //this._dbContextProvider = new MsSqlContextServiceProvider(dbConnectionFactory, this);
        }
    }
}
