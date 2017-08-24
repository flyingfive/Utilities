using FlyingFive.Data.Infrastructure;
using FlyingFive.Data.Kernel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data
{
    /// <summary>
    /// 表示DB上下文
    /// </summary>
    public abstract class DbContext : IDbContext
    {
        private bool _disposed = false;
        public IDbSession Session { get; private set; }
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
                    this._commonSession = new CommonAdoSession(this.DbContextProvider.CreateConnection());
                return this._commonSession;
            }
        }

        /// <summary>
        /// 上下文环境供应者(提供上下文所需要的DB连接以及表达式翻译等功能)
        /// </summary>
        public abstract IDbContextProvider DbContextProvider { get;  }

        protected DbContext()
        {
            this.Session = new DbSession(this);
        }

        public TEntity QueryByKey<TEntity>(object key, bool tracking = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> SqlQuery<T>(string plainSql, CommandType cmdType = CommandType.Text, params FakeParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public TEntity Insert<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public int Update<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> body)
        {
            throw new NotImplementedException();
        }

        public int Delete<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public int Delete<TEntity>(Expression<Func<TEntity, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public int DeleteByKey<TEntity>(object key)
        {
            throw new NotImplementedException();
        }

        public void TrackEntity(object entity)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (this._disposed)
                return;

            if (this._commonSession != null)
                this._commonSession.Dispose();
            this.Dispose(true);
            this._disposed = true;
        }

        protected virtual void Dispose(bool disposing)
        {

        }

        protected void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(string.Format("无法访问已释放的对象:{0}", this.GetType().FullName));
            }
        }
    }
}
