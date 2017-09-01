using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    public class DbExpressionTranslator : IDbExpressionTranslator
    {
        public static readonly DbExpressionTranslator Instance = new DbExpressionTranslator();

        public string Translate(DbExpression expression, out List<FakeParameter> parameters)
        {
            var generator = MsSqlGenerator.CreateInstance();
            expression.Accept(generator);

            parameters = generator.Parameters;
            string sql = generator.SqlBuilder.ToSql();

            return sql;
        }
    }
}
