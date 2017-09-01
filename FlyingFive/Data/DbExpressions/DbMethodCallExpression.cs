using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 一个DB方法调用操作
    /// </summary>
    public class DbMethodCallExpression : DbExpression
    {
        /// <summary>
        /// 方法调用时传递的参数
        /// </summary>
        public ReadOnlyCollection<DbExpression> Arguments { get; private set; }
        /// <summary>
        /// 调用的方法
        /// </summary>
        public MethodInfo Method { get; private set; }
        /// <summary>
        /// 调用方法的对象
        /// </summary>
        public DbExpression Object { get; private set; }
        /// <summary>
        /// 方法的返回类型
        /// </summary>
        public override Type Type { get { return this.Method.ReturnType; } }

        public DbMethodCallExpression(DbExpression callObj, MethodInfo method, IList<DbExpression> arguments)
            : base(DbExpressionType.Call)
        {
            this.Object = callObj;
            this.Method = method;
            this.Arguments = new ReadOnlyCollection<DbExpression>(arguments);
        }
        
        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
