using FlyingFive.Data.DbExpressions;
using FlyingFive.Data.Infrastructure;
using FlyingFive.Data.Mapping;
using FlyingFive.Data.Schema;
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
        private IDictionary<MemberInfo, MappingMemberDescriptor> _mappingMemberDescriptors;
        private IDictionary<MemberInfo, DbColumnAccessExpression> _memberColumnMap;
        //private ReadOnlyCollection<MappingMemberDescriptor> _primaryKeys;
        private MappingMemberDescriptor _autoIncrement = null;
        public Type EntityType { get; private set; }
        public DbTable Table { get; private set; }
        /// <summary>
        /// 实体的主键列表
        /// </summary>
        public ReadOnlyCollection<MappingMemberDescriptor> PrimaryKeys { get; private set; }

        public EntityTypeDescriptor(Type entityType)
        {
            this.EntityType = entityType;
            InitTable();
            InitMemberInfo();
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
                var autoIncrementMemberDescriptor = identityColumns.First();
                if (!(autoIncrementMemberDescriptor.MemberInfoType == typeof(Int32) || autoIncrementMemberDescriptor.MemberInfoType == typeof(Int32)))
                {
                    throw new DataAccessException("映射为自增列的成员类型只能为：Int32或Int64.");
                }
                //autoIncrementMemberDescriptor.IsAutoIncrement = true;
                this._autoIncrement = autoIncrementMemberDescriptor;
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

            this._mappingMemberDescriptors = new Dictionary<MemberInfo, MappingMemberDescriptor>(mappingMemberDescriptors.Count);
            foreach (MappingMemberDescriptor mappingMemberDescriptor in mappingMemberDescriptors)
            {
                this._mappingMemberDescriptors.Add(mappingMemberDescriptor.MemberInfo, mappingMemberDescriptor);
            }
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
    }
}
