using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Query.Mapping
{
    public class MappingFieldExpression : IMappingObjectExpression
    {
        private Type _type;
        public DbExpression Expression { get; private set; }
        public DbExpression NullChecking { get; set; }
        public MappingFieldExpression(Type type, DbExpression exp)
        {
            this._type = type;
            this.Expression = exp;
        }



        public void AddMappingConstructorParameter(ParameterInfo p, DbExpression exp)
        {
            throw new NotSupportedException();
        }
        public void AddComplexConstructorParameter(ParameterInfo p, IMappingObjectExpression exp)
        {
            throw new NotSupportedException();
        }
        public void AddMappingMemberExpression(MemberInfo p, DbExpression exp)
        {
            throw new NotSupportedException();
        }
        public void AddComplexMemberExpression(MemberInfo p, IMappingObjectExpression exp)
        {
            throw new NotSupportedException();
        }
        public DbExpression GetMappingMemberExpression(MemberInfo memberInfo)
        {
            throw new NotSupportedException();
        }
        public IMappingObjectExpression GetComplexMemberExpression(MemberInfo memberInfo)
        {
            throw new NotSupportedException();
        }
        public DbExpression GetDbExpression(MemberExpression memberExpressionDeriveParameter)
        {
            Stack<MemberExpression> memberExpressions = memberExpressionDeriveParameter.Reverse();

            if (memberExpressions.Count == 0) { throw new InvalidOperationException("没有找到访问成员."); }

            DbExpression ret = this.Expression;

            foreach (MemberExpression memberExpression in memberExpressions)
            {
                MemberInfo member = memberExpression.Member;
                ret = DbExpression.MemberAccess(member, ret);
            }

            if (ret == null)
                throw new InvalidOperationException(memberExpressionDeriveParameter.ToString());

            return ret;
        }
        public IMappingObjectExpression GetComplexMemberExpression(MemberExpression exp)
        {
            throw new NotSupportedException();
        }

        public IObjectActivatorCreator GenarateObjectActivatorCreator(DbSqlQueryExpression sqlQuery)
        {
            int ordinal;
            ordinal = MappingObjectExpressionHelper.TryGetOrAddColumn(sqlQuery, this.Expression).Value;

            MappingField mf = new MappingField(this._type, ordinal);

            mf.CheckNullOrdinal = MappingObjectExpressionHelper.TryGetOrAddColumn(sqlQuery, this.NullChecking);

            return mf;
        }


        public IMappingObjectExpression ToNewObjectExpression(DbSqlQueryExpression sqlQuery, DbTable table)
        {
            DbColumnAccessExpression cae = null;
            cae = MappingObjectExpressionHelper.ParseColumnAccessExpression(sqlQuery, table, this.Expression);

            MappingFieldExpression mf = new MappingFieldExpression(this._type, cae);

            mf.NullChecking = MappingObjectExpressionHelper.TryGetOrAddNullChecking(sqlQuery, table, this.NullChecking);

            return mf;
        }

        public void SetNullChecking(DbExpression exp)
        {
            if (this.NullChecking == null)
                this.NullChecking = exp;
        }

    }
}
