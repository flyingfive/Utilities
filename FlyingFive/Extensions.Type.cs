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
        /// 指示当前类型是否自定义类型
        /// </summary>
        /// <param name="type">要判断的类型</param>
        /// <returns></returns>
        public static bool IsCustomType(this Type type)
        {
            if (type.IsPrimitive) { return false; }
            if (type.IsArray && type.HasElementType && type.GetElementType().IsPrimitive) { return false; }
            bool isCustomType = (type != typeof(object) && type != typeof(Guid) &&
                Type.GetTypeCode(type) == TypeCode.Object && !type.IsGenericType);
            return isCustomType;
        }

        /// <summary>
        /// 转换为对应的数据库类型
        /// </summary>
        /// <param name="csharpType">C#内置类型</param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(this Type csharpType)
        {
            if (csharpType.IsNullableType())
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
                default: return SqlDbType.VarChar;
            }
        }

        /// <summary>
        /// 转换为对应的数据库类型
        /// </summary>
        /// <param name="csharpType">C#内置类型</param>
        /// <returns></returns>
        public static DbType ToDbType(this Type csharpType)
        {
            if (csharpType.IsNullableType())
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
        public static bool IsNullableType(this Type type)
        {
            Type underlyingType = null;
            return IsNullableType(type, out underlyingType);
        }

        /// <summary>
        /// 判断类型是否为可空类型,是则返回该类型的实际类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="underlyingType">可空类型</param>
        /// <returns></returns>
        public static bool IsNullableType(this Type type, out Type underlyingType)
        {
            underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType != null;
        }

        /// <summary>
        /// 是否泛型集合类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsListType(this Type type)
        {
            var flag = type.IsGenericType &&
                type.GetGenericTypeDefinition().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
            return flag;
        }


        /// <summary>
        /// 判断指定该类型是否为匿名类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAnonymousType(this Type type)
        {
            const string csharpAnonPrefix = "<>f__AnonymousType";
            const string vbAnonPrefix = "VB$Anonymous";
            var typeName = type.Name;
            return typeName.StartsWith(csharpAnonPrefix) || typeName.StartsWith(vbAnonPrefix);
        }

        /// <summary>
        /// 获取可空类型的实际值类型,如:int? => int
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetUnderlyingType(this Type type)
        {
            Type underlyingType;
            if (!IsNullableType(type, out underlyingType))
            {
                underlyingType = type;
            }
            return underlyingType;
        }
    }
}
