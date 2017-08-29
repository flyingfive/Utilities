using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Mapping
{
    /// <summary>
    /// 表示实体映射
    /// </summary>
    [Serializable]
    public class EntityMapping
    {
        /// <summary>
        /// 映射的表名
        /// </summary>
        public string TableName { get; set; }

        public string Schema { get; set; }
        /// <summary>
        /// 映射的实体类型
        /// </summary>
        public Type EntityType { get; set; }
        /// <summary>
        /// 属性映射集合
        /// </summary>
        public List<PropertyMapping> PropertyMappings { get; set; }

        public EntityMapping()
        {
            this.PropertyMappings = new List<PropertyMapping>();
        }

        internal void FinalPass()
        {

        }
    }

    /// <summary>
    /// 实体属性映射
    /// </summary>
    [Serializable]
    public class PropertyMapping
    {
        public EntityMapping EntityMapping { get; set; }
        /// <summary>
        /// 映射的属性名称
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 对应的数据库列名称
        /// </summary>
        public string ColumnName { get; private set; }
        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimaryKey { get; private set; }
        /// <summary>
        /// 是否标识
        /// </summary>
        public bool IsIdentity { get; private set; }
        /// <summary>
        /// 字节数
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// (小数)数据精度
        /// </summary>
        public int Precision { get; private set; }
        /// <summary>
        /// 数值类型总长度
        /// </summary>
        public int Scale { get; private set; }
        /// <summary>
        /// 是否可空
        /// </summary>
        public bool IsNullable { get; private set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DbType DbType { get; private set; }

        public PropertyMapping HasColumnName(string columnName)
        {
            this.ColumnName = columnName;
            return this;
        }

        public PropertyMapping HasPrimaryKey()
        {
            this.IsPrimaryKey = true;
            return this;
        }

        public PropertyMapping HasIdentity()
        {
            this.IsIdentity = true;
            return this;
        }

        public PropertyMapping HasRequired(bool required)
        {
            this.IsNullable = !required;
            return this;
        }

        public PropertyMapping HasMaxSize(int maxSize)
        {
            this.Size = maxSize;
            return this;
        }

        public PropertyMapping HasPrecision(int precision)
        {
            this.Precision = precision;
            return this;
        }

        public PropertyMapping HasScale(int scale)
        {
            this.Scale = scale;
            return this;
        }

        public PropertyMapping HasDbType(DbType type)
        {
            this.DbType = type;
            return this;
        }

        public void AddToEntityMapping()
        {
            var exists = this.EntityMapping.PropertyMappings.Where(p => p.PropertyName.Equals(this.PropertyName)).SingleOrDefault();
            if (exists != null)
            {
                this.EntityMapping.PropertyMappings.Remove(exists);
            }
            this.EntityMapping.PropertyMappings.Add(this);
            this.EntityMapping.PropertyMappings.TrimExcess();
        }

        internal void FinalPass()
        {

        }
    }
}
