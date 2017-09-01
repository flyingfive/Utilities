using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data
{
    /// <summary>
    /// 常用固定定义
    /// </summary>
    public static class UtilConstants
    {
        public const string DefaultTableAlias = "T";
        public const string DefaultColumnAlias = "C";

        public static readonly Type TypeOfVoid = typeof(void);
        public static readonly Type TypeOfInt16 = typeof(Int16);
        public static readonly Type TypeOfInt32 = typeof(Int32);
        public static readonly Type TypeOfInt64 = typeof(Int64);
        public static readonly Type TypeOfDecimal = typeof(Decimal);
        public static readonly Type TypeOfDouble = typeof(Double);
        public static readonly Type TypeOfSingle = typeof(Single);
        public static readonly Type TypeOfBoolean = typeof(Boolean);
        public static readonly Type TypeOfBoolean_Nullable = typeof(Boolean?);
        public static readonly Type TypeOfDateTime = typeof(DateTime);
        public static readonly Type TypeOfGuid = typeof(Guid);
        public static readonly Type TypeOfByte = typeof(Byte);
        public static readonly Type TypeOfChar = typeof(Char);
        public static readonly Type TypeOfString = typeof(String);
        public static readonly Type TypeOfObject = typeof(Object);
        public static readonly Type TypeOfTimeSpan = typeof(TimeSpan);
        public static readonly Type TypeOfByteArray = typeof(Byte[]);

        public static readonly Type TypeOfMath = typeof(Math);

        #region DbExpression constants

        public static readonly DbParameterExpression DbParameter_1 = DbExpression.Parameter(1);
        public static readonly DbConstantExpression DbConstant_Null_String = DbExpression.Constant(null, typeof(string));

        public static readonly ConstantExpression Constant_Null_String = Expression.Constant(null, typeof(string));
        public static readonly ConstantExpression Constant_Empty_String = Expression.Constant(string.Empty);
        public static readonly ConstantExpression Constant_Null_Boolean = Expression.Constant(null, typeof(Boolean?));
        public static readonly ConstantExpression Constant_True = Expression.Constant(true);
        public static readonly ConstantExpression Constant_False = Expression.Constant(false);
        public static readonly UnaryExpression Convert_TrueToNullable = Expression.Convert(Expression.Constant(true), typeof(Boolean?));
        public static readonly UnaryExpression Convert_FalseToNullable = Expression.Convert(Expression.Constant(false), typeof(Boolean?));

        #endregion

        #region MemberInfo constants

        public static readonly PropertyInfo PropertyInfo_String_Length = typeof(string).GetProperty("Length");

        /* DateTime */
        public static readonly PropertyInfo PropertyInfo_DateTime_Now = typeof(DateTime).GetProperty("Now");
        public static readonly PropertyInfo PropertyInfo_DateTime_UtcNow = typeof(DateTime).GetProperty("UtcNow");
        public static readonly PropertyInfo PropertyInfo_DateTime_Today = typeof(DateTime).GetProperty("Today");
        public static readonly PropertyInfo PropertyInfo_DateTime_Date = typeof(DateTime).GetProperty("Date");
        public static readonly PropertyInfo PropertyInfo_DateTime_Year = typeof(DateTime).GetProperty("Year");
        public static readonly PropertyInfo PropertyInfo_DateTime_Month = typeof(DateTime).GetProperty("Month");
        public static readonly PropertyInfo PropertyInfo_DateTime_Day = typeof(DateTime).GetProperty("Day");
        public static readonly PropertyInfo PropertyInfo_DateTime_Hour = typeof(DateTime).GetProperty("Hour");
        public static readonly PropertyInfo PropertyInfo_DateTime_Minute = typeof(DateTime).GetProperty("Minute");
        public static readonly PropertyInfo PropertyInfo_DateTime_Second = typeof(DateTime).GetProperty("Second");
        public static readonly PropertyInfo PropertyInfo_DateTime_Millisecond = typeof(DateTime).GetProperty("Millisecond");
        public static readonly PropertyInfo PropertyInfo_DateTime_DayOfWeek = typeof(DateTime).GetProperty("DayOfWeek");


        /* String */
        public static readonly MethodInfo MethodInfo_String_Concat_String_String = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
        public static readonly MethodInfo MethodInfo_String_Concat_Object_Object = typeof(string).GetMethod("Concat", new Type[] { typeof(object), typeof(object) });
        public static readonly MethodInfo MethodInfo_String_Trim = typeof(string).GetMethod("Trim", Type.EmptyTypes);
        public static readonly MethodInfo MethodInfo_String_TrimStart = typeof(string).GetMethod("TrimStart", new Type[] { typeof(char[]) });
        public static readonly MethodInfo MethodInfo_String_TrimEnd = typeof(string).GetMethod("TrimEnd", new Type[] { typeof(char[]) });
        public static readonly MethodInfo MethodInfo_String_StartsWith = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
        public static readonly MethodInfo MethodInfo_String_EndsWith = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
        public static readonly MethodInfo MethodInfo_String_Contains = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
        public static readonly MethodInfo MethodInfo_String_IsNullOrEmpty = typeof(string).GetMethod("IsNullOrEmpty", new Type[] { typeof(string) });
        public static readonly MethodInfo MethodInfo_String_ToUpper = typeof(string).GetMethod("ToUpper", Type.EmptyTypes);
        public static readonly MethodInfo MethodInfo_String_ToLower = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
        public static readonly MethodInfo MethodInfo_String_Substring_Int32 = typeof(string).GetMethod("Substring", new Type[] { typeof(Int32) });
        public static readonly MethodInfo MethodInfo_String_Substring_Int32_Int32 = typeof(string).GetMethod("Substring", new Type[] { typeof(Int32), typeof(Int32) });

        public static readonly MethodInfo MethodInfo_Guid_NewGuid = typeof(Guid).GetMethod("NewGuid");

        /* DbFunctions */
        public static readonly MethodInfo MethodInfo_DbFunctions_DiffYears = typeof(DbFunctions).GetMethod("DiffYears");
        public static readonly MethodInfo MethodInfo_DbFunctions_DiffMonths = typeof(DbFunctions).GetMethod("DiffMonths");
        public static readonly MethodInfo MethodInfo_DbFunctions_DiffDays = typeof(DbFunctions).GetMethod("DiffDays");
        public static readonly MethodInfo MethodInfo_DbFunctions_DiffHours = typeof(DbFunctions).GetMethod("DiffHours");
        public static readonly MethodInfo MethodInfo_DbFunctions_DiffMinutes = typeof(DbFunctions).GetMethod("DiffMinutes");
        public static readonly MethodInfo MethodInfo_DbFunctions_DiffSeconds = typeof(DbFunctions).GetMethod("DiffSeconds");
        public static readonly MethodInfo MethodInfo_DbFunctions_DiffMilliseconds = typeof(DbFunctions).GetMethod("DiffMilliseconds");
        public static readonly MethodInfo MethodInfo_DbFunctions_DiffMicroseconds = typeof(DbFunctions).GetMethod("DiffMicroseconds");
        #endregion


        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(Dictionary<TKey, TValue> source, IEqualityComparer<TKey> comparer = null)
        {
            Dictionary<TKey, TValue> ret;
            if (comparer == null)
                ret = new Dictionary<TKey, TValue>(source.Count);
            else
                ret = new Dictionary<TKey, TValue>(source.Count, comparer);

            foreach (var kv in source)
            {
                ret.Add(kv.Key, kv.Value);
            }

            return ret;
        }

        public static Type GetFuncDelegateType(Type[] typeArguments)
        {
            int parameters = typeArguments.Length;
            Type funcType = null;
            switch (parameters)
            {
                case 3:
                    funcType = typeof(Func<,,>);
                    break;
                case 4:
                    funcType = typeof(Func<,,,>);
                    break;
                case 5:
                    funcType = typeof(Func<,,,,>);
                    break;
                case 6:
                    funcType = typeof(Func<,,,,,>);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return funcType.MakeGenericType(typeArguments);
        }

        public static string AppendDbCommandInfo(string cmdText, FakeParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    if (param == null)
                        continue;

                    string typeName = null;
                    object value = null;
                    Type parameterType;
                    if (param.Value == null || param.Value == DBNull.Value)
                    {
                        parameterType = param.DataType;
                        value = "NULL";
                    }
                    else
                    {
                        value = param.Value;
                        parameterType = param.Value.GetType();

                        if (parameterType == typeof(string) || parameterType == typeof(DateTime))
                            value = "'" + value + "'";
                    }

                    if (parameterType != null)
                        typeName = GetTypeName(parameterType);

                    sb.AppendFormat("{0} {1} = {2};", typeName, param.Name, value);
                    sb.AppendLine();
                }
            }

            sb.AppendLine(cmdText);

            return sb.ToString();
        }
        private static string GetTypeName(Type type)
        {
            Type underlyingType;
            if (type.IsNullable(out underlyingType))
            {
                return string.Format("Nullable<{0}>", GetTypeName(underlyingType));
            }

            return type.Name;
        }

        public static string GenerateUniqueColumnAlias(DbSqlQueryExpression sqlQuery, string defaultAlias = UtilConstants.DefaultColumnAlias)
        {
            string alias = defaultAlias;
            int i = 0;
            while (sqlQuery.ColumnSegments.Any(a => string.Equals(a.Alias, alias, StringComparison.OrdinalIgnoreCase)))
            {
                alias = defaultAlias + i.ToString();
                i++;
            }

            return alias;
        }

        public static bool AreEqual(object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 != null)
            {
                return obj1.Equals(obj2);
            }

            if (obj2 != null)
            {
                return obj2.Equals(obj1);
            }

            return object.Equals(obj1, obj2);
        }
    }

    public static class UtilExceptions
    {
        public static void CheckNull(object obj, string paramName = null)
        {
            if (obj == null)
                throw new ArgumentNullException(paramName);
        }

        public static NotSupportedException NotSupportedMethod(MethodInfo method)
        {
            return new NotSupportedException(string.Format("Does not support method '{0}'.", ToMethodString(method)));
        }

        public static string ToMethodString(MethodInfo method)
        {
            StringBuilder sb = new StringBuilder();
            ParameterInfo[] parameters = method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo p = parameters[i];

                if (i > 0)
                    sb.Append(",");

                string s = null;
                if (p.IsOut)
                    s = "out ";

                sb.AppendFormat("{0}{1} {2}", s, p.ParameterType.Name, p.Name);
            }

            return string.Format("{0}.{1}({2})", method.DeclaringType.Name, method.Name, sb.ToString());
        }
    }
}
