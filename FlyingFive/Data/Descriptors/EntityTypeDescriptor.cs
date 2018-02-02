using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Infrastructure;
using FlyingFive.Data.Mapping;
using FlyingFive.Data.Schema;
using FlyingFive.Data.Visitors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Descriptors
{
    /// <summary>
    /// 表示实体类型描述
    /// </summary>
    public class EntityTypeDescriptor
    {
        private IDictionary<MemberInfo, DbColumnAccessExpression> _memberColumnMap = null;
        /// <summary>
        /// 所有映射的成员描述
        /// </summary>
        public IDictionary<MemberInfo, MappingMemberDescriptor> MappingMemberDescriptors { get; private set; }
        /// <summary>
        /// 映射为标识列的成员描述
        /// </summary>
        public MappingMemberDescriptor AutoIncrement { get; private set; }
        /// <summary>
        /// 实体类型
        /// </summary>
        public Type EntityType { get; private set; }
        /// <summary>
        /// 实体映射到的数据库表
        /// </summary>
        public DbTable Table { get; private set; }
        /// <summary>
        /// 实体的主键列表描述
        /// </summary>
        public ReadOnlyCollection<MappingMemberDescriptor> PrimaryKeys { get; private set; }
        /// <summary>
        /// 实体类型是否存在主键描述
        /// </summary>
        public bool HasPrimaryKeys { get { return PrimaryKeys != null && PrimaryKeys.Count > 0; } }

        public EntityTypeDescriptor(Type entityType)
        {
            this.EntityType = entityType;
            //this.PrimaryKeys = new ReadOnlyCollection<MappingMemberDescriptor>()
            InitTable();
            InitMemberInfo();
            InitMemberColumnMap();
        }

        private void InitTable()
        {
            Type entityType = this.EntityType;
            var tableFlag = entityType.GetCustomAttribute<TableAttribute>(false);

            if (tableFlag != null)
            {
                this.Table = new DbTable(tableFlag.Name, tableFlag.Schema);
                //tableFlag = new TableAttribute(entityType.Name);
            }
            else
            {
                var key = entityType.FullName;
                EntityMapping entityMapping = null;
                var flag = EntityMappingCollection.Mappings.TryGetValue(key, out entityMapping);
                if (!flag || entityMapping == null) { throw new InvalidOperationException("实体没有映射"); }
                this.Table = new DbTable(entityMapping.TableName, entityMapping.Schema);
            }
            //else if (tableFlag.Name == null)
            //    tableFlag.Name = entityType.Name;
        }

        private void InitMemberInfo()
        {
            var mappingMemberDescriptors = this.ExtractMappingMemberDescriptors();
            var primaryKeys = mappingMemberDescriptors.Where(a => a.Column.IsPrimaryKey).ToList();

            if (primaryKeys.Count == 0)
            {
                throw new InvalidOperationException(string.Format("没有为实体类型配置主键映射: {0}", this.EntityType.FullName));
            }

            this.PrimaryKeys = primaryKeys.AsReadOnly();

            var identityColumns = mappingMemberDescriptors.Where(m => m.Column.IsIdentity).ToList();
            //List<MappingMemberDescriptor> autoIncrementMemberDescriptors = mappingMemberDescriptors.Where(a => a.IsDefined(typeof(AutoIncrementAttribute))).ToList();
            if (identityColumns.Count > 1)
            {
                throw new NotSupportedException(string.Format("The entity type '{0}' can not define multiple auto increment members.", this.EntityType.FullName));
            }
            else //(autoIncrementMemberDescriptors.Count == 1)
            {
                var autoIncrementMemberDescriptor = identityColumns.FirstOrDefault();
                if (autoIncrementMemberDescriptor != null)
                {
                    if (!(autoIncrementMemberDescriptor.MemberInfoType == typeof(Int32) || autoIncrementMemberDescriptor.MemberInfoType == typeof(Int32)))
                    {
                        throw new DataAccessException("映射为自增列的成员类型只能为：Int32或Int64.");
                    }
                }
                //autoIncrementMemberDescriptor.IsAutoIncrement = true;
                this.AutoIncrement = autoIncrementMemberDescriptor;
            }
            //if (primaryKeys.Count == 1)
            //{
            //    /* 如果没有显示定义自增成员，并且主键只有 1 个，如果该主键满足一定条件，则默认其是自增列 */
            //    MappingMemberDescriptor primaryKeyDescriptor = primaryKeys[0];
            //    if (IsAutoIncrementType(primaryKeyDescriptor.MemberInfoType) && !primaryKeyDescriptor.IsDefined(typeof(NonAutoIncrementAttribute)))
            //    {
            //        primaryKeyDescriptor.IsAutoIncrement = true;
            //        this._autoIncrement = primaryKeyDescriptor;
            //    }
            //}

            this.MappingMemberDescriptors = new Dictionary<MemberInfo, MappingMemberDescriptor>(mappingMemberDescriptors.Count);
            foreach (MappingMemberDescriptor mappingMemberDescriptor in mappingMemberDescriptors)
            {
                this.MappingMemberDescriptors.Add(mappingMemberDescriptor.MemberInfo, mappingMemberDescriptor);
            }
        }

        private void InitMemberColumnMap()
        {
            Dictionary<MemberInfo, DbColumnAccessExpression> memberColumnMap = new Dictionary<MemberInfo, DbColumnAccessExpression>(this.MappingMemberDescriptors.Count);
            foreach (var kv in this.MappingMemberDescriptors)
            {
                memberColumnMap.Add(kv.Key, new DbColumnAccessExpression(this.Table, kv.Value.Column));
            }
            this._memberColumnMap = memberColumnMap;
        }

        /// <summary>
        /// 提取实体类型上可映射的成员描述
        /// </summary>
        /// <returns></returns>
        private List<MappingMemberDescriptor> ExtractMappingMemberDescriptors()
        {
            var members = this.EntityType.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            var mappingMemberDescriptors = new List<MappingMemberDescriptor>();
            foreach (var member in members)
            {
                if (!CanMap(member)) { continue; }
                if (SupportedMappingTypes.IsMappingType(member.GetMemberType()))
                {
                    var memberDescriptor = new MappingMemberDescriptor(member, this);
                    mappingMemberDescriptors.Add(memberDescriptor);
                }
            }
            return mappingMemberDescriptors;
        }

        private static bool CanMap(MemberInfo member)
        {
            var ignoreFlags = member.GetCustomAttribute<NotMappedAttribute>(false);
            if (ignoreFlags != null) { return false; }

            if (member.MemberType == MemberTypes.Property)
            {
                if (((PropertyInfo)member).GetSetMethod() == null) { return false; }//对于没有公共的 setter 直接跳过
                return true;
            }
            else if (member.MemberType == MemberTypes.Field)
            {
                return true;
            }
            else
            {
                return false;//只支持公共属性和字段
            }
        }

        private DefaultExpressionParser _expressionParser = null;
        public DefaultExpressionParser GetExpressionParser(DbTable explicitDbTable)
        {
            if (explicitDbTable == null)
            {
                if (this._expressionParser == null)
                    this._expressionParser = new DefaultExpressionParser(this, null);
                return this._expressionParser;
            }
            else
                return new DefaultExpressionParser(this, explicitDbTable);
        }
        public MappingMemberDescriptor TryGetMappingMemberDescriptor(MemberInfo memberInfo)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.EntityType);
            MappingMemberDescriptor memberDescriptor;
            this.MappingMemberDescriptors.TryGetValue(memberInfo, out memberDescriptor);
            return memberDescriptor;
        }
        public DbColumnAccessExpression TryGetColumnAccessExpression(MemberInfo memberInfo)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.EntityType);
            DbColumnAccessExpression dbColumnAccessExpression;
            this._memberColumnMap.TryGetValue(memberInfo, out dbColumnAccessExpression);
            return dbColumnAccessExpression;
        }


        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, EntityTypeDescriptor> InstanceCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, EntityTypeDescriptor>();

        /// <summary>
        /// 获取实体类型描述
        /// </summary>
        /// <param name="type">实体类型</param>
        /// <returns></returns>
        public static EntityTypeDescriptor GetEntityTypeDescriptor(Type type)
        {
            EntityTypeDescriptor instance = null;
            if (!InstanceCache.TryGetValue(type, out instance))
            {
                lock (type)
                {
                    if (!InstanceCache.TryGetValue(type, out instance))
                    {
                        instance = new EntityTypeDescriptor(type);
                        InstanceCache.GetOrAdd(type, instance);
                    }
                }
            }
            return instance;
        }
    }
}
