using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// DB参数表现
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Value = {Value}")]
    public class DbParameterExpression : DbExpression
    {

        public DbParameterExpression(object value)
            : base(DbExpressionType.Parameter)
        {
            this.Value = value;
            this.Type = value != null ? value.GetType() : typeof(Object);
        }

        public DbParameterExpression(object value, Type type)
            : base(DbExpressionType.Parameter)
        {
            if (type == null) { throw new ArgumentNullException("参数: type不能为NULL"); }
            if (value != null)
            {
                Type t = value.GetType();
                if (!type.IsAssignableFrom(t)) { throw new ArgumentException(string.Format("参数值的实际类型{0}与指定类型{1}不匹配", t.FullName, type.FullName)); }
            }
            this.Value = value;
            this.Type = type;
        }

        /// <summary>
        /// 参数类型
        /// </summary>
        public override Type Type { get; protected set; }
        /// <summary>
        /// 参数值
        /// </summary>
        public object Value { get; private set; }
        /// <summary>
        /// 参数的数据库类型
        /// </summary>
        public DbType? DbType { get; set; }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
