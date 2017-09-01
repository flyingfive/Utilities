using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    /// <summary>
    /// MsSql表达式翻译器,将Db表现形式翻译成MsSql规范的SQL代码(2005及以上版本)
    /// </summary>
    public class MsSqlExpressionTranslator : IDbExpressionTranslator
    {
        public static readonly MsSqlExpressionTranslator Instance = new MsSqlExpressionTranslator();
        public string Translate(DbExpression expression, out List<FakeParameter> parameters)
        {
            var generator = MsSqlGenerator.CreateInstance();
            expression.Accept(generator);

            parameters = generator.Parameters;
            string sql = generator.SqlBuilder.ToSql();

            return sql;
        }
    }

    /// <summary>
    /// MsSql表达式翻译器,将Db表现形式翻译成MsSql规范的SQL代码(2012及以上版本)
    /// </summary>
    public class MsSqlExpressionTranslator_OffsetFetch : IDbExpressionTranslator
    {
        /// <summary>
        /// 表达式翻译器实例
        /// </summary>
        public static readonly MsSqlExpressionTranslator_OffsetFetch Instance = new MsSqlExpressionTranslator_OffsetFetch();

        public string Translate(DbExpression expression, out List<FakeParameter> parameters)
        {
            var generator = new MsSqlGenerator_OffsetFetch();
            expression.Accept(generator);

            parameters = generator.Parameters;
            string sql = generator.SqlBuilder.ToSql();

            return sql;
        }
    }
}
