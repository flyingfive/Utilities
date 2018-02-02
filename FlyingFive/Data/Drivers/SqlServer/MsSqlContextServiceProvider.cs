using FlyingFive.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    /// <summary>
    /// MsSql数据库上下文服务供应器
    /// </summary>
    public class MsSqlContextServiceProvider : IDbContextServiceProvider
    {
        private IDbConnectionFactory _dbConnectionFactory = null;
        private MsSqlContext _msSqlContext = null;

        public MsSqlContextServiceProvider(IDbConnectionFactory dbConnectionFactory, MsSqlContext msSqlContext)
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

        public IDbExpressionTranslator CreateDbExpressionTranslator()
        {
            if (this._msSqlContext.PagingMode == PagingMode.ROW_NUMBER)
            {
                return MsSqlExpressionTranslator.Instance;
            }
            else if (this._msSqlContext.PagingMode == PagingMode.OFFSET_FETCH)
            {
                return MsSqlExpressionTranslator_OffsetFetch.Instance;
            }
            return null;

        }
    }
}
