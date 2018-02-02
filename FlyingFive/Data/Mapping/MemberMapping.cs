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
        public List<MemberMapping> PropertyMappings { get; set; }

        public EntityMapping()
        {
            Schema = "dbo";
            this.PropertyMappings = new List<MemberMapping>();
        }

        internal void FinalPass()
        {
            if (string.IsNullOrWhiteSpace(TableName)) { throw new InvalidOperationException("实体映射配置无效:不存在表名称!"); }
            if (PropertyMappings.Count <= 0) { throw new InvalidOperationException("实体映射配置无效: 不存在任何属性映射!"); }
            if (!PropertyMappings.Any(p => p.IsPrimaryKey)) { throw new InvalidOperationException("实体映射配置无效: 实体属性映射中不存在任何主键信息!"); }
            foreach (var property in PropertyMappings)
            {
                property.FinalPass();
            }
        }
    }

    /// <summary>
    /// 实体成员映射
    /// </summary>
    [Serializable]
    public class MemberMapping
    {
        public EntityMapping EntityMapping { get; set; }
        /// <summary>
        /// 映射的属性名称
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 对应的数据库列名称
        /// </summary>
        public string ColumnName { get; internal set; }
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

        public MemberMapping HasColumnName(string columnName)
        {
            this.ColumnName = columnName;
            return this;
        }

        /// <summary>
        /// 将映射属性设置为主键
        /// </summary>
        /// <returns></returns>
        public MemberMapping HasPrimaryKey()
        {
            this.IsPrimaryKey = true;
            return this;
        }

        /// <summary>
        /// 将映射属性设置为标识列
        /// </summary>
        /// <returns></returns>
        public MemberMapping HasIdentity()
        {
            this.IsIdentity = true;
            return this;
        }
        /// <summary>
        /// 将映射属性设置为必填字段
        /// </summary>
        /// <param name="required">是否必填</param>
        /// <returns></returns>
        public MemberMapping HasRequired(bool required)
        {
            this.IsNullable = !required;
            return this;
        }
        /// <summary>
        /// 为映射属性设置为最大存储字节数
        /// </summary>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        public MemberMapping HasMaxSize(int maxSize)
        {
            this.Size = maxSize;
            return this;
        }
        /// <summary>
        /// 为映射属性设置数字精度与长度
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public MemberMapping HasPrecision(int precision, int scale)
        {
            this.Precision = precision;
            this.Scale = scale;
            return this;
        }

        public MemberMapping HasDbType(DbType type)
        {
            this.DbType = type;
            return this;
        }

        internal void FinalPass()
        {
            if (string.IsNullOrWhiteSpace(PropertyName)) { throw new InvalidOperationException("配置无效:找不到实体属性名称!"); }
            if (string.IsNullOrWhiteSpace(ColumnName)) { throw new InvalidOperationException("配置无效:实体属性没有映射对应列名!"); }
        }
    }
}
