using FlyingFive.Data.Interception;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Kernel
{
    /// <summary>
    /// 与驱动无关的Ado.NET DB通用处理会话
    /// </summary>
    public class CommonAdoSession : IDisposable
    {
        private bool _disposed = false;
        private IDbConnection _dbConnection = null;
        private IDbTransaction _dbTransaction = null;
        /// <summary>
        /// 当前会话是否处理事务处理中
        /// </summary>
        public bool IsInTransaction { get; private set; }
        /// <summary>
        /// 当前会话的命令处理超时时间（单位：秒）
        /// </summary>
        public int CommandTimeOut { get; set; }
        /// <summary>
        /// 该会话启用的拦截器列表
        /// </summary>
        public IList<IDbCommandInterceptor> DbCommandInterceptors { get; private set; }

        public CommonAdoSession(IDbConnection connection)
        {
            this.CommandTimeOut = 30;
            this._dbConnection = connection;
            this.DbCommandInterceptors = new List<IDbCommandInterceptor>();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
