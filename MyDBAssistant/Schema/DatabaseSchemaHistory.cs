using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using MyDBAssistant.Data;

namespace MyDBAssistant.Schema
{
    public class DatabaseSchemaHistory
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string DeveloperId { get; set; }
        public DateTime RecordDate { get; set; }
        public string ClientIp { get; set; }
        public string SqlScript { get; set; }
        public string Remark { get; set; }

        public bool Insert(MsSqlHelper helper)
        {
            bool hasLogTable = Convert.ToInt32(helper.ExecuteQueryAsSingle("SELECT ISNULL(COUNT(0), 0) FROM sysobjects WHERE name= 'sys_DatabaseSchemaHistory'", CommandType.Text)) > 0;
            if (!hasLogTable)
            {
                helper.ExecuteNonQuery(CREATE_SCHEMA_TABLE_SQL, CommandType.Text);
            }
            var address = System.Net.Dns.GetHostAddresses("localhost").FirstOrDefault();
            var ip = address == null ? "127.0.0.1" : address.ToString();
            string insertLogSql = @"INSERT INTO [dbo].[sys_DatabaseSchemaHistory]
               ([Id]
               ,[Description]
               ,[DeveloperId]
               ,[RecordDate]
               ,[ClientIp]
               ,[SqlScript]
               ,[Remark])
         VALUES
               (@Id,
                @Description,
                @DeveloperId,
                @RecordDate, 
                @ClientIp, 
                @SqlScript,
                @Remark)";
            DbParameter[] paras = new DbParameter[] { 
                new SqlParameter("@Id", SequentialGUID(Guid.NewGuid())),
                new SqlParameter("@Description", Description),
                new SqlParameter("@DeveloperId", "admin"),
                new SqlParameter("@RecordDate", DateTime.Now),
                new SqlParameter("@ClientIp", ip),
                new SqlParameter("@SqlScript", SqlScript),
                new SqlParameter("@Remark", DBNull.Value)
            };
            int count = helper.ExecuteNonQuery(insertLogSql, CommandType.Text, paras);
            return count > 0;
        }

