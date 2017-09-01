using FlyingFive.Data.Infrastructure;
using FlyingFive.Data.Kernel;
using FlyingFive.Data.Mapper;
using FlyingFive.Data.Query.Mapping;
using FlyingFive.Data.Query.QueryState;
using FlyingFive.Data.Query.Visitors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Query.Internals
{
    internal class InternalQuery<T> : IEnumerable<T>, IEnumerable
    {
        Query<T> _query;

        internal InternalQuery(Query<T> query)
        {
            this._query = query;
        }

        DbCommandFactor GenerateCommandFactor()
        {
            IQueryState qs = QueryExpressionVisitor.VisitQueryExpression(this._query.QueryExpression);
            MappingData data = qs.GenerateMappingData();

            IObjectActivator objectActivator;
            if (this._query._trackEntity)
                objectActivator = data.ObjectActivatorCreator.CreateObjectActivator(this._query.DbContext);
            else
                objectActivator = data.ObjectActivatorCreator.CreateObjectActivator();

            IDbExpressionTranslator translator = this._query.DbContext.DbContextServiceProvider.CreateDbExpressionTranslator();
            List<FakeParameter> parameters;
            string cmdText = translator.Translate(data.SqlQuery, out parameters);

            DbCommandFactor commandFactor = new DbCommandFactor(objectActivator, cmdText, parameters.ToArray());
            return commandFactor;
        }

        public IEnumerator<T> GetEnumerator()
        {
            DbCommandFactor commandFactor = this.GenerateCommandFactor();
            var enumerator = QueryEnumeratorCreator.CreateEnumerator<T>(this._query.DbContext.CommonSession, commandFactor);
            return enumerator;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            DbCommandFactor commandFactor = this.GenerateCommandFactor();
            return UtilConstants.AppendDbCommandInfo(commandFactor.CommandText, commandFactor.Parameters);
        }
    }
}
