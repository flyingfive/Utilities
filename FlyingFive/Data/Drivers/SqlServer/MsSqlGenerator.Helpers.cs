using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    public partial class MsSqlGenerator
    {
        static MsSqlGenerator()
        {
            List<DbExpressionType> safeDbExpressionTypes = new List<DbExpressionType>();
            safeDbExpressionTypes.Add(DbExpressionType.MemberAccess);
            safeDbExpressionTypes.Add(DbExpressionType.ColumnAccess);
            safeDbExpressionTypes.Add(DbExpressionType.Constant);
            safeDbExpressionTypes.Add(DbExpressionType.Parameter);
            safeDbExpressionTypes.Add(DbExpressionType.Convert);
            SafeDbExpressionTypes = safeDbExpressionTypes.AsReadOnly();

            Dictionary<Type, string> castTypeMap = new Dictionary<Type, string>();
            castTypeMap.Add(typeof(string), "NVARCHAR(MAX)");
            castTypeMap.Add(typeof(byte), "TINYINT");
            castTypeMap.Add(typeof(Int16), "SMALLINT");
            castTypeMap.Add(typeof(int), "INT");
            castTypeMap.Add(typeof(long), "BIGINT");
            castTypeMap.Add(typeof(float), "REAL");
            castTypeMap.Add(typeof(double), "FLOAT");
            castTypeMap.Add(typeof(decimal), "DECIMAL(19,0)");//I think this will be a bug.
            castTypeMap.Add(typeof(bool), "BIT");
            castTypeMap.Add(typeof(DateTime), "DATETIME");
            castTypeMap.Add(typeof(Guid), "UNIQUEIDENTIFIER");
            CastTypeMap = UtilConstants.Clone(castTypeMap);


            Dictionary<Type, Type> numericTypes = new Dictionary<Type, Type>();
            numericTypes.Add(typeof(byte), typeof(byte));
            numericTypes.Add(typeof(sbyte), typeof(sbyte));
            numericTypes.Add(typeof(short), typeof(short));
            numericTypes.Add(typeof(ushort), typeof(ushort));
            numericTypes.Add(typeof(int), typeof(int));
            numericTypes.Add(typeof(uint), typeof(uint));
            numericTypes.Add(typeof(long), typeof(long));
            numericTypes.Add(typeof(ulong), typeof(ulong));
            numericTypes.Add(typeof(float), typeof(float));
            numericTypes.Add(typeof(double), typeof(double));
            numericTypes.Add(typeof(decimal), typeof(decimal));
            NumericTypes = UtilConstants.Clone(numericTypes);
        }

        /// <summary>
        /// 创建参数名称
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        private static string CreateParameterName(int ordinal)
        {
            var paramName = string.Format("{0}{1}", ParameterPrefix, ordinal.ToString());
            return paramName;
        }
        
        /// <summary>
        /// 修改DB参数表达式的DbType信息
        /// </summary>
        /// <param name="column"></param>
        /// <param name="exp"></param>
        private static void AmendDbInfo(DbColumn column, DbExpression exp)
        {
            if (column.DbType == null || exp.NodeType != DbExpressionType.Parameter) { return; }
            DbParameterExpression expToAmend = (DbParameterExpression)exp;
            if (expToAmend.DbType == null)
            {
                expToAmend.DbType = column.DbType;
            }
        }

        /// <summary>
        /// 确认DB操作是否返回了C#中的布尔值
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        private static DbExpression EnsureDbExpressionReturnCSharpBoolean(DbExpression exp)
        {
            if (exp.Type != UtilConstants.TypeOfBoolean && exp.Type != UtilConstants.TypeOfBoolean_Nullable)
                return exp;

            if (SafeDbExpressionTypes.Contains(exp.NodeType))
            {
                return exp;
            }

            //将且认为不符合上述条件的都是诸如 a.Id>1,a.Name=="name" 等不能作为 bool 返回值的表达式
            //构建 case when 
            return ConstructReturnCSharpBooleanCaseWhenExpression(exp);
        }

        /// <summary>
        /// 构建返回C#布尔值的CASE WHEN形式的DB操作
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static DbCaseWhenExpression ConstructReturnCSharpBooleanCaseWhenExpression(DbExpression exp)
        {
            // case when 1>0 then 1 when not (1>0) then 0 else Null end
            DbCaseWhenExpression.WhenThenExpressionPair whenThenPair = new DbCaseWhenExpression.WhenThenExpressionPair(exp, DbConstantExpression.True);
            DbCaseWhenExpression.WhenThenExpressionPair whenThenPair1 = new DbCaseWhenExpression.WhenThenExpressionPair(DbExpression.Not(exp), DbConstantExpression.False);
            List<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(2);
            whenThenExps.Add(whenThenPair);
            whenThenExps.Add(whenThenPair1);
            DbCaseWhenExpression caseWhenExpression = DbExpression.CaseWhen(whenThenExps, DbConstantExpression.Null, UtilConstants.TypeOfBoolean);

            return caseWhenExpression;
        }

        private static bool TryGetCastTargetDbTypeString(Type sourceType, Type targetType, out string dbTypeString, bool throwNotSupportedException = true)
        {
            dbTypeString = null;

            sourceType = sourceType.GetUnderlyingType();
            targetType = targetType.GetUnderlyingType();

            if (sourceType == targetType)
                return false;

            if (targetType == UtilConstants.TypeOfDecimal)
            {
                //Casting to Decimal is not supported when missing the precision and scale information.I have no idea to deal with this case now.
                if (sourceType != UtilConstants.TypeOfInt16 && sourceType != UtilConstants.TypeOfInt32 && sourceType != UtilConstants.TypeOfInt64 && sourceType != UtilConstants.TypeOfByte)
                {
                    if (throwNotSupportedException)
                    {
                        throw new NotSupportedException(string.Format("指定类型 '{0}' 不能转换为: '{1}'.", sourceType.FullName, targetType.FullName));
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            if (CastTypeMap.TryGetValue(targetType, out dbTypeString))
            {
                return true;
            }

            if (throwNotSupportedException)
                throw new NotSupportedException(string.Format("指定类型 '{0}' 不能转换为: '{1}'.", sourceType.FullName, targetType.FullName));
            else
                return false;
        }

        /// <summary>
        /// 确认DB操作中的方法是否与指定方法一致
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="methodInfo"></param>
        private static void EnsureMethod(DbMethodCallExpression exp, MethodInfo methodInfo)
        {
            if (exp.Method != methodInfo)
            {
                throw new NotSupportedException(string.Format("不支持的方法调用: {0}", exp.Method.Name));
            }
        }


        private static void EnsureTrimCharArgumentIsSpaces(DbExpression exp)
        {
            var m = exp as DbMemberExpression;
            if (m == null)
                throw new NotSupportedException();

            DbParameterExpression p;
            if (!m.TryConvertToParameterExpression(out p))
            {
                throw new NotSupportedException();
            }

            var arg = p.Value;

            if (arg == null)
                throw new NotSupportedException();

            var chars = arg as char[];
            if (chars.Length != 1 || chars[0] != ' ')
            {
                throw new NotSupportedException();
            }
        }

        private static void EnsureMethodDeclaringType(DbMethodCallExpression exp, Type ensureType)
        {
            if (exp.Method.DeclaringType != ensureType)
                throw UtilExceptions.NotSupportedMethod(exp.Method);
        }


        private static void DbFunction_DATEADD(MsSqlGenerator generator, string interval, DbMethodCallExpression exp)
        {
            generator.SqlBuilder.Append("DATEADD(");
            generator.SqlBuilder.Append(interval);
            generator.SqlBuilder.Append(",");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(",");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(")");
        }
        private static void DbFunction_DATEPART(MsSqlGenerator generator, string interval, DbExpression exp)
        {
            generator.SqlBuilder.Append("DATEPART(");
            generator.SqlBuilder.Append(interval);
            generator.SqlBuilder.Append(",");
            exp.Accept(generator);
            generator.SqlBuilder.Append(")");
        }
        private static void DbFunction_DATEDIFF(MsSqlGenerator generator, string interval, DbExpression startDateTimeExp, DbExpression endDateTimeExp)
        {
            generator.SqlBuilder.Append("DATEDIFF(");
            generator.SqlBuilder.Append(interval);
            generator.SqlBuilder.Append(",");
            startDateTimeExp.Accept(generator);
            generator.SqlBuilder.Append(",");
            endDateTimeExp.Accept(generator);
            generator.SqlBuilder.Append(")");
        }

        private static void AmendDbInfo(DbExpression exp1, DbExpression exp2)
        {
            DbColumnAccessExpression datumPointExp = null;
            DbParameterExpression expToAmend = null;

            DbExpression e = Trim_Nullable_Value(exp1);
            if (e.NodeType == DbExpressionType.ColumnAccess && exp2.NodeType == DbExpressionType.Parameter)
            {
                datumPointExp = (DbColumnAccessExpression)e;
                expToAmend = (DbParameterExpression)exp2;
            }
            else if ((e = Trim_Nullable_Value(exp2)).NodeType == DbExpressionType.ColumnAccess && exp1.NodeType == DbExpressionType.Parameter)
            {
                datumPointExp = (DbColumnAccessExpression)e;
                expToAmend = (DbParameterExpression)exp1;
            }
            else
                return;

            if (datumPointExp.Column.DbType != null)
            {
                if (expToAmend.DbType == null)
                    expToAmend.DbType = datumPointExp.Column.DbType;
            }
        }
        private static DbExpression Trim_Nullable_Value(DbExpression exp)
        {
            DbMemberExpression memberExp = exp as DbMemberExpression;
            if (memberExp == null)
                return exp;

            if (memberExp.Member.Name == "Value" && memberExp.Expression.Type.IsNullable())
                return memberExp.Expression;

            return exp;
        }

        private static Stack<DbExpression> GatherBinaryExpressionOperand(DbBinaryExpression exp)
        {
            DbExpressionType nodeType = exp.NodeType;
            Stack<DbExpression> items = new Stack<DbExpression>();
            items.Push(exp.Right);
            DbExpression left = exp.Left;
            while (left.NodeType == nodeType)
            {
                exp = (DbBinaryExpression)left;
                items.Push(exp.Right);
                left = exp.Left;
            }
            items.Push(left);
            return items;
        }

        private static string GenRowNumberName(List<DbColumnSegment> columns)
        {
            int ROW_NUMBER_INDEX = 1;
            string row_numberName = "ROW_NUMBER_0";
            while (columns.Any(a => string.Equals(a.Alias, row_numberName, StringComparison.OrdinalIgnoreCase)))
            {
                row_numberName = "ROW_NUMBER_" + ROW_NUMBER_INDEX.ToString();
                ROW_NUMBER_INDEX++;
            }

            return row_numberName;
        }
    }
}
