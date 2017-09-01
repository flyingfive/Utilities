using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    /// <summary>
    /// MsSql表达式翻译器,将Db表现形式翻译成MsSql规范的SQL代码
    /// </summary>
    public class MsSqlExpressionTranslator : IDbExpressionTranslator
    {
        public string Translate(DbExpression expression, out List<FakeParameter> parameters)
        {
            var generator = new MsSqlGenerator();//.CreateInstance();
            expression.Accept(generator);

            parameters = generator.Parameters;
            string sql = generator.SqlBuilder.ToSql();

            return sql;
        }
    }
}
