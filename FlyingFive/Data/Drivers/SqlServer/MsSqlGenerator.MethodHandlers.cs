using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    public partial class MsSqlGenerator
    {
        private static readonly Dictionary<string, Action<DbMethodCallExpression, MsSqlGenerator>> MethodHandlers = InitMethodHandlers();
        private static readonly Dictionary<string, Action<DbAggregateExpression, MsSqlGenerator>> AggregateHandlers = InitAggregateHandlers();
        private static readonly Dictionary<MethodInfo, Action<DbBinaryExpression, MsSqlGenerator>> BinaryWithMethodHandlers = InitBinaryWithMethodHandlers();
        private static readonly Dictionary<Type, string> CastTypeMap;
        private static readonly Dictionary<Type, Type> NumericTypes;


        public static readonly ReadOnlyCollection<DbExpressionType> SafeDbExpressionTypes;

        /// <summary>
        /// 支持的方法调用
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Action<DbMethodCallExpression, MsSqlGenerator>> InitMethodHandlers()
        {
            var methodHandlers = new Dictionary<string, Action<DbMethodCallExpression, MsSqlGenerator>>();

            methodHandlers.Add("Equals", Method_Equals);

            methodHandlers.Add("Trim", Method_Trim);
            methodHandlers.Add("TrimStart", Method_TrimStart);
            methodHandlers.Add("TrimEnd", Method_TrimEnd);
            methodHandlers.Add("StartsWith", Method_StartsWith);
            methodHandlers.Add("EndsWith", Method_EndsWith);
            methodHandlers.Add("ToUpper", Method_String_ToUpper);
            methodHandlers.Add("ToLower", Method_String_ToLower);
            methodHandlers.Add("Substring", Method_String_Substring);
            methodHandlers.Add("IsNullOrEmpty", Method_String_IsNullOrEmpty);

            methodHandlers.Add("Contains", Method_Contains);

            methodHandlers.Add("Count", Method_Count);
            methodHandlers.Add("LongCount", Method_LongCount);
            methodHandlers.Add("Sum", Method_Sum);
            methodHandlers.Add("Max", Method_Max);
            methodHandlers.Add("Min", Method_Min);
            methodHandlers.Add("Average", Method_Average);

            methodHandlers.Add("AddYears", Method_DateTime_AddYears);
            methodHandlers.Add("AddMonths", Method_DateTime_AddMonths);
            methodHandlers.Add("AddDays", Method_DateTime_AddDays);
            methodHandlers.Add("AddHours", Method_DateTime_AddHours);
            methodHandlers.Add("AddMinutes", Method_DateTime_AddMinutes);
            methodHandlers.Add("AddSeconds", Method_DateTime_AddSeconds);
            methodHandlers.Add("AddMilliseconds", Method_DateTime_AddMilliseconds);

            methodHandlers.Add("Parse", Method_Parse);

            methodHandlers.Add("NewGuid", Method_Guid_NewGuid);

            methodHandlers.Add("DiffYears", Method_DbFunctions_DiffYears);
            methodHandlers.Add("DiffMonths", Method_DbFunctions_DiffMonths);
            methodHandlers.Add("DiffDays", Method_DbFunctions_DiffDays);
            methodHandlers.Add("DiffHours", Method_DbFunctions_DiffHours);
            methodHandlers.Add("DiffMinutes", Method_DbFunctions_DiffMinutes);
            methodHandlers.Add("DiffSeconds", Method_DbFunctions_DiffSeconds);
            methodHandlers.Add("DiffMilliseconds", Method_DbFunctions_DiffMilliseconds);
            methodHandlers.Add("DiffMicroseconds", Method_DbFunctions_DiffMicroseconds);

            methodHandlers.Add("Abs", Method_Math_Abs);

            var ret = UtilConstants.Clone(methodHandlers);
            return ret;
        }

        private static void Method_Equals(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            if (exp.Method.ReturnType != UtilConstants.TypeOfBoolean || exp.Method.IsStatic || exp.Method.GetParameters().Length != 1)
                throw UtilExceptions.NotSupportedMethod(exp.Method);

            DbExpression right = exp.Arguments[0];
            if (right.Type != exp.Object.Type)
            {
                right = DbExpression.Convert(right, exp.Object.Type);
            }

            DbExpression.Equal(exp.Object, right).Accept(generator);
        }

        private static void Method_Trim(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_Trim);

            generator.SqlBuilder.Append("RTRIM(LTRIM(");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append("))");
        }
        private static void Method_TrimStart(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_TrimStart);
            EnsureTrimCharArgumentIsSpaces(exp.Arguments[0]);

            generator.SqlBuilder.Append("LTRIM(");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(")");
        }
        private static void Method_TrimEnd(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_TrimEnd);
            EnsureTrimCharArgumentIsSpaces(exp.Arguments[0]);

            generator.SqlBuilder.Append("RTRIM(");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(")");
        }
        private static void Method_StartsWith(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_StartsWith);

            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(" LIKE ");
            exp.Arguments.First().Accept(generator);
            generator.SqlBuilder.Append(" + '%'");
        }
        private static void Method_EndsWith(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_EndsWith);

            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(" LIKE '%' + ");
            exp.Arguments.First().Accept(generator);
        }
        private static void Method_String_Contains(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_Contains);

            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(" LIKE '%' + ");
            exp.Arguments.First().Accept(generator);
            generator.SqlBuilder.Append(" + '%'");
        }
        private static void Method_String_ToUpper(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_ToUpper);

            generator.SqlBuilder.Append("UPPER(");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(")");
        }
        private static void Method_String_ToLower(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_ToLower);

            generator.SqlBuilder.Append("LOWER(");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(")");
        }
        private static void Method_String_Substring(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            generator.SqlBuilder.Append("SUBSTRING(");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(",");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(" + 1");
            generator.SqlBuilder.Append(",");
            if (exp.Method == UtilConstants.MethodInfo_String_Substring_Int32)
            {
                var string_LengthExp = DbExpression.MemberAccess(UtilConstants.PropertyInfo_String_Length, exp.Object);
                string_LengthExp.Accept(generator);
            }
            else if (exp.Method == UtilConstants.MethodInfo_String_Substring_Int32_Int32)
            {
                exp.Arguments[1].Accept(generator);
            }
            else
                throw UtilExceptions.NotSupportedMethod(exp.Method);

            generator.SqlBuilder.Append(")");
        }
        private static void Method_String_IsNullOrEmpty(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_IsNullOrEmpty);

            DbExpression e = exp.Arguments.First();
            DbEqualExpression equalNullExpression = DbExpression.Equal(e, DbExpression.Constant(null, UtilConstants.TypeOfString));
            DbEqualExpression equalEmptyExpression = DbExpression.Equal(e, DbExpression.Constant(string.Empty));

            DbOrExpression orExpression = DbExpression.Or(equalNullExpression, equalEmptyExpression);

            DbCaseWhenExpression.WhenThenExpressionPair whenThenPair = new DbCaseWhenExpression.WhenThenExpressionPair(orExpression, DbConstantExpression.One);

            List<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(1);
            whenThenExps.Add(whenThenPair);

            DbCaseWhenExpression caseWhenExpression = DbExpression.CaseWhen(whenThenExps, DbConstantExpression.Zero, UtilConstants.TypeOfBoolean);

            var eqExp = DbExpression.Equal(caseWhenExpression, DbConstantExpression.One);
            eqExp.Accept(generator);
        }

        private static void Method_Contains(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            MethodInfo method = exp.Method;

            if (method.DeclaringType == UtilConstants.TypeOfString)
            {
                Method_String_Contains(exp, generator);
                return;
            }

            List<DbExpression> exps = new List<DbExpression>();
            IEnumerable values = null;
            DbExpression operand = null;

            Type declaringType = method.DeclaringType;

            if (typeof(IList).IsAssignableFrom(declaringType) || (declaringType.IsGenericType && typeof(ICollection<>).MakeGenericType(declaringType.GetGenericArguments()).IsAssignableFrom(declaringType)))
            {
                DbMemberExpression memberExp = exp.Object as DbMemberExpression;

                if (memberExp == null || !memberExp.IsEvaluable())
                    throw new NotSupportedException(exp.ToString());

                values = memberExp.Evaluate() as IEnumerable; //Enumerable
                operand = exp.Arguments[0];
                goto constructInState;
            }
            if (method.IsStatic && declaringType == typeof(Enumerable) && exp.Arguments.Count == 2)
            {
                DbMemberExpression memberExp = exp.Arguments[0] as DbMemberExpression;

                if (memberExp == null || !memberExp.IsEvaluable())
                    throw new NotSupportedException(exp.ToString());

                values = memberExp.Evaluate() as IEnumerable;
                operand = exp.Arguments[1];
                goto constructInState;
            }

            throw UtilExceptions.NotSupportedMethod(exp.Method);

        constructInState:
            foreach (object value in values)
            {
                if (value == null)
                    exps.Add(DbExpression.Constant(null, operand.Type));
                else
                    exps.Add(DbExpression.Parameter(value));
            }
            In(generator, exps, operand);
        }


        private static void In(MsSqlGenerator generator, List<DbExpression> elementExps, DbExpression operand)
        {
            if (elementExps.Count == 0)
            {
                generator.SqlBuilder.Append("1 = 0");
                return;
            }

            operand.Accept(generator);
            generator.SqlBuilder.Append(" IN (");

            for (int i = 0; i < elementExps.Count; i++)
            {
                if (i > 0)
                    generator.SqlBuilder.Append(",");

                elementExps[i].Accept(generator);
            }

            generator.SqlBuilder.Append(")");

            return;
        }

        private static void Method_Count(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, typeof(AggregateFunctions));
            Aggregate_Count(generator);
        }
        private static void Method_LongCount(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, typeof(AggregateFunctions));
            Aggregate_LongCount(generator);
        }
        private static void Method_Sum(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, typeof(AggregateFunctions));
            Aggregate_Sum(generator, exp.Arguments.First(), exp.Method.ReturnType);
        }
        private static void Method_Max(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, typeof(AggregateFunctions));
            Aggregate_Max(generator, exp.Arguments.First(), exp.Method.ReturnType);
        }
        private static void Method_Min(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, typeof(AggregateFunctions));
            Aggregate_Min(generator, exp.Arguments.First(), exp.Method.ReturnType);
        }
        private static void Method_Average(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, typeof(AggregateFunctions));
            Aggregate_Average(generator, exp.Arguments.First(), exp.Method.ReturnType);
        }

        private static void Method_DateTime_AddYears(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD(generator, "YEAR", exp);
        }
        private static void Method_DateTime_AddMonths(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD(generator, "MONTH", exp);
        }
        private static void Method_DateTime_AddDays(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD(generator, "DAY", exp);
        }
        private static void Method_DateTime_AddHours(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD(generator, "HOUR", exp);
        }
        private static void Method_DateTime_AddMinutes(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD(generator, "MINUTE", exp);
        }
        private static void Method_DateTime_AddSeconds(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD(generator, "SECOND", exp);
        }
        private static void Method_DateTime_AddMilliseconds(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD(generator, "MILLISECOND", exp);
        }

        private static void Method_Parse(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            if (exp.Arguments.Count != 1)
                throw UtilExceptions.NotSupportedMethod(exp.Method);

            DbExpression arg = exp.Arguments[0];
            if (arg.Type != UtilConstants.TypeOfString)
                throw UtilExceptions.NotSupportedMethod(exp.Method);

            Type retType = exp.Method.ReturnType;
            EnsureMethodDeclaringType(exp, retType);

            DbExpression e = DbExpression.Convert(arg, retType);
            if (retType == UtilConstants.TypeOfBoolean)
            {
                e.Accept(generator);
                generator.SqlBuilder.Append(" = ");
                DbConstantExpression.True.Accept(generator);
            }
            else
                e.Accept(generator);
        }

        private static void Method_Guid_NewGuid(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_Guid_NewGuid);

            generator.SqlBuilder.Append("NEWID()");
        }

        private static void Method_DbFunctions_DiffYears(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_DbFunctions_DiffYears);

            DbFunction_DATEDIFF(generator, "YEAR", exp.Arguments[0], exp.Arguments[1]);
        }
        private static void Method_DbFunctions_DiffMonths(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_DbFunctions_DiffMonths);

            DbFunction_DATEDIFF(generator, "MONTH", exp.Arguments[0], exp.Arguments[1]);
        }
        private static void Method_DbFunctions_DiffDays(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_DbFunctions_DiffDays);

            DbFunction_DATEDIFF(generator, "DAY", exp.Arguments[0], exp.Arguments[1]);
        }
        private static void Method_DbFunctions_DiffHours(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_DbFunctions_DiffHours);

            DbFunction_DATEDIFF(generator, "HOUR", exp.Arguments[0], exp.Arguments[1]);
        }
        private static void Method_DbFunctions_DiffMinutes(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_DbFunctions_DiffMinutes);

            DbFunction_DATEDIFF(generator, "MINUTE", exp.Arguments[0], exp.Arguments[1]);
        }
        private static void Method_DbFunctions_DiffSeconds(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_DbFunctions_DiffSeconds);

            DbFunction_DATEDIFF(generator, "SECOND", exp.Arguments[0], exp.Arguments[1]);
        }
        private static void Method_DbFunctions_DiffMilliseconds(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_DbFunctions_DiffMilliseconds);

            DbFunction_DATEDIFF(generator, "MILLISECOND", exp.Arguments[0], exp.Arguments[1]);
        }
        private static void Method_DbFunctions_DiffMicroseconds(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_DbFunctions_DiffMicroseconds);

            DbFunction_DATEDIFF(generator, "MICROSECOND", exp.Arguments[0], exp.Arguments[1]);
        }

        private static void Method_Math_Abs(DbMethodCallExpression exp, MsSqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfMath);

            generator.SqlBuilder.Append("ABS(");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }
}
