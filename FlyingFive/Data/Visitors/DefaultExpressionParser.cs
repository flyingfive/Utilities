using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Descriptors;
using FlyingFive.Data.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlyingFive.Data.Visitors
{
    public class DefaultExpressionParser : ExpressionVisitorBase
    {
        EntityTypeDescriptor _typeDescriptor;
        DbTable _explicitDbTable;

        public DefaultExpressionParser(EntityTypeDescriptor typeDescriptor, DbTable explicitDbTable)
        {
            this._typeDescriptor = typeDescriptor;
            this._explicitDbTable = explicitDbTable;
        }

        public DbExpression ParseFilterPredicate(LambdaExpression lambda)
        {
            lambda = ExpressionVisitorBase.ReBuildFilterPredicate(lambda);
            return this.Visit(lambda);
        }
        public DbExpression Parse(Expression exp)
        {
            return this.Visit(exp);
        }

        protected override DbExpression VisitMemberAccess(MemberExpression exp)
        {
            if (exp.IsDerivedFromParameter())
            {
                Stack<MemberExpression> reversedExps = exp.Reverse();

                DbExpression dbExp = null;
                bool first = true;
                foreach (var me in reversedExps)
                {
                    if (first)
                    {
                        DbColumnAccessExpression dbColumnAccessExpression = this._typeDescriptor.TryGetColumnAccessExpression(me.Member);
                        if (this._explicitDbTable != null)
                            dbColumnAccessExpression = new DbColumnAccessExpression(this._explicitDbTable, dbColumnAccessExpression.Column);

                        if (dbColumnAccessExpression == null)
                        {
                            throw new DataAccessException(string.Format("The member '{0}' does not map any column.", me.Member.Name));
                        }

                        dbExp = dbColumnAccessExpression;
                        first = false;
                    }
                    else
                    {
                        DbMemberExpression dbMe = new DbMemberExpression(me.Member, dbExp);
                        dbExp = dbMe;
                    }
                }

                if (dbExp != null)
                {
                    return dbExp;
                }
                else
                    throw new Exception();
            }
            else
            {
                return base.VisitMemberAccess(exp);
            }
        }

        protected override DbExpression VisitParameter(ParameterExpression exp)
        {
            throw new NotSupportedException(exp.ToString());
        }
    }
}
