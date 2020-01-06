using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Schema
{
    /// <summary>
    /// 表字段信息
    /// </summary>
    public class ColumnInfo
    {
        /// <summary>
        /// 表ID
        /// </summary>
        public int TableId { get; set; }
        /// <summary>
        /// 列顺序
        /// </summary>
        public short ColumnOrder { get; set; }
        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 是否标识
        /// </summary>
        public bool IsIdentity { get; set; }
        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        /// <summary>
        /// SQL数据类型
        /// </summary>
        public string SqlType { get; set; }
        /// <summary>
        /// 大小
        /// </summary>
        public short Size { get; set; }
        //长度
        public int Precision { get; set; }
        /// <summary>
        /// 精度
        /// </summary>
        public int Scale { get; set; }
        /// <summary>
        /// 是否可空
        /// </summary>
        public bool IsNullable { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// 列描述
        /// </summary>
        public string ColumnDescription { get; set; }
        /// <summary>
        /// SQL用户自定义类型名称
        /// </summary>
        public string UserTypeName { get; set; }
        /// <summary>
        /// 是否用户自定义类型
        /// </summary>
        public bool IsUserType { get; set; }
    }
}
