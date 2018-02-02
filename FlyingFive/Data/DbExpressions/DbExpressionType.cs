using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// DB表现形式类型
    /// </summary>
    public enum DbExpressionType : int
    {
        #region 逻辑比较操作
        /// <summary>
        /// DB等于操作
        /// </summary>
        Equal = 1,
        /// <summary>
        /// DB不等于操作
        /// </summary>
        NotEqual = 2,
        /// <summary>
        /// DB并且操作
        /// </summary>
        And = 3,
        /// <summary>
        /// DB或者操作
        /// </summary>
        Or = 4,
        /// <summary>
        /// DB取反操作
        /// </summary>
        Not = 5,
        /// <summary>
        /// DB小于操作
        /// </summary>
        LessThan = 6,
        /// <summary>
        /// DB小于等于操作
        /// </summary>
        LessThanOrEqual = 7,
        /// <summary>
        /// DB大于操作
        /// </summary>
        GreaterThan = 8,
        /// <summary>
        /// DB大于等于操作
        /// </summary>
        GreaterThanOrEqual = 9,

        #endregion

        #region 逻辑运算操作

        /// <summary>
        /// DB逻辑加法操作
        /// </summary>
        Add = 10,
        /// <summary>
        /// DB逻辑减法操作
        /// </summary>
        Subtract = 11,
        /// <summary>
        /// DB逻辑乘法操作
        /// </summary>
        Multiply = 12,
        /// <summary>
        /// DB逻辑除法操作
        /// </summary>
        Divide = 13,
        /// <summary>
        /// DB逻辑按位且操作
        /// </summary>
        BitAnd = 14,
        /// <summary>
        /// DB逻辑按位或操作
        /// </summary>
        BitOr = 15,
        /// <summary>
        /// DB逻辑取余操作
        /// </summary>
        Modulo = 16,
        #endregion

        /// <summary>
        /// DB Insert操作
        /// </summary>
        Insert = 17,
        /// <summary>
        /// DB Update操作
        /// </summary>
        Update = 18,
        /// <summary>
        /// DB Delete操作
        /// </summary>
        Delete = 19,
        /// <summary>
        /// DB SQL查询操作
        /// </summary>
        SqlQuery = 20,
        /// <summary>
        /// DB 子查询操作
        /// </summary>
        SubQuery = 21,
        /// <summary>
        /// DB转换操作
        /// </summary>
        Convert = 22,
        /// <summary>
        /// DB常量操作
        /// </summary>
        Constant = 23,
        /// <summary>
        /// DB的NULL转换操作
        /// </summary>
        NullConvert = 24,
        /// <summary>
        /// DB条件判断操作
        /// </summary>
        CaseWhen = 25,
        /// <summary>
        /// DB成员访问操作
        /// </summary>
        MemberAccess = 26,
        /// <summary>
        /// DB方法调用操作
        /// </summary>
        Call = 27,
        /// <summary>
        /// DB表操作
        /// </summary>
        Table = 28,
        /// <summary>
        /// DB列访问
        /// </summary>
        ColumnAccess = 29,
        /// <summary>
        /// DB参数表现
        /// </summary>
        Parameter = 30,
        /// <summary>
        /// DB主表查询操作
        /// </summary>
        FromTable = 31,
        /// <summary>
        /// DB表连接查询操作
        /// </summary>
        JoinTable = 32,
        /// <summary>
        /// DB聚合操作
        /// </summary>
        Aggregate = 33,

    }
}
