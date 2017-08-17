using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDBAssistant.Schema
{
    [Serializable]
    public class DatabaseObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Xtype { get; set; }
        public int ParentId { get; set; }
        public DatabaseObjectType ObjectType { get; set; }

        public const String QUERY_SQL = @"SELECT  id AS Id,
                    name AS [Name],
                    xtype AS Xtype,
                    parent_obj AS ParentId ,
                    CASE xtype
                      WHEN 'V' THEN '视图'
                      WHEN 'P' THEN '存储过程'
                      WHEN 'FN' THEN '用户函数'
                      WHEN 'TR' THEN '触发器'
                    END AS ObjectType
            FROM    sysobjects
            WHERE   xtype IN ( 'V', 'P', 'tr', 'fn' ) AND [status] >= 0";
    }

    [Serializable]
    public enum DatabaseObjectType : int
    {
        数据表 = 1,
        视图 = 2,
        存储过程 = 3,
        用户函数 = 4,
        触发器 = 5
    }
}
