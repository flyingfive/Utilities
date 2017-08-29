using FlyingFive.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    /// <summary>
    /// MsSql数据库上下文
    /// </summary>
    public class MsSqlContext : DbContext
    {
        /// <summary>
        /// 当前上下文的分页查询模式。
        /// </summary>
        public PagingMode PagingMode { get; set; }

        private IDbContextProvider _dbContextProvider = null;

        /// <summary>
        /// 上下文环境供应者(提供上下文所需要的DB连接以及表达式翻译等功能)
        /// </summary>
        public override IDbContextProvider DbContextProvider { get { return _dbContextProvider; } }

        public MsSqlContext(string connectionString)
            : this(new DefaultDbConnectionFactory(connectionString))
        {
        }

        public MsSqlContext(IDbConnectionFactory dbConnectionFactory)
        {
            if (dbConnectionFactory == null) { throw new ArgumentNullException("参数: dbConnectionFactory不能为null"); }
            this.PagingMode = PagingMode.ROW_NUMBER;
            this._dbContextProvider = new MsSqlContextProvider(dbConnectionFactory, this);
        }
    }
}
