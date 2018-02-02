using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// DB中的常量描述
    /// </summary>
    public class DbConstantExpression : DbExpression
    {
        /// <summary>
        /// 常量数据类型
        /// </summary>
        public override Type Type { get; protected set; }
        /// <summary>
        /// 常量值
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// 表示DB中的NULL常量
        /// </summary>
        public static readonly DbConstantExpression Null = new DbConstantExpression(null);
        /// <summary>
        /// 表示DB中的空字符串常量
        /// </summary>
        public static readonly DbConstantExpression StringEmpty = new DbConstantExpression(string.Empty);
        /// <summary>
        /// 表示DB中的整形值1常量
        /// </summary>
        public static readonly DbConstantExpression One = new DbConstantExpression(1);
        /// <summary>
        /// 表示DB中的整形值0常量
        /// </summary>
        public static readonly DbConstantExpression Zero = new DbConstantExpression(0);
        /// <summary>
        /// 表示DB中的布尔值True常量
        /// </summary>
        public static readonly DbConstantExpression True = new DbConstantExpression(true);
        /// <summary>
        /// 表示DB中的布尔值False常量
        /// </summary>
        public static readonly DbConstantExpression False = new DbConstantExpression(false);

        public DbConstantExpression(object value)
            : base(DbExpressionType.Constant)
        {
            this.Value = value;

            if (value != null)
                this.Type = value.GetType();
            else
                this.Type = UtilConstants.TypeOfObject;
        }

        public DbConstantExpression(object value, Type type)
            : base(DbExpressionType.Constant)
        {
            if (type == null) { throw new ArgumentNullException("参数: type不能为NULL."); }
            if (value != null)
            {
                Type t = value.GetType();
                if (!type.IsAssignableFrom(t)) { throw new ArgumentException(string.Format("常量值的实际类型{0}与指定类型{1}不匹配", t.FullName, type.FullName)); }
            }
            this.Value = value;
            this.Type = type;
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
