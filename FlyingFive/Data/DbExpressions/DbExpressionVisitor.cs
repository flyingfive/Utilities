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
        /// 翻译DB列访问操作，生成SQL编码
        /// </summary>
        /// <param name="exp">一个列访问操作</param>
        /// <returns></returns>
        public abstract T Visit(DbColumnAccessExpression exp);
    }
}
