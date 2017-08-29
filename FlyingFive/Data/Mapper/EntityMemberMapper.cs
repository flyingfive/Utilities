using FlyingFive.Data.Emit;
using FlyingFive.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Mapper
{
    /// <summary>
    /// 实体成员映射
    /// </summary>
    public class EntityMemberMapper
    {
        private Dictionary<MemberInfo, IMemberMapper> _mappingMemberMappers = null;
        private Dictionary<MemberInfo, Action<object, object>> _complexMemberSetters = null;
        /// <summary>
        /// 实体类型
        /// </summary>
        public Type Type { get; private set; }

        public EntityMemberMapper(Type type)
        {
            this._mappingMemberMappers = new Dictionary<MemberInfo, IMemberMapper>();
            this._complexMemberSetters = new Dictionary<MemberInfo, Action<object, object>>();
            this.Type = type;
            Init();
        }

        private void Init()
        {
            Type t = this.Type;
            var members = t.GetMembers(BindingFlags.Public | BindingFlags.Instance);

            var mappingMemberMappers = new Dictionary<MemberInfo, IMemberMapper>();
            var complexMemberSetters = new Dictionary<MemberInfo, Action<object, object>>();

            foreach (var member in members)
            {
                Type memberType = null;
                PropertyInfo prop = null;
                FieldInfo field = null;

                if ((prop = member as PropertyInfo) != null)
                {
                    if (prop.GetSetMethod() == null)
                    {
                        continue;
                    }
                    memberType = prop.PropertyType;
                }
                else if ((field = member as FieldInfo) != null)
                {
                    memberType = field.FieldType;
                }
                else
                {
                    continue;
                }
                if (SupportedMappingTypes.IsMappingType(memberType))
                {
                    var mrm = MemberMapperHelper.CreateMemberMapper(member);
                    mappingMemberMappers.Add(member, mrm);
                }
                else
                {
                    if (prop != null)
                    {
                        Action<object, object> valueSetter = DelegateGenerator.CreateValueSetter(prop);
                        complexMemberSetters.Add(member, valueSetter);
                    }
                    else if (field != null)
                    {
                        Action<object, object> valueSetter = DelegateGenerator.CreateValueSetter(field);
                        complexMemberSetters.Add(member, valueSetter);
                    }
                    else
                    {
                        continue;
                    }
                    continue;
                }
            }
            foreach (var item in mappingMemberMappers.Keys)
            {
                _mappingMemberMappers.Add(item, mappingMemberMappers[item]);
            }
            foreach (var item in complexMemberSetters.Keys)
            {
                _complexMemberSetters.Add(item, complexMemberSetters[item]);
            }
        }

        /// <summary>
        /// 尝试获取映射成员的映射器
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public IMemberMapper TryGetMappingMemberMapper(MemberInfo memberInfo)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.Type);
            IMemberMapper mapper = null;
            this._mappingMemberMappers.TryGetValue(memberInfo, out mapper);
            return mapper;
        }

        /// <summary>
        /// 尝试获取复杂成员的setter访问器
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public Action<object, object> TryGetComplexMemberSetter(MemberInfo memberInfo)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.Type);
            Action<object, object> valueSetter = null;
            this._complexMemberSetters.TryGetValue(memberInfo, out valueSetter);
            return valueSetter;
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, EntityMemberMapper> _entityMapperCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, EntityMemberMapper>();


        /// <summary>
        /// 获取实体类型映射实例
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static EntityMemberMapper GetEntityMapperInstance(Type type)
        {
            EntityMemberMapper instance = null;
            if (!_entityMapperCache.TryGetValue(type, out instance))
            {
                lock (type)
                {
                    if (!_entityMapperCache.TryGetValue(type, out instance))
                    {
                        instance = new EntityMemberMapper(type);
                        _entityMapperCache.GetOrAdd(type, instance);
                    }
                }
            }
            return instance;
        }
    }
}
