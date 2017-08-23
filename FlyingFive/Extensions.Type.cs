using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive
{
    /// <summary>
    /// 提供.NET类型扩展行为
    /// </summary>
    public static partial class Extensions
    {

        /// <summary>
        /// 转换为对应的数据库类型
        /// </summary>
        /// <param name="csharpType">C#内置类型</param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(this Type csharpType)
        {
            if (csharpType.IsNullable())
            {
                var type = csharpType.GetGenericArguments().FirstOrDefault();
                return type.ToSqlDbType();
            }
            switch (csharpType.Name.ToLowerInvariant())
            {
                case "int":
                case "int32": return SqlDbType.Int;
                case "int64": return SqlDbType.BigInt;
                case "guid": return SqlDbType.UniqueIdentifier;
                case "datetime": return SqlDbType.DateTime;
                case "byte[]": return SqlDbType.VarBinary;
                case "boolean": return SqlDbType.Bit;
                case "double":
                case "float": return SqlDbType.Float;
                case "decimal": return SqlDbType.Decimal;
                case "string":
                default: return SqlDbType.NVarChar;
            }
        }

        /// <summary>
        /// 转换为对应的数据库类型
        /// </summary>
        /// <param name="csharpType">C#内置类型</param>
        /// <returns></returns>
        public static DbType ToDbType(this Type csharpType)
        {
            if (csharpType.IsNullable())
            {
                var type = csharpType.GetGenericArguments().FirstOrDefault();
                return type.ToDbType();
            }
            switch (csharpType.Name.ToLowerInvariant())
            {
                case "int":
                case "int32": return DbType.Int32;
                case "int64": return DbType.Int64;
                case "guid": return DbType.Guid;
                case "datetime": return DbType.DateTime;
                case "byte[]": return DbType.Binary;
                case "boolean": return DbType.Boolean;
                case "double": return DbType.Double;
                case "float": return DbType.Single;
                case "decimal": return DbType.Decimal;
                case "string":
                default: return DbType.AnsiString;
            }
        }

        /// <summary>
        /// 判断类型是否为可空类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullable(this Type type)
        {
            Type underlyingType = null;
            return IsNullable(type, out underlyingType);
        }

        /// <summary>
        /// 判断类型是否为可空类型,是则返回该类型的实际类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="underlyingType">可空类型</param>
        /// <returns></returns>
        public static bool IsNullable(this Type type, out Type underlyingType)
        {
            underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType != null;
        }
    }
}
