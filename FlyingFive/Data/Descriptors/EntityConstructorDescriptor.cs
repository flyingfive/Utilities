using FlyingFive.Data.Mapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Descriptors
{
    /// <summary>
    /// 表示实体构造器的描述
    /// </summary>
    public class EntityConstructorDescriptor
    {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<ConstructorInfo, EntityConstructorDescriptor> _constructorDescriptorsCache = new System.Collections.Concurrent.ConcurrentDictionary<ConstructorInfo, EntityConstructorDescriptor>();

        private EntityMemberMapper _mapper = null;
        private EntityConstructor _entityConstructor = null;
        /// <summary>
        /// 实体构造函数
        /// </summary>
        public ConstructorInfo ConstructorInfo { get; private set; }
        /// <summary>
        /// 匿名类型构造函数的成员参数列表
        /// </summary>
        public Dictionary<MemberInfo, ParameterInfo> MemberParameterMap { get; private set; }

        EntityConstructorDescriptor(ConstructorInfo constructorInfo)
        {
            this.ConstructorInfo = constructorInfo;
            this.Init();
        }

        void Init()
        {
            ConstructorInfo constructor = this.ConstructorInfo;
            Type type = constructor.DeclaringType;

            if (type.IsAnonymousType())
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                this.MemberParameterMap = new Dictionary<MemberInfo, ParameterInfo>(parameters.Length);
                foreach (ParameterInfo parameter in parameters)
                {
                    PropertyInfo prop = type.GetProperty(parameter.Name);
                    this.MemberParameterMap.Add(prop, parameter);
                }
            }
            else
            {
                this.MemberParameterMap = new Dictionary<MemberInfo, ParameterInfo>(0);
            }
        }

        /// <summary>
        /// 获取实体的实例创建器
        /// </summary>
        /// <returns></returns>
        public Func<IDataReader, DataReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> GetInstanceCreator()
        {
            if (this._entityConstructor == null)
            {
                this._entityConstructor = EntityConstructor.GetInstance(this.ConstructorInfo);
            }
            return this._entityConstructor.InstanceCreator;
        }

        /// <summary>
        /// 获取实体的成员映射器
        /// </summary>
        /// <returns></returns>
        public EntityMemberMapper GetEntityMemberMapper()
        {
            if (this._mapper == null)
            {
                this._mapper = EntityMemberMapper.GetEntityMapperInstance(this.ConstructorInfo.DeclaringType);
            }
            return this._mapper;
        }
        
        /// <summary>
        /// 获取实体构造函数的描述实例
        /// </summary>
        /// <param name="constructorInfo">构造函数</param>
        /// <returns></returns>
        public static EntityConstructorDescriptor GetConstructorDescriptor(ConstructorInfo constructorInfo)
        {
            EntityConstructorDescriptor instance = null;
            if (!_constructorDescriptorsCache.TryGetValue(constructorInfo, out instance))
            {
                lock (constructorInfo)
                {
                    if (!_constructorDescriptorsCache.TryGetValue(constructorInfo, out instance))
                    {
                        instance = new EntityConstructorDescriptor(constructorInfo);
                        _constructorDescriptorsCache.GetOrAdd(constructorInfo, instance);
                    }
                }
            }
            return instance;
        }
    }
}
