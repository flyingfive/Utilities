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
        [Obsolete("可能存在未知的不准确场景，可改用IsUserType", true)]
        public static bool IsCustomType(this Type type)
        {
            if (type.IsPrimitive) { return false; }
            if (type.IsArray && type.HasElementType && type.GetElementType().IsPrimitive) { return false; }
            bool isCustomType = (type != typeof(object) && type != typeof(Guid) &&
                Type.GetTypeCode(type) == TypeCode.Object && !type.IsGenericType);
            return isCustomType;
        }

        /// <summary>
        /// 指示当前类型是否用户类型
        /// </summary>
        /// <param name="type">System.Type实例</param>
        /// <returns></returns>
        public static bool IsUserType(this Type type)
        {
            type = type.GetUnderlyingType();
            if (type.IsPrimitive) { return false; }
            if (type.IsEnum) { return false; }
            if (type.IsArray && type.HasElementType && type.GetElementType().IsPrimitive) { return false; }
            var code = Type.GetTypeCode(type);
            var flag = code == TypeCode.Object && type != typeof(object) && !DataTypeExtension.SystemDataTypeName.Contains(code.ToString());
            return flag;
        }

        /// <summary>
        /// 指示当前类型是否为系统原生数据类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSystemType(this Type type)
        {
            type = type.GetUnderlyingType();
            if (type.IsPrimitive) { return true; }
            if (type.IsEnum) { return true; }
            if (type.IsArray && type.HasElementType && type.GetElementType().IsPrimitive) { return true; }
            var code = Type.GetTypeCode(type);
            return type.Namespace.StartsWith("System") && DataTypeExtension.SystemDataTypeName.Contains(code.ToString());
        }

        /// <summary>
        /// 转换为对应的数据库类型
        /// </summary>
        /// <param name="csharpType">C#内置类型</param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(this Type csharpType)
        {
            csharpType = csharpType.GetUnderlyingType();
            if (!csharpType.IsSystemType()) { throw new NotSupportedException("仅支持C#原生数据类型转换"); }
            switch (csharpType.Name.ToLowerInvariant())
            {
                case "int":
                case "int32": return SqlDbType.Int;
                case "biginteger":
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
        /// <param name="csharpType">System.Type实例</param>
        /// <returns></returns>
        public static DbType ToDbType(this Type csharpType)
        {
            csharpType = csharpType.GetUnderlyingType();
            if (!csharpType.IsSystemType()) { throw new NotSupportedException("仅支持C#原生数据类型转换"); }
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
        /// <param name="type">System.Type实例</param>
        /// <returns></returns>
        public static bool IsNullableType(this Type type)
        {
            Type underlyingType = null;
            return IsNullableType(type, ref underlyingType);
        }

        /// <summary>
        /// 判断类型是否为可空类型,是则返回该类型的实际类型
        /// </summary>
        /// <param name="type">System.Type实例</param>
        /// <param name="underlyingType">可空类型</param>
        /// <returns></returns>
        public static bool IsNullableType(this Type type, ref Type underlyingType)
        {
            if (type == null) { throw new ArgumentNullException("参数type不能为null."); }
            if (!type.IsGenericType) { return false; }
            var actType = Nullable.GetUnderlyingType(type);
            if (actType != null)
            {
                underlyingType = actType;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取可空类型的实际值类型,如:int? => int
        /// </summary>
        /// <param name="type">System.Type实例</param>
        /// <returns></returns>
        public static Type GetUnderlyingType(this Type type)
        {
            if (type == null) { throw new ArgumentNullException("参数type不能为null."); }
            Type underlyingType = null;
            if (!IsNullableType(type, ref underlyingType))
            {
                underlyingType = type;
            }
            return underlyingType;
        }

        /// <summary>
        /// 是否泛型集合类型
        /// </summary>
        /// <param name="type">System.Type实例</param>
        /// <returns></returns>
        public static bool IsGenericListType(this Type type)
        {
            if (type == null) { throw new ArgumentNullException("参数type不能为null."); }
            var flag = type.IsGenericType &&
                type.GetGenericTypeDefinition().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
            return flag;
        }


        /// <summary>
        /// 判断指定该类型是否为匿名类型
        /// </summary>
        /// <param name="type">System.Type实例</param>
        /// <returns></returns>
        public static bool IsAnonymousType(this Type type)
        {
            if (type == null) { throw new ArgumentNullException("参数type不能为null."); }
            const string csharpAnonPrefix = "<>f__AnonymousType";
            const string vbAnonPrefix = "VB$Anonymous";
            var typeName = type.Name;
            return typeName.StartsWith(csharpAnonPrefix) || typeName.StartsWith(vbAnonPrefix);
        }

        /// <summary>
        /// 判断一个类型是否存在默认的空参构造
        /// </summary>
        /// <param name="type">System.Type实例</param>
        /// <returns></returns>
        public static bool HasDefaultEmptyConstructor(this Type type)
        {
            if (type == null) { throw new ArgumentNullException("参数type不能为null."); }
            var defaultConstructor = type.GetConstructor(Type.EmptyTypes);
            return defaultConstructor != null;
        }
    }

    /// <summary>
    /// 数据类型扩展
    /// </summary>
    public class DataTypeExtension
    {
        /// <summary>
        /// 系统的数据类型名称
        /// </summary>
        public static readonly string[] SystemDataTypeName = Enum.GetNames(typeof(TypeCode))
                .Except(new string[] { "Empty", "Object" })                             //除掉这2个类型
                .Concat(new string[] { "BigInteger", "Guid" }).ToArray();               //加上这2个类型
    }
}
