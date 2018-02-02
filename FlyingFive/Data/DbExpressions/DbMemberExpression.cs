using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// Db成员表现
    /// </summary>
    public class DbMemberExpression : DbExpression
    {

        public override Type Type
        {
            get
            {
                return this.Member.GetMemberType();
            }
        }

        public MemberInfo Member { get; private set; }

        public DbType? DbType { get; set; }

        /// <summary>
        /// 字段或属性的包含对象
        /// </summary>
        public DbExpression Expression { get; private set; }

        public DbMemberExpression(MemberInfo member, DbExpression exp)
            : base(DbExpressionType.MemberAccess)
        {
            if (member.MemberType != MemberTypes.Property && member.MemberType != MemberTypes.Field)
            {
                throw new ArgumentException("此表达式不能用于描述非实体属性或字段外的其它表现");
            }
            this.Member = member;
            this.Expression = exp;
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
