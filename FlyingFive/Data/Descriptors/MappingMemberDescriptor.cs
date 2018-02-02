using FlyingFive.Data.Emit;
using FlyingFive.Data.Mapper;
using FlyingFive.Data.Mapping;
using FlyingFive.Data.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace FlyingFive.Data.Descriptors
{
    /// <summary>
    /// 表示映射实体成员的描述
    /// </summary>
    public class MappingMemberDescriptor : EntityMemberDescriptor
    {
        private Func<object, object> _valueGetter = null;
        private Action<object, object> _valueSetter = null;
        /// <summary>
        /// 该实体映射遇到的的DB列构架信息
        /// </summary>
        public DbColumn Column { get; private set; }
        /// <summary>
        /// 该成员的映射信息
        /// </summary>
        public PropertyMapping Mapping { get; private set; }

        public MappingMemberDescriptor(MemberInfo memberInfo, EntityTypeDescriptor entityTypeDescriptor)
            : base(memberInfo, entityTypeDescriptor)
        {
            var columnFlag = this.MemberInfo.GetCustomAttribute<ColumnAttribute>(false);
            if (columnFlag != null)
            {
                this.Column = new DbColumn(columnFlag.Name, this.MemberInfoType, columnFlag.DbType, columnFlag.Size);
                this.Column.IsPrimaryKey = columnFlag.IsPrimaryKey;
                this.Column.IsIdentity = MemberInfo.GetCustomAttribute<AutoIncrementAttribute>(false) != null;
            }
            else
            {
                var key = MemberInfo.DeclaringType.FullName;
                EntityMapping entityMapping = null;
                var flag = EntityMappingCollection.Mappings.TryGetValue(key, out entityMapping);
                if (!flag || entityMapping == null) { throw new InvalidOperationException("实体没有映射"); }
                var propMapping = entityMapping.PropertyMappings.Where(p => p.PropertyName.Equals(memberInfo.Name)).SingleOrDefault();
                if (propMapping == null) { throw new InvalidOperationException("实体没有映射"); }
                this.Mapping = propMapping;
                this.Column = new DbColumn(propMapping.ColumnName, this.MemberInfoType, propMapping.DbType, propMapping.Size);
                this.Column.IsPrimaryKey = propMapping.IsPrimaryKey;
                this.Column.IsIdentity = propMapping.IsIdentity;
                this.Column.IsNullable = propMapping.IsNullable;
            }
        }

        /// <summary>
        /// 从实体对象实例上获取映射成员的值
        /// </summary>
        /// <param name="instance">实体对象</param>
        /// <returns></returns>
        public object GetValue(object instance)
        {
            if (instance == null) { throw new ArgumentException("必需提供参数：instance"); }
            object retValue = null;
            if (null == this._valueGetter)
            {
                if (Monitor.TryEnter(this))
                {
                    try
                    {
                        if (null == this._valueGetter) { this._valueGetter = DelegateGenerator.CreateValueGetter(this.MemberInfo); }
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                }
                else
                {
                    retValue = this.MemberInfo.GetMemberValue(instance);
                    return retValue;
                }
            }
            retValue = this._valueGetter(instance);
            return retValue;
        }

        /// <summary>
        /// 为实体对象的此映射成员赋值
        /// </summary>
        /// <param name="instance">实体对象</param>
        /// <param name="value">要更新的值</param>
        public void SetValue(object instance, object value)
        {
            if (instance == null) { throw new ArgumentException("必需提供参数：instance"); }
            if (null == this._valueSetter)
            {
                if (Monitor.TryEnter(this))
                {
                    try
                    {
                        if (null == this._valueSetter) { this._valueSetter = DelegateGenerator.CreateValueSetter(this.MemberInfo); }
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                }
                else
                {
                    this.MemberInfo.SetMemberValue(instance, value);
                    return;
                }
            }
            this._valueSetter(instance, value);
        }
    }
}
