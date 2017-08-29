using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Schema
{
    /// <summary>
    /// 表示数据库中的表
    /// </summary>
    [DebuggerDisplay("Name = {Name}")]
    public class DbTable
    {
        /// <summary>
        /// 表名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 所属构架
        /// </summary>
        public string Schema { get; set; }
        /// <summary>
        /// 表描述信息
        /// </summary>
        public string Description { get; set; }

        public DbTable(string name) : this(name, null, null) { }
        public DbTable(string name, string schema) : this(name, schema, null) { }
        public DbTable(string name, string schema, string desc)
        {
            this.Name = name;
            this.Schema = schema;
            this.Description = desc;
        }
    }
}
