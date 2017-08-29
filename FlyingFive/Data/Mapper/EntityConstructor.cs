using FlyingFive.Data.Emit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Mapper
{
    /// <summary>
    /// 表示实体的构造器信息
    /// </summary>
    public class EntityConstructor
    {
        /// <summary>
        /// 实体的构造器
        /// </summary>
        public ConstructorInfo ConstructorInfo { get; private set; }
        /// <summary>
        /// 生成的对象创建器
        /// </summary>
        public Func<IDataReader, DataReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> InstanceCreator { get; private set; }
        public EntityConstructor(ConstructorInfo constructorInfo)
        {
            if (constructorInfo.DeclaringType.IsAbstract)
            {
                throw new InvalidOperationException("抽象类不能被创建实例！");
            }
            this.ConstructorInfo = constructorInfo;
            this.InstanceCreator = DelegateGenerator.CreateObjectGenerator(constructorInfo);
        }

        private static readonly ConcurrentDictionary<ConstructorInfo, EntityConstructor> _instanceCreatorCache = new ConcurrentDictionary<ConstructorInfo, EntityConstructor>();

        /// <summary>
        /// 获取构造函数的实体构造器实例
        /// </summary>
        /// <param name="constructorInfo">构造函数</param>
        /// <returns></returns>
        public static EntityConstructor GetInstance(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
            {
                throw new ArgumentNullException("参数: constructorInfo不能为NULL");
            }
            EntityConstructor instance = null;
            if (!_instanceCreatorCache.TryGetValue(constructorInfo, out instance))
            {
                lock (constructorInfo)
                {
                    if (!_instanceCreatorCache.TryGetValue(constructorInfo, out instance))
                    {
                        instance = new EntityConstructor(constructorInfo);
                        _instanceCreatorCache.GetOrAdd(constructorInfo, instance);
                    }
                }
            }
            return instance;
        }
    }
}
