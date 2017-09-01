using FlyingFive.Data.Infrastructure;
using FlyingFive.Data.Kernel;
using FlyingFive.Data.Mapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Query.Internals
{
    static class QueryEnumeratorCreator
    {
        public static IEnumerator<T> CreateEnumerator<T>(CommonAdoSession adoSession, DbCommandFactor commandFactor)
        {
            return new QueryEnumerator<T>(adoSession, commandFactor);
        }
        internal struct QueryEnumerator<T> : IEnumerator<T>
        {
            CommonAdoSession _adoSession;
            DbCommandFactor _commandFactor;
            IObjectActivator _objectActivator;

            IDataReader _reader;

            T _current;
            bool _hasFinished;
            bool _disposed;
            public QueryEnumerator(CommonAdoSession adoSession, DbCommandFactor commandFactor)
            {
                this._adoSession = adoSession;
                this._commandFactor = commandFactor;
                this._objectActivator = commandFactor.ObjectActivator;

                this._reader = null;

                this._current = default(T);
                this._hasFinished = false;
                this._disposed = false;
            }

            public T Current { get { return this._current; } }

            object IEnumerator.Current { get { return this._current; } }

            public bool MoveNext()
            {
                if (this._hasFinished || this._disposed)
                    return false;

                if (this._reader == null)
                {
                    //TODO 执行 sql 语句，获取 DataReader
                    this._reader = this._adoSession.ExecuteReader(this._commandFactor.CommandText, CommandType.Text, this._commandFactor.Parameters);
                }

                if (this._reader.Read())
                {
                    this._current = (T)this._objectActivator.CreateInstance(this._reader);
                    return true;
                }
                else
                {
                    this._reader.Close();
                    this._current = default(T);
                    this._hasFinished = true;
                    return false;
                }
            }

            public void Dispose()
            {
                if (this._disposed)
                    return;

                if (this._reader != null)
                {
                    if (!this._reader.IsClosed)
                        this._reader.Close();
                    this._reader.Dispose();
                    this._reader = null;
                }

                if (!this._hasFinished)
                {
                    this._hasFinished = true;
                }

                this._current = default(T);
                this._disposed = true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }
    }
}
