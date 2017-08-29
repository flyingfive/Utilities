using FlyingFive.Data.Emit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Mapper
{
    /// <summary>
    /// 映射类型构造器
    /// </summary>
    public class MappingTypeConstructor
    {
        /// <summary>
        /// 支持DB映射的数据类型
        /// </summary>
        public Type MappingType { get; private set; }

        /// <summary>
        /// 从IDataReader获取指定类型数据的生成器
        /// </summary>
        public Func<IDataReader, int, object> DataCreator { get; private set; }

        private MappingTypeConstructor(Type type)
        {
            this.MappingType = type;
            this.DataCreator = DelegateGenerator.CreateMappingTypeGenerator(MappingType);
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, MappingTypeConstructor> _constructorCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, MappingTypeConstructor>();

        /// <summary>
        /// 获取指定映射类型的构造器实例
        /// </summary>
        /// <param name="type">映射类型</param>
        /// <returns></returns>
        public static MappingTypeConstructor GetInstance(Type type)
        {
            MappingTypeConstructor instance;
            if (!_constructorCache.TryGetValue(type, out instance))
            {
                lock (type)
                {
                    if (!_constructorCache.TryGetValue(type, out instance))
                    {
                        instance = new MappingTypeConstructor(type);
                        _constructorCache.GetOrAdd(type, instance);
                    }
                }
            }
            return instance;
        }
    }
}
