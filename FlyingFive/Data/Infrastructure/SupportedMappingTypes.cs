using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Infrastructure
{
    /// <summary>
    /// 系统支持DB映射的数据类型
    /// </summary>
    public static class SupportedMappingTypes
    {
        private static readonly object _lockObj = new object();
        /// <summary>
        /// 默认支持的映射类型
        /// </summary>
        private static readonly Dictionary<Type, DbType> _defaultTypeMappers = null;
        /// <summary>
        /// 实际支持的映射类型(可以通过Register方法注册扩展支持的映射类型)
        /// </summary>
        private static readonly Dictionary<Type, DbType> _realTypeMappers = null;

        static SupportedMappingTypes()
        {
            _defaultTypeMappers = new Dictionary<Type, DbType>();
            _defaultTypeMappers[typeof(byte)] = DbType.Byte;
            _defaultTypeMappers[typeof(sbyte)] = DbType.SByte;
            _defaultTypeMappers[typeof(short)] = DbType.Int16;
            _defaultTypeMappers[typeof(ushort)] = DbType.UInt16;
            _defaultTypeMappers[typeof(int)] = DbType.Int32;
            _defaultTypeMappers[typeof(uint)] = DbType.UInt32;
            _defaultTypeMappers[typeof(long)] = DbType.Int64;
            _defaultTypeMappers[typeof(ulong)] = DbType.UInt64;
            _defaultTypeMappers[typeof(float)] = DbType.Single;
            _defaultTypeMappers[typeof(double)] = DbType.Double;
            _defaultTypeMappers[typeof(decimal)] = DbType.Decimal;
            _defaultTypeMappers[typeof(bool)] = DbType.Boolean;
            _defaultTypeMappers[typeof(string)] = DbType.String;
            _defaultTypeMappers[typeof(Guid)] = DbType.Guid;
            _defaultTypeMappers[typeof(DateTime)] = DbType.DateTime;
            _defaultTypeMappers[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            _defaultTypeMappers[typeof(TimeSpan)] = DbType.Time;
            _defaultTypeMappers[typeof(byte[])] = DbType.Binary;
            _defaultTypeMappers[typeof(Object)] = DbType.Object;

            _realTypeMappers = new Dictionary<Type, DbType>();
            foreach (var item in _defaultTypeMappers.Keys)
            {
                _realTypeMappers.Add(item, _defaultTypeMappers[item]);
            }
        }

        /// <summary>
        /// 从系统支持映射的数据类型中获取对应的DbType
        /// 不支持则返回null
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DbType? GetDbType(Type type)
        {
            if (type == null) { return null; }
            Type underlyingType = type.GetUnderlyingType();
            if (underlyingType.IsEnum)
            {
                underlyingType = typeof(Int32);
            }
            DbType retValue = DbType.Object;
            if (_realTypeMappers.TryGetValue(underlyingType, out retValue))
            {
                return retValue;
            }
            return null;
        }

        /// <summary>
        /// 判断类型是否系统支持映射的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsMappingType(Type type)
        {
            Type underlyingType = type.GetUnderlyingType();
            if (underlyingType.IsEnum) { return true; }
            var supported = _realTypeMappers.ContainsKey(underlyingType);
            return supported;
        }

        /// <summary>
        /// 注册一个需要映射的类型。
        /// </summary>
        /// <param name="csharpType">新增映射的C#数据类型</param>
        /// <param name="dbTypeToMap">映射的数据库DbType</param>
        public static void Register(Type csharpType, DbType dbTypeToMap)
        {
            if (csharpType == null)
            {
                throw new ArgumentNullException("参数: csharpType不能为null");
            }
            csharpType = csharpType.GetUnderlyingType();
            lock (_lockObj)
            {
                if (!_realTypeMappers.ContainsKey(csharpType))
                {
                    _realTypeMappers.Add(csharpType, dbTypeToMap);
                }
            }
        }
    }
}
