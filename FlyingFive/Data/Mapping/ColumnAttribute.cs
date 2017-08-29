using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Mapping
{
    /// <summary>
    /// 映射表标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {
        public TableAttribute() { }
        public TableAttribute(string name)
        {
            this.Name = name;
        }
        public string Name { get; set; }
        public string Schema { get; set; }
    }

    /// <summary>
    /// 映射列标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ColumnAttribute : System.Attribute
    {
        public string Name { get; set; }
        public bool IsPrimaryKey { get; set; }
        public DbType DbType { get; set; }
        public int Size { get; set; }

        public ColumnAttribute(string name, bool isPrimaryKey, DbType dbType, int size)
        {
            this.Name = name;
            this.IsPrimaryKey = isPrimaryKey;
            this.DbType = dbType;
            this.Size = size;
        }
    }

    /// <summary>
    /// 自增标识列标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class AutoIncrementAttribute : Attribute
    {
    }

    /// <summary>
    /// 非映射标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NotMappedAttribute : Attribute
    {
    }
}
