using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Mapping
{
    /// <summary>
    /// 表示实体类型到数据库的映射
    /// </summary>
    public class EntityMapping
    {
        /// <summary>
        /// 映射DB构架
        /// </summary>
        public string Schema { get; internal set; }
        /// <summary>
        /// 映射的表名
        /// </summary>
        public string TableName { get; internal set; }
        /// <summary>
        /// 映射的实体类型
        /// </summary>
        public Type EntityType { get; internal set; }
        /// <summary>
        /// 成员映射集合
        /// </summary>
        public IList<MemberMapping> MemberMappings { get; set; }


        public EntityMapping()
        {
            Schema = "dbo";
            MemberMappings = new List<MemberMapping>();
        }

        /// <summary>
        /// 验证实体映射是否正确
        /// </summary>
        public void FinalPass()
        {
            if (string.IsNullOrWhiteSpace(TableName)) { throw new InvalidOperationException("实体映射配置无效:不存在表名称!"); }
            if (MemberMappings.Count <= 0) { throw new InvalidOperationException("实体映射配置无效: 不存在任何属性映射!"); }
            if (!MemberMappings.Any(p => p.IsPrimaryKey)) { throw new InvalidOperationException("实体映射配置无效: 实体属性映射中不存在任何主键信息!"); }
            foreach (var property in MemberMappings)
            {
                property.FinalPass();
            }
        }
    }
}
