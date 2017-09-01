using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Infrastructure;
using FlyingFive.Data.Query.Mapping;
using FlyingFive.Data.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Query.Visitors
{
    internal class SelectorExpressionVisitor : ExpressionVisitor<IMappingObjectExpression>
    {
        private ExpressionVisitorBase _visitor;
        private LambdaExpression _lambda;
        private List<IMappingObjectExpression> _moeList;
        SelectorExpressionVisitor(List<IMappingObjectExpression> moeList)
        {
            this._moeList = moeList;
        }

        public static IMappingObjectExpression ResolveSelectorExpression(LambdaExpression selector, List<IMappingObjectExpression> moeList)
        {
            SelectorExpressionVisitor visitor = new SelectorExpressionVisitor(moeList);
            return visitor.Visit(selector);
        }

        private int FindParameterIndex(ParameterExpression exp)
        {
            int idx = this._lambda.Parameters.IndexOf(exp);
            if (idx == -1)
            {
                throw new Exception("Can not find the ParameterExpression index");
            }

            return idx;
        }
        private DbExpression ResolveExpression(Expression exp)
        {
            return this._visitor.Visit(exp);
        }
        private IMappingObjectExpression ResolveComplexMember(MemberExpression exp)
        {
            ParameterExpression p;
            if (exp.IsDerivedFromParameter(out p))
            {
                int idx = this.FindParameterIndex(p);
                IMappingObjectExpression moe = this._moeList[idx];
                return moe.GetComplexMemberExpression(exp);
            }
            else
            {
                throw new Exception();
            }
        }

        public override IMappingObjectExpression Visit(Expression exp)
        {
            if (exp == null)
                return default(IMappingObjectExpression);
            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return this.VisitNew((NewExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)exp);
                default:
                    return this.VisistMapTypeSelector(exp);
            }
        }

        protected override IMappingObjectExpression VisitLambda(LambdaExpression exp)
        {
            this._lambda = exp;
            this._visitor = new GeneralExpressionVisitor(exp, this._moeList);
            return this.Visit(exp.Body);
        }
        protected override IMappingObjectExpression VisitNew(NewExpression exp)
        {
            IMappingObjectExpression result = new MappingObjectExpression(exp.Constructor);
            ParameterInfo[] parames = exp.Constructor.GetParameters();
            for (int i = 0; i < parames.Length; i++)
            {
                ParameterInfo pi = parames[i];
                Expression argExp = exp.Arguments[i];
                if (SupportedMappingTypes.IsMappingType(pi.ParameterType))
                {
                    DbExpression dbExpression = this.ResolveExpression(argExp);
                    result.AddMappingConstructorParameter(pi, dbExpression);
                }
                else
                {
                    IMappingObjectExpression subResult = this.Visit(argExp);
                    result.AddComplexConstructorParameter(pi, subResult);
                }
            }

            return result;
        }
        protected override IMappingObjectExpression VisitMemberInit(MemberInitExpression exp)
        {
            IMappingObjectExpression result = this.Visit(exp.NewExpression);

            foreach (MemberBinding binding in exp.Bindings)
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                {
                    throw new NotSupportedException();
                }

                MemberAssignment memberAssignment = (MemberAssignment)binding;
                MemberInfo member = memberAssignment.Member;
                Type memberType = member.GetMemberType();

                //是数据库映射类型
                if (SupportedMappingTypes.IsMappingType(memberType))
                {
                    DbExpression dbExpression = this.ResolveExpression(memberAssignment.Expression);
                    result.AddMappingMemberExpression(member, dbExpression);
                }
                else
                {
                    //对于非数据库映射类型，只支持 NewExpression 和 MemberInitExpression
                    IMappingObjectExpression subResult = this.Visit(memberAssignment.Expression);
                    result.AddComplexMemberExpression(member, subResult);
                }
            }

            return result;
        }
        /// <summary>
        /// a => a.Id   a => a.Name   a => a.User
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        protected override IMappingObjectExpression VisitMemberAccess(MemberExpression exp)
        {
            //create MappingFieldExpression object if exp is map type
            if (SupportedMappingTypes.IsMappingType(exp.Type))
            {
                DbExpression dbExp = this.ResolveExpression(exp);
                MappingFieldExpression ret = new MappingFieldExpression(exp.Type, dbExp);
                return ret;
            }

            //如 a.Order a.User 等形式
            return this.ResolveComplexMember(exp);
        }
        protected override IMappingObjectExpression VisitParameter(ParameterExpression exp)
        {
            int idx = this.FindParameterIndex(exp);
            IMappingObjectExpression moe = this._moeList[idx];
            return moe;
        }

        private IMappingObjectExpression VisistMapTypeSelector(Expression exp)
        {
            if (!SupportedMappingTypes.IsMappingType(exp.Type))
            {
                throw new NotSupportedException(exp.ToString());
            }

            DbExpression dbExp = this.ResolveExpression(exp);
            MappingFieldExpression ret = new MappingFieldExpression(exp.Type, dbExp);
            return ret;
        }
    }
}
