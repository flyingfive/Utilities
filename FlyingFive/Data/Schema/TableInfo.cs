using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Schema
{
    /// <summary>
    /// 表信息
    /// </summary>
    public class TableInfo
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public int TableId { get; set; }
        /// <summary>
        /// 是否为视图
        /// </summary>
        public bool IsView { get; set; }
        /// <summary>
        /// 表描述信息
        /// </summary>
        public string TableDescription { get; set; }
        /// <summary>
        /// 列信息集合
        /// </summary>
        public List<ColumnInfo> Columns { get; set; }

        public TableInfo() { Columns = new List<ColumnInfo>(); }
    }
}
