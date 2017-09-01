using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// DB操作表现形式的访问器
    /// 对各种DB表现形式的操作进行翻译,形成对应的SQL编码
    /// </summary>
    /// <typeparam name="T">DB操作的数据类型</typeparam>
    public abstract class DbExpressionVisitor<T>
    {
        /// <summary>
        /// 翻译DB列访问操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一个列访问操作</param>
        /// <returns></returns>
        public abstract T Visit(DbColumnAccessExpression exp);
        /// <summary>
        /// 翻译DB参数化操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一个DB参数</param>
        /// <returns></returns>
        public abstract T Visit(DbParameterExpression exp);
        /// <summary>
        /// 翻译DB转换操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一个DB转换动作</param>
        /// <returns></returns>
        public abstract T Visit(DbConvertExpression exp);
        /// <summary>
        /// 翻译DB成员访问操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一个DB成员访问动作</param>
        /// <returns></returns>
        public abstract T Visit(DbMemberExpression exp);
        /// <summary>
        /// 翻译DB条件判断操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一个DB条件判断表达式</param>
        /// <returns></returns>
        public abstract T Visit(DbCaseWhenExpression exp);
        /// <summary>
        /// 翻译DB方法调用操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一个DB方法调用操作</param>
        /// <returns></returns>
        public abstract T Visit(DbMethodCallExpression exp);
        /// <summary>
        /// 翻译DB相等比较操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbEqualExpression exp);
        /// <summary>
        /// 翻译DB不等比较操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbNotEqualExpression exp);
        /// <summary>
        /// 翻译DB加法操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbAddExpression exp);
        /// <summary>
        /// 翻译DB减法操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbSubtractExpression exp);
        /// <summary>
        /// 翻译DB乘法操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbMultiplyExpression exp);
        /// <summary>
        /// 翻译DB除法操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbDivideExpression exp);
        /// <summary>
        /// 翻译DB取余操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbModuloExpression exp);
        /// <summary>
        /// 翻译DB小于比较操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbLessThanExpression exp);
        /// <summary>
        /// 翻译DB小于或等于比较操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbLessThanOrEqualExpression exp);
        /// <summary>
        /// 翻译DB大于比较操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbGreaterThanExpression exp);
        /// <summary>
        /// 翻译DB大于或等于比较操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbGreaterThanOrEqualExpression exp);
        /// <summary>
        /// 翻译DB按位且操作操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbBitAndExpression exp);
        /// <summary>
        /// 翻译DB并且操作操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbAndExpression exp);
        /// <summary>
        /// 翻译DB按位或操作操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbBitOrExpression exp);
        /// <summary>
        /// 翻译DB或者操作操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbOrExpression exp);
        /// <summary>
        /// 翻译DB常量,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbConstantExpression exp);
        /// <summary>
        /// 翻译DB取反操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbNotExpression exp);
        /// <summary>
        /// 翻译DB中的NULL替换操作,生成SQL编码
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public abstract T Visit(DbNullConvertExpression exp);
        /// <summary>
        /// 翻译DB表访问操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一个DB表访问操作</param>
        /// <returns></returns>
        public abstract T Visit(DbTableExpression exp);
        /// <summary>
        /// 翻译DB子查询操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一次子DB子查询操作</param>
        /// <returns></returns>
        public abstract T Visit(DbSubQueryExpression exp);
        /// <summary>
        /// 翻译DB查询操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一次DB查询操作</param>
        /// <returns></returns>
        public abstract T Visit(DbSqlQueryExpression exp);
        /// <summary>
        /// 翻译DB查询中的FROM子句操作,生成SQL编码
        /// </summary>
        /// <param name="exp">查询中的FROM子句操作</param>
        /// <returns></returns>
        public abstract T Visit(DbFromTableExpression exp);
        /// <summary>
        /// 翻译DB查询中的JOIN连接操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一个DB查询中的JOIN连接操作</param>
        /// <returns></returns>
        public abstract T Visit(DbJoinTableExpression exp);
        /// <summary>
        /// 翻译支持的DB聚合操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一个DB聚合操作</param>
        /// <returns></returns>
        public abstract T Visit(DbAggregateExpression exp);

        /// <summary>
        /// 翻译DB插入操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一个DB插入动作</param>
        /// <returns></returns>
        public abstract T Visit(DbInsertExpression exp);
        /// <summary>
        /// 翻译DB更新操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一个DB更新动作</param>
        /// <returns></returns>
        public abstract T Visit(DbUpdateExpression exp);
        /// <summary>
        /// 翻译DB删除操作,生成SQL编码
        /// </summary>
        /// <param name="exp">一个DB删除动作</param>
        /// <returns></returns>
        public abstract T Visit(DbDeleteExpression exp);
    }
}
