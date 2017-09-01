using FlyingFive.Data.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    public partial class MsSqlGenerator
    {
        /// <summary>
        /// 初始化聚合操作处理
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Action<DbAggregateExpression, MsSqlGenerator>> InitAggregateHandlers()
        {
            var aggregateHandlers = new Dictionary<string, Action<DbAggregateExpression, MsSqlGenerator>>();
            aggregateHandlers.Add("Count", Aggregate_Count);
            aggregateHandlers.Add("LongCount", Aggregate_LongCount);
            aggregateHandlers.Add("Sum", Aggregate_Sum);
            aggregateHandlers.Add("Max", Aggregate_Max);
            aggregateHandlers.Add("Min", Aggregate_Min);
            aggregateHandlers.Add("Average", Aggregate_Average);

            var ret = UtilConstants.Clone(aggregateHandlers);
            return ret;
        }

        private static void Aggregate_Count(DbAggregateExpression exp, MsSqlGenerator generator)
        {
            Aggregate_Count(generator);
        }
        private static void Aggregate_LongCount(DbAggregateExpression exp, MsSqlGenerator generator)
        {
            Aggregate_LongCount(generator);
        }
        private static void Aggregate_Sum(DbAggregateExpression exp, MsSqlGenerator generator)
        {
            Aggregate_Sum(generator, exp.Arguments.First(), exp.Method.ReturnType);
        }
        private static void Aggregate_Max(DbAggregateExpression exp, MsSqlGenerator generator)
        {
            Aggregate_Max(generator, exp.Arguments.First(), exp.Method.ReturnType);
        }
        private static void Aggregate_Min(DbAggregateExpression exp, MsSqlGenerator generator)
        {
            Aggregate_Min(generator, exp.Arguments.First(), exp.Method.ReturnType);
        }
        private static void Aggregate_Average(DbAggregateExpression exp, MsSqlGenerator generator)
        {
            Aggregate_Average(generator, exp.Arguments.First(), exp.Method.ReturnType);
        }

        #region AggregateFunction
        private static void Aggregate_Count(MsSqlGenerator generator)
        {
            generator.SqlBuilder.Append("COUNT(1)");
        }
        private static void Aggregate_LongCount(MsSqlGenerator generator)
        {
            generator.SqlBuilder.Append("COUNT_BIG(1)");
        }
        private static void Aggregate_Max(MsSqlGenerator generator, DbExpression exp, Type retType)
        {
            AppendAggregateFunction(generator, exp, retType, "MAX", false);
        }
        private static void Aggregate_Min(MsSqlGenerator generator, DbExpression exp, Type retType)
        {
            AppendAggregateFunction(generator, exp, retType, "MIN", false);
        }
        private static void Aggregate_Sum(MsSqlGenerator generator, DbExpression exp, Type retType)
        {
            if (retType.IsNullable())
            {
                AppendAggregateFunction(generator, exp, retType, "SUM", true);
            }
            else
            {
                generator.SqlBuilder.Append("ISNULL(");
                AppendAggregateFunction(generator, exp, retType, "SUM", true);
                generator.SqlBuilder.Append(",");
                generator.SqlBuilder.Append("0");
                generator.SqlBuilder.Append(")");
            }
        }
        private static void Aggregate_Average(MsSqlGenerator generator, DbExpression exp, Type retType)
        {
            AppendAggregateFunction(generator, exp, retType, "AVG", true);
        }

        private static void AppendAggregateFunction(MsSqlGenerator generator, DbExpression exp, Type retType, string functionName, bool withCast)
        {
            string dbTypeString = null;
            if (withCast == true)
            {
                Type underlyingType = retType.GetUnderlyingType();
                if (underlyingType != UtilConstants.TypeOfDecimal/* We don't know the precision and scale,so,we can not cast exp to decimal,otherwise maybe cause problems. */ && CastTypeMap.TryGetValue(underlyingType, out dbTypeString))
                {
                    generator.SqlBuilder.Append("CAST(");
                }
            }

            generator.SqlBuilder.Append(functionName, "(");
            exp.Accept(generator);
            generator.SqlBuilder.Append(")");

            if (dbTypeString != null)
            {
                generator.SqlBuilder.Append(" AS ", dbTypeString, ")");
            }
        }
        #endregion
    }
}
