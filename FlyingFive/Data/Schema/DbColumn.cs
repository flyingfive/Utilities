using FlyingFive.Data.DbExpressions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Schema
{
    /// <summary>
    /// 表示数据库构架中的列信息
    /// </summary>
    [DebuggerDisplay("Name = {Name}")]
    public class DbColumn
    {
        /// <summary>
        /// 列名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 对应的C#数据类型
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DbType? DbType { get; set; }
        /// <summary>
        /// DB中占用的字节大小
        /// </summary>
        public int? Size { get; set; }

        #region 扩展信息

        /// <summary>
        /// 是否标识列
        /// </summary>
        public bool IsIdentity { get; set; }
        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        /// <summary>
        /// sql数据类型
        /// </summary>
        public string SqlType { get; set; }
        /// <summary>
        /// (小数)数据精度
        /// </summary>
        public int Precision { get; set; }
        /// <summary>
        /// 数值类型总长度
        /// </summary>
        public int Scale { get; set; }
        /// <summary>
        /// 是否可空
        /// </summary>
        public bool IsNullable { get; set; }
        /// <summary>
        /// 默认值绑定
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// 列描述信息
        /// </summary>
        public string Description { get; set; }

        #endregion

        public DbColumn(string name, Type csharpType) : this(name, csharpType, null, null) { }

        public DbColumn(string name, Type csharpType, DbType? dbType, int? size)
        {
            this.Name = name;
            this.Type = csharpType;
            this.DbType = dbType;
            this.Size = size;
        }

        public static DbColumn MakeColumn(DbExpression exp, string alias)
        {
            DbColumn column;
            DbColumnAccessExpression e = exp as DbColumnAccessExpression;
            if (e != null)
                column = new DbColumn(alias, e.Column.Type, e.Column.DbType, e.Column.Size);
            else
                column = new DbColumn(alias, exp.Type);

            return column;
        }
    }
}
