using FlyingFive.Data.Infrastructure;
using FlyingFive.Data.Kernel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    /// <summary>
    /// SQL Server操作工具
    /// </summary>
    public class MsSqlHelper : DatabaseHelper, IDatabaseHelper
    {
        public MsSqlHelper(string connectionString)
            : this(new SqlServerDbConnectionFactory(connectionString))
        {
        }

        public MsSqlHelper(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
        {
            //this.PagingMode = PagingMode.ROW_NUMBER;
            //this._dbContextProvider = new MsSqlContextServiceProvider(dbConnectionFactory, this);
        }

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="data">数据表</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="batchSize">分批大小</param>
        /// <param name="options">插入选项</param>
        /// <returns></returns>
        public bool BulkCopy(DataTable data, int timeout = 60, int batchSize = 10000, SqlBulkCopyOptions options = SqlBulkCopyOptions.Default)
        {
            if (data == null) { UtilExceptions.CheckNull(data); }
            if (data.Rows.Count == 0) { return true; }
            if (timeout <= 0) { timeout = 60; }                //默认1分钟
            if (batchSize <= 0) { batchSize = data.Rows.Count; }
            using (var connection = (SqlConnection)this.DbConnectionFactory.CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                using (var copy = new SqlBulkCopy(connection, options, transaction))
                {
                    copy.BatchSize = batchSize;
                    copy.BulkCopyTimeout = timeout;
                    copy.DestinationTableName = data.TableName;
                    foreach (DataColumn col in data.Columns)
                    {
                        copy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    }
                    try
                    {
                        copy.WriteToServer(data);
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }
        #region create schema info view script
        /// <summary>
        /// 创建2000版本数据字典的视图sql脚本
        /// </summary>
        public const String CREATE_DATA_DICT_VIEW_SQL2000 = @"
        CREATE VIEW v_sys_DataDict
        AS
            SELECT  d.name AS TableName ,
                    d.id AS TableId ,
                    CAST(CASE d.type
                      WHEN 'U' THEN 0
                      ELSE 1
                    END AS BIT) AS IsView ,
                    ISNULL(f.value, '') AS TableDescription ,
                    a.colorder AS [ColumnOrder] ,
                    a.name AS ColumnName ,
                    CAST(CASE WHEN COLUMNPROPERTY(a.id, a.name, 'IsIdentity') = 1 THEN 1
                         ELSE 0
                    END AS BIT) AS IsIdentity ,
                    CAST(CASE WHEN EXISTS ( SELECT   1
                                       FROM     sysobjects
                                       WHERE    xtype = 'PK'
                                                AND parent_obj = a.id
                                                AND name IN (
                                                SELECT  name
                                                FROM    sysindexes
                                                WHERE   indid IN (
                                                        SELECT  indid
                                                        FROM    sysindexkeys
                                                        WHERE   id = a.id
                                                                AND colid = a.colid ) ) )
                         THEN 1
                         ELSE 0
                    END AS BIT) AS IsPrimaryKey ,
                    --LOWER(b.name) AS SqlType ,
                    CASE WHEN b.xtype = b.xusertype THEN 0 ELSE 1 END AS IsUserType ,
                    b.name AS UserTypeName ,
                    LOWER(CASE WHEN b.xtype = b.xusertype THEN  LOWER(b.name) ELSE b2.name END) AS SqlType ,
                    a.length AS Size ,
                    COLUMNPROPERTY(a.id, a.name, 'PRECISION') AS [Precision] ,
                    ISNULL(COLUMNPROPERTY(a.id, a.name, 'Scale'), 0) AS Scale ,
                    CAST(CASE WHEN a.isnullable = 1 THEN 1
                         ELSE 0
                    END AS BIT) AS IsNullable ,
                    ISNULL(e.[text], '') AS DefaultValue ,
                    ISNULL(g.[value], '') AS ColumnDescription
            FROM    syscolumns a
                    LEFT JOIN systypes b ON a.xusertype = b.xusertype
                    LEFT JOIN (SELECT * FROM systypes WHERE xtype IN(SELECT xtype FROM systypes WHERE xtype <> xusertype) AND xtype = dbo.systypes.xusertype) b2 ON b.xtype = b2.xtype
                    INNER JOIN sysobjects d ON a.id = d.id
                                               AND d.xtype IN ( 'U', 'V' )
                                               AND d.name NOT IN( 'dtproperties', 'sysconstraints', 'syssegments')
                    LEFT JOIN syscomments e ON a.cdefault = e.id
                    LEFT JOIN sysproperties g ON a.id = g.id
                                                 AND a.colid = g.smallid
                                                 AND g.name = 'MS_Description'
                    LEFT JOIN sysproperties f ON d.id = f.id
                                                 AND f.smallid = 0
                                                 AND f.name = 'MS_Description'";

        /// <summary>
        /// 创建2005及以上版本数据字典的视图sql脚本
        /// </summary>
        public const String CREATE_DATA_DICT_VIEW_SQL2005 = @"
        CREATE VIEW v_sys_DataDict
        AS
            SELECT  d.name AS TableName ,
                    d.id AS TableId ,
                    CAST(CASE d.type
                      WHEN 'U' THEN 0
                      ELSE 1
                    END AS BIT) AS IsView ,
                    ISNULL(f.value, '') AS TableDescription ,
                    a.colorder AS [ColumnOrder] ,
                    a.name AS ColumnName ,
                    CAST(CASE WHEN COLUMNPROPERTY(a.id, a.name, 'IsIdentity') = 1 THEN 1
                         ELSE 0
                    END AS BIT) AS IsIdentity ,
                    CAST(CASE WHEN EXISTS ( SELECT   1
                                       FROM     sysobjects
                                       WHERE    xtype = 'PK'
                                                AND parent_obj = a.id
                                                AND name IN (
                                                SELECT  name
                                                FROM    sysindexes
                                                WHERE   indid IN (
                                                        SELECT  indid
                                                        FROM    sysindexkeys
                                                        WHERE   id = a.id
                                                                AND colid = a.colid ) ) )
                         THEN 1
                         ELSE 0
                    END AS BIT) AS IsPrimaryKey ,
                    --LOWER(b.name) AS SqlType ,
                    CAST(CASE WHEN b.xtype = b.xusertype THEN 0 ELSE 1 END AS BIT) AS IsUserType ,
                    b.name AS UserTypeName ,
                    LOWER(CASE WHEN b.xtype = b.xusertype THEN b.NAME ELSE b2.NAME END) AS SqlType ,
                    a.length AS Size ,
                    COLUMNPROPERTY(a.id, a.name, 'PRECISION') AS [Precision] ,
                    ISNULL(COLUMNPROPERTY(a.id, a.name, 'Scale'), 0) AS Scale ,
                    CAST(CASE WHEN a.isnullable = 1 THEN 1
                         ELSE 0
                    END AS BIT) AS IsNullable ,
                    ISNULL(e.[text], '') AS DefaultValue ,
                    ISNULL(g.[value], '') AS ColumnDescription
            FROM    syscolumns a
                    LEFT JOIN systypes b ON a.xusertype = b.xusertype
                    LEFT JOIN sys.types b2 ON b.xtype = b2.system_type_id AND b2.is_user_defined = 0 AND b2.name <> 'sysname'
                    INNER JOIN sysobjects d ON a.id = d.id
                                               AND d.xtype IN ( 'U', 'V' )
                                               AND d.name NOT IN( 'dtproperties', 'sysconstraints', 'syssegments')
                    LEFT JOIN syscomments e ON a.cdefault = e.id
                    LEFT JOIN sys.extended_properties g ON a.id = g.major_id
                                                           AND a.colid = g.minor_id
                                                           AND g.name = 'MS_Description'
                    LEFT JOIN sys.extended_properties f ON d.id = f.major_id
                                                           AND f.minor_id = 0
                                                           AND f.name = 'MS_Description'";
        #endregion

    }
}
