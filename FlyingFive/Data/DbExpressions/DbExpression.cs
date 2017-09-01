using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 各种数据库的表现形式基类
    /// </summary>
    public abstract class DbExpression
    {
        /// <summary>
        /// 当前节点所操作的数据类型
        /// </summary>
        public virtual Type Type { get; protected set; }
        /// <summary>
        /// 当前节点的DB表现形式类型
        /// </summary>
        public virtual DbExpressionType NodeType { get; private set; }

        protected DbExpression(DbExpressionType nodeType) : this(nodeType, typeof(void)) { }

        protected DbExpression(DbExpressionType nodeType, Type type)
        {
            this.NodeType = nodeType;
            this.Type = type;
        }

        /// <summary>
        /// 接受一个DB操作表现的实例，并转换成对应的SQL编码
        /// </summary>
        /// <typeparam name="T">DB操作的数据类型</typeparam>
        /// <param name="visitor">DB操作表现翻译器</param>
        /// <returns></returns>
        public abstract T Accept<T>(DbExpressionVisitor<T> visitor);


        /// <summary>
        /// 将一个值包装成Db参数表现
        /// </summary>
        /// <param name="value">参数值</param>
        /// <returns></returns>
        public static DbParameterExpression Parameter(object value)
        {
            return new DbParameterExpression(value);
        }

        /// <summary>
        /// 将一个值包装成DB参数表现
        /// </summary>
        /// <param name="value">参数值</param>
        /// <param name="defaultType">参数默认类型</param>
        /// <returns></returns>
        public static DbParameterExpression Parameter(object value, Type defaultType)
        {
            if (value == null)
                return new DbParameterExpression(value, defaultType);
            else
                return new DbParameterExpression(value, value.GetType());
        }

        /// <summary>
        /// 创建一个DB SWITCH条件操作
        /// </summary>
        /// <param name="whenThenExps">全部条件部分表达式</param>
        /// <param name="elseExp">否则部分表达式</param>
        /// <param name="type">判断返回类型</param>
        /// <returns></returns>
        public static DbCaseWhenExpression CaseWhen(IList<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps, DbExpression elseExp, Type type)
        {
            return new DbCaseWhenExpression(type, whenThenExps, elseExp);
        }
        public static DbCaseWhenExpression CaseWhen(DbCaseWhenExpression.WhenThenExpressionPair whenThenExpPair, DbExpression elseExp, Type type)
        {
            List<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(1);
            whenThenExps.Add(whenThenExpPair);
            return DbExpression.CaseWhen(whenThenExps, elseExp, type);
        }
        /// <summary>
        /// 创建一个DB取反操作
        /// </summary>
        /// <param name="exp">要进行取反的操作</param>
        /// <returns></returns>
        public static DbNotExpression Not(DbExpression exp)
        {
            return new DbNotExpression(exp);
        }

        /// <summary>
        /// 创建一个DB转换操作
        /// </summary>
        /// <param name="operand">转换对象</param>
        /// <param name="type">转换目标类型</param>
        /// <returns></returns>
        public static DbConvertExpression Convert(DbExpression operand, Type type)
        {
            return new DbConvertExpression(type, operand);
        }

        public static DbAddExpression Add(DbExpression left, DbExpression right, Type returnType, MethodInfo method)
        {
            return new DbAddExpression(returnType, left, right, method);
        }
        public static DbSubtractExpression Subtract(DbExpression left, DbExpression right, Type returnType)
        {
            return new DbSubtractExpression(returnType, left, right);
        }
        public static DbMultiplyExpression Multiply(DbExpression left, DbExpression right, Type returnType)
        {
            return new DbMultiplyExpression(returnType, left, right);
        }
        public static DbDivideExpression Divide(DbExpression left, DbExpression right, Type returnType)
        {
            return new DbDivideExpression(returnType, left, right);
        }
        public static DbModuloExpression Modulo(DbExpression left, DbExpression right, Type returnType)
        {
            return new DbModuloExpression(returnType, left, right);
        }

        public static DbBitAndExpression BitAnd(Type type, DbExpression left, DbExpression right)
        {
            return new DbBitAndExpression(type, left, right);
        }
        public static DbBitOrExpression BitOr(Type type, DbExpression left, DbExpression right)
        {
            return new DbBitOrExpression(type, left, right);
        }

        public static DbAndExpression And(DbExpression left, DbExpression right)
        {
            return new DbAndExpression(left, right);
        }
        public static DbOrExpression Or(DbExpression left, DbExpression right)
        {
            return new DbOrExpression(left, right);
        }
        public static DbEqualExpression Equal(DbExpression left, DbExpression right)
        {
            return new DbEqualExpression(left, right);
        }
        public static DbNotEqualExpression NotEqual(DbExpression left, DbExpression right)
        {
            return new DbNotEqualExpression(left, right);
        }
        public static DbConstantExpression Constant(object value)
        {
            return new DbConstantExpression(value);
        }

        public static DbConstantExpression Constant(object value, Type type)
        {
            return new DbConstantExpression(value, type);
        }
        public static DbMemberExpression MemberAccess(MemberInfo member, DbExpression exp)
        {
            return new DbMemberExpression(member, exp);
        }
        public static DbMethodCallExpression MethodCall(DbExpression @object, MethodInfo method, IList<DbExpression> arguments)
        {
            return new DbMethodCallExpression(@object, method, arguments);
        }
        public static DbGreaterThanExpression GreaterThan(DbExpression left, DbExpression right)
        {
            return new DbGreaterThanExpression(left, right);
        }
        public static DbGreaterThanOrEqualExpression GreaterThanOrEqual(DbExpression left, DbExpression right)
        {
            return new DbGreaterThanOrEqualExpression(left, right);
        }

        public static DbLessThanExpression LessThan(DbExpression left, DbExpression right)
        {
            return new DbLessThanExpression(left, right);
        }
        public static DbLessThanOrEqualExpression LessThanOrEqual(DbExpression left, DbExpression right)
        {
            return new DbLessThanOrEqualExpression(left, right);
        }
    }
}