        private Guid SequentialGUID(Guid guid)
        {
            if (guid == Guid.Empty) { guid = Guid.NewGuid(); }
            byte[] guidArray = guid.ToByteArray();
            var baseDate = new DateTime(1900, 1, 1);
            DateTime now = DateTime.Now;
            var days = new TimeSpan(now.Ticks - baseDate.Ticks);
            TimeSpan msecs = now.TimeOfDay;
            byte[] daysArray = BitConverter.GetBytes(days.Days);
            byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);
            Guid newId = new Guid(guidArray);
            return newId;
        }

        /*
         SELECT * FROM systypes WHERE xtype = (SELECT xtype FROM systypes WHERE name = 'u_trans_no') AND uid <>1
             SELECT uid, * from systypes where xtype<>xusertype
             select * from sys.types where is_user_defined=1
         */
        public const String CREATE_SCHEMA_TABLE_SQL = @"
            CREATE TABLE sys_DatabaseSchemaHistory
            (
		        Id UNIQUEIDENTIFIER PRIMARY KEY,
		        [Description] VARCHAR(100) NOT NULL,
		        DeveloperId VARCHAR(40) NOT NULL,
		        RecordDate DATETIME NOT NULL,
		        ClientIp VARCHAR(20),
		        SqlScript TEXT,
		        Remark VARCHAR(200)
            )";

        //        public const String CREATE_DATA_DICT_VIEW_SQL2000 = @"
        //CREATE VIEW v_sys_DataDict
        //AS
        //    SELECT  d.name AS TableName ,
        //            d.id AS TableId ,
        //            CASE d.type
        //              WHEN 'U' THEN 0
        //              ELSE 1
        //            END AS IsView ,
        //            ISNULL(f.value, '') AS TableDescription ,
        //            a.colorder AS [ColumnOrder] ,
        //            a.name AS ColumnName ,
        //            CASE WHEN COLUMNPROPERTY(a.id, a.name, 'IsIdentity') = 1 THEN 1
        //                 ELSE 0
        //            END AS IsIdentity ,
        //            CASE WHEN EXISTS ( SELECT   1
        //                               FROM     sysobjects
        //                               WHERE    xtype = 'PK'
        //                                        AND parent_obj = a.id
        //                                        AND name IN (
        //                                        SELECT  name
        //                                        FROM    sysindexes
        //                                        WHERE   indid IN (
        //                                                SELECT  indid
        //                                                FROM    sysindexkeys
        //                                                WHERE   id = a.id
        //                                                        AND colid = a.colid ) ) )
        //                 THEN 1
        //                 ELSE 0
        //            END AS IsPrimaryKey ,
        //            --LOWER(b.name) AS SqlType ,
        //            CASE WHEN b.xtype = b.xusertype THEN 0 ELSE 1 END AS IsUserType ,
        //            b.name AS UserTypeName ,
        //            LOWER(CASE WHEN b.xtype = b.xusertype THEN  LOWER(b.name) ELSE b2.name END) AS SqlType ,
        //            a.length AS Size ,
        //            COLUMNPROPERTY(a.id, a.name, 'PRECISION') AS [Precision] ,
        //            ISNULL(COLUMNPROPERTY(a.id, a.name, 'Scale'), 0) AS Scale ,
        //            CASE WHEN a.isnullable = 1 THEN 1
        //                 ELSE 0
        //            END AS IsNullable ,
        //            ISNULL(e.[text], '') AS DefaultValue ,
        //            ISNULL(g.[value], '') AS ColumnDescription
        //    FROM    syscolumns a
        //            LEFT JOIN systypes b ON a.xusertype = b.xusertype
        //            LEFT JOIN (SELECT * FROM systypes WHERE xtype IN(SELECT xtype FROM systypes WHERE xtype <> xusertype) AND xtype = dbo.systypes.xusertype) b2 ON b.xtype = b2.xtype
        //            INNER JOIN sysobjects d ON a.id = d.id
        //                                       AND d.xtype IN ( 'U', 'V' )
        //                                       AND d.name NOT IN( 'dtproperties', 'sysconstraints', 'syssegments')
        //            LEFT JOIN syscomments e ON a.cdefault = e.id
        //            LEFT JOIN sysproperties g ON a.id = g.id
        //                                         AND a.colid = g.smallid
        //                                         AND g.name = 'MS_Description'
        //            LEFT JOIN sysproperties f ON d.id = f.id
        //                                         AND f.smallid = 0
        //                                         AND f.name = 'MS_Description'";

        //        public const String CREATE_DATA_DICT_VIEW_SQL2005 = @"
        //CREATE VIEW v_sys_DataDict
        //AS
        //    SELECT  d.name AS TableName ,
        //            d.id AS TableId ,
        //            CASE d.type
        //              WHEN 'U' THEN 0
        //              ELSE 1
        //            END AS IsView ,
        //            ISNULL(f.value, '') AS TableDescription ,
        //            a.colorder AS [ColumnOrder] ,
        //            a.name AS ColumnName ,
        //            CASE WHEN COLUMNPROPERTY(a.id, a.name, 'IsIdentity') = 1 THEN 1
        //                 ELSE 0
        //            END AS IsIdentity ,
        //            CASE WHEN EXISTS ( SELECT   1
        //                               FROM     sysobjects
        //                               WHERE    xtype = 'PK'
        //                                        AND parent_obj = a.id
        //                                        AND name IN (
        //                                        SELECT  name
        //                                        FROM    sysindexes
        //                                        WHERE   indid IN (
        //                                                SELECT  indid
        //                                                FROM    sysindexkeys
        //                                                WHERE   id = a.id
        //                                                        AND colid = a.colid ) ) )
        //                 THEN 1
        //                 ELSE 0
        //            END AS IsPrimaryKey ,
        //            --LOWER(b.name) AS SqlType ,
        //            CASE WHEN b.xtype = b.xusertype THEN 0 ELSE 1 END AS IsUserType ,
        //            b.name AS UserTypeName ,
        //            LOWER(CASE WHEN b.xtype = b.xusertype THEN b.NAME ELSE b2.NAME END) AS SqlType ,
        //            a.length AS Size ,
        //            COLUMNPROPERTY(a.id, a.name, 'PRECISION') AS [Precision] ,
        //            ISNULL(COLUMNPROPERTY(a.id, a.name, 'Scale'), 0) AS Scale ,
        //            CASE WHEN a.isnullable = 1 THEN 1
        //                 ELSE 0
        //            END AS IsNullable ,
        //            ISNULL(e.[text], '') AS DefaultValue ,
        //            ISNULL(g.[value], '') AS ColumnDescription
        //    FROM    syscolumns a
        //            LEFT JOIN systypes b ON a.xusertype = b.xusertype
        //            LEFT JOIN sys.types b2 ON b.xtype = b2.system_type_id AND b2.is_user_defined = 0 AND b2.name <> 'sysname'
        //            INNER JOIN sysobjects d ON a.id = d.id
        //                                       AND d.xtype IN ( 'U', 'V' )
        //                                       AND d.name NOT IN( 'dtproperties', 'sysconstraints', 'syssegments')
        //            LEFT JOIN syscomments e ON a.cdefault = e.id
        //            LEFT JOIN sys.extended_properties g ON a.id = g.major_id
        //                                                   AND a.colid = g.minor_id
        //                                                   AND g.name = 'MS_Description'
        //            LEFT JOIN sys.extended_properties f ON d.id = f.major_id
        //                                                   AND f.minor_id = 0
        //                                                   AND f.name = 'MS_Description'";
    }
}
