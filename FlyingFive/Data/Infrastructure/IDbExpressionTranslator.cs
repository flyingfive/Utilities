using FlyingFive.Data.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Infrastructure
{
    /// <summary>
    /// DB表现形式的翻译器
    /// </summary>
    public interface IDbExpressionTranslator
    {
        /// <summary>
        /// 翻译一个DB表现,生成完整的SQL代码
        /// </summary>
        /// <param name="expression">一个DB操作表现</param>
        /// <param name="parameters">DB操作中产生的参数列表</param>
        /// <returns></returns>
        string Translate(DbExpression expression, out List<FakeParameter> parameters);
    }
}
