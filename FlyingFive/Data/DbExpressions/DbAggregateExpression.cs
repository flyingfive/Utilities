using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 表示DB聚合操作
    /// </summary>
    public class DbAggregateExpression : DbExpression
    {
        /// <summary>
        /// 聚合方法
        /// </summary>
        public MethodInfo Method { get; private set; }
        /// <summary>
        /// 调用方法的参数
        /// </summary>
        public ReadOnlyCollection<DbExpression> Arguments { get; private set; }
        public DbAggregateExpression(Type type, MethodInfo method, IList<DbExpression> arguments)
            : base(DbExpressionType.Aggregate, type)
        {
            this.Method = method;
            this.Arguments = new ReadOnlyCollection<DbExpression>(arguments);
        }
        
        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
