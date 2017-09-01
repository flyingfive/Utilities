using FlyingFive.Data.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    public partial class MsSqlGenerator
    {

        private static Dictionary<MethodInfo, Action<DbBinaryExpression, MsSqlGenerator>> InitBinaryWithMethodHandlers()
        {
            var binaryWithMethodHandlers = new Dictionary<MethodInfo, Action<DbBinaryExpression, MsSqlGenerator>>();
            binaryWithMethodHandlers.Add(UtilConstants.MethodInfo_String_Concat_String_String, StringConcat);
            binaryWithMethodHandlers.Add(UtilConstants.MethodInfo_String_Concat_Object_Object, StringConcat);

            var ret = UtilConstants.Clone(binaryWithMethodHandlers);
            return ret;
        }

        private static void StringConcat(DbBinaryExpression exp, MsSqlGenerator generator)
        {
            List<DbExpression> operands = new List<DbExpression>();
            operands.Add(exp.Right);

            DbExpression left = exp.Left;
            DbAddExpression e = null;
            while ((e = (left as DbAddExpression)) != null && (e.Method == UtilConstants.MethodInfo_String_Concat_String_String || e.Method == UtilConstants.MethodInfo_String_Concat_Object_Object))
            {
                operands.Add(e.Right);
                left = e.Left;
            }

            operands.Add(left);

            DbExpression whenExp = null;
            List<DbExpression> operandExps = new List<DbExpression>(operands.Count);
            for (int i = operands.Count - 1; i >= 0; i--)
            {
                DbExpression operand = operands[i];
                DbExpression opBody = operand;
                if (opBody.Type != UtilConstants.TypeOfString)
                {
                    // 需要 cast type
                    opBody = DbExpression.Convert(opBody, UtilConstants.TypeOfString);
                }

                DbExpression equalNullExp = DbExpression.Equal(opBody, UtilConstants.DbConstant_Null_String);

                if (whenExp == null)
                    whenExp = equalNullExp;
                else
                    whenExp = DbExpression.And(whenExp, equalNullExp);

                operandExps.Add(opBody);
            }

            generator.SqlBuilder.Append("CASE", " WHEN ");
            whenExp.Accept(generator);
            generator.SqlBuilder.Append(" THEN ");
            DbConstantExpression.Null.Accept(generator);
            generator.SqlBuilder.Append(" ELSE ");

            generator.SqlBuilder.Append("(");

            for (int i = 0; i < operandExps.Count; i++)
            {
                if (i > 0)
                    generator.SqlBuilder.Append(" + ");

                generator.SqlBuilder.Append("ISNULL(");
                operandExps[i].Accept(generator);
                generator.SqlBuilder.Append(",");
                DbConstantExpression.StringEmpty.Accept(generator);
                generator.SqlBuilder.Append(")");
            }

            generator.SqlBuilder.Append(")");

            generator.SqlBuilder.Append(" END");
        }
    }
}
