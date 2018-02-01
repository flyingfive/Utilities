using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FlyingFive.Data.Mapping
{
    /// <summary>
    /// 实体成员到数据库的映射
    /// </summary>
    public class MemberMapping
    {
        /// <summary>
        /// 映射的属性名称
        /// </summary>
        public string PropertyName { get; internal set; }
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
        /// 是否可空
        /// </summary>
        public bool IsNullable { get; private set; }
        /// <summary>
        /// DB列的存储字节数
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// 成员映射表达式
        /// </summary>
        public MemberExpression MappingExpression { get; internal set; }
        /// <summary>
        /// 参与映射的成员
        /// </summary>
        public MemberInfo MappingMember { get { return MappingExpression == null ? null : MappingExpression.Member; } }

        public EntityMapping EntityMapping { get; set; }

        public MemberMapping()
        {
            IsNullable = true;
        }

        /// <summary>
        /// 设置属性映射的列名
        /// </summary>
        /// <param name="columnName">DB列名称</param>
        /// <returns></returns>
        public MemberMapping HasColumnName(string columnName)
        {
            this.ColumnName = columnName;
            return this;
        }

        /// <summary>
        /// 设置属性是否映射为主键
        /// </summary>
        /// <returns></returns>
        public MemberMapping HasPrimaryKey()
        {
            this.IsPrimaryKey = true;
            return this;
        }

        /// <summary>
        /// 设置属性是否映射为DB标识列
        /// </summary>
        /// <returns></returns>
        public MemberMapping HasIdentity()
        {
            this.IsIdentity = true;
            return this;
        }

        /// <summary>
        /// 设置属性是否映射为必填项
        /// </summary>
        /// <param name="required">是否必填</param>
        /// <returns></returns>
        public MemberMapping HasRequired(bool required)
        {
            this.IsNullable = !required;
            return this;
        }
        /// <summary>
        /// 设置属性映射到DB的最大存储字节数
        /// </summary>
        /// <param name="maxSize">最大存储字节数</param>
        /// <returns></returns>
        public MemberMapping HasMaxLength(int maxSize)
        {
            //this.MaxLength = maxSize;
            this.Size = maxSize;
            return this;
        }

        /// <summary>
        /// 验证属性映射
        /// </summary>
        public void FinalPass()
        {
            if (string.IsNullOrWhiteSpace(PropertyName)) { throw new InvalidOperationException("配置无效:找不到实体属性名称!"); }
            if (string.IsNullOrWhiteSpace(ColumnName)) { throw new InvalidOperationException("配置无效:实体属性没有映射对应列名!"); }
        }
    }
}
