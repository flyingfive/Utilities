using FlyingFive.Caching;
using FlyingFive.Data.Kernel;
using FlyingFive.Data.Schema;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyingFive.Data.Drivers.SqlServer
{
    /// <summary>
    /// SQL Server操作工具
    /// </summary>
    public class MsSqlHelper : DatabaseHelper, IDatabaseHelper
    {
        /// <summary>
        /// SQL服务器版本
        /// </summary>
        public SqlServerVersion Version { get; private set; }

        public MsSqlHelper(string connectionString)
            : this(new SqlServerConnectionFactory(connectionString))
        {
        }

        public MsSqlHelper(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
        {
            //ReadServerVersion();
            //默认创建sys_datadict视图，除非显示配置不用数据字典
            var config = ConfigurationManager.AppSettings["UnusedDataDict"];
            if (!string.IsNullOrEmpty(config))
            {
                if (config.IsTrue()) { return; }
            }
            Task.Factory.StartNew(CreateDataDictView);
        }

        /// <summary>
        /// 读取服务器版本
        /// </summary>
        public SqlServerVersion ReadServerVersion()
        {
            Func<string> readVersion = () =>
            {
                using (var connection = DbConnectionFactory.CreateConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT SERVERPROPERTY('ProductVersion')";
                    var obj = command.ExecuteScalar().ToString();
                    return obj;
                }
            };
            var cache = Singleton<ICacheManager>.Instance;
            if (cache != null)
            {
                var ver = cache.TryGet<string>(base.DbConnectionFactory.ConnectionString, readVersion);
                this.Version = ConvertToSqlVersion(ver);
            }
            else
            {
                this.Version = ConvertToSqlVersion(readVersion());
            }
            return this.Version;
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

        private static bool? _hasSchemaViewCreated = false;

        /// <summary>
        /// 创建系统数据字典视图
        /// </summary>
        private void CreateDataDictView()
        {
            if (_hasSchemaViewCreated.HasValue && _hasSchemaViewCreated.Value)
            {
                return;
            }
            using (var connection = base.DbConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT OBJECT_ID('v_sys_DataDict', 'V')";
                var obj = command.ExecuteScalar();
                _hasSchemaViewCreated = obj != null && obj != DBNull.Value;
                if (!_hasSchemaViewCreated.Value)
                {
                    if (Convert.ToInt32(this.Version) < Convert.ToInt32(SqlServerVersion.SQL2005))
                    {
                        command.CommandText = CREATE_DATA_DICT_VIEW_SQL2000;
                    }
                    else
                    {
                        command.CommandText = CREATE_DATA_DICT_VIEW_SQL2005;
                    }
                    command.ExecuteNonQuery();
                    _hasSchemaViewCreated = true;
                }
                else
                {
                    try
                    {
                        command.CommandText = "SELECT TOP 1 TableName FROM dbo.v_sys_DataDict";
                        var name = command.ExecuteScalar();
                    }
                    catch              //跨版本还原数据库（2000还原到2008上）后，视图绑定会出错，需要删除重建
                    {
                        _hasSchemaViewCreated = false;
                        command.CommandText = "DROP VIEW dbo.v_sys_DataDict";
                        command.ExecuteNonQuery();
                        CreateDataDictView();
                    }
                }
            }
        }

        private static object _syncSchemaObj = new object();
        private static readonly ConcurrentDictionary<string, TableInfo> _loadedTables = new ConcurrentDictionary<string, TableInfo>();

        /// <summary>
        /// 获取表构架信息(不适用于跨库操作)
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <returns></returns>
        public TableInfo GetTableSchema(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) { throw new ArgumentException("参数tableName不能为空"); }
            TableInfo table = null;
            if (_loadedTables.TryGetValue(tableName, out table))
            {
                return table;
            }
            lock (_syncSchemaObj)
            {
                table = _loadedTables.GetOrAdd(tableName, (key) =>
                {
                    var sql = "SELECT DISTINCT TableName, TableId, IsView, TableDescription FROM dbo.v_sys_DataDict WHERE TableName = @TableName";
                    TableInfo schema = this.SqlQuery<TableInfo>(sql, new { TableName = key }).FirstOrDefault();
                    if (schema != null)
                    {
                        sql = "SELECT TableId, ColumnOrder, ColumnName, IsIdentity, IsPrimaryKey, SqlType, Size, [Precision], Scale, IsNullable, DefaultValue, ColumnDescription FROM dbo.v_sys_DataDict WHERE TableId = @TableId";
                        schema.Columns = this.SqlQuery<ColumnInfo>(sql, new { TableId = schema.TableId }).ToList();
                    }
                    return schema;
                });
                return table;
            }
        }

        /// <summary>
        /// 按ProdcutVersion转换SQLServer版本
        /// </summary>
        /// <param name="productVersion"></param>
        /// <param name="throwOverflow"></param>
        /// <returns></returns>
        public static SqlServerVersion ConvertToSqlVersion(string productVersion, bool throwOverflow = false)
        {
            // SQL Server 2008 的10。0
            // 10.50 2008 R2 SQL Server
            // 2012 SQL Server 11.0. xx
            // 12.0 SQL Server 2014
            // SQL Server 2016 的13。0
            // SQL Server 2017 的 14.0. xx
            switch (productVersion.Substring(0, 4))
            {
                case "14.0": return SqlServerVersion.SQL2017;
                case "13.0": return SqlServerVersion.SQL2016;
                case "12.0": return SqlServerVersion.SQL2014;
                case "11.0": return SqlServerVersion.SQL2012;
                case "10.5": return SqlServerVersion.SQL2008R2;
                case "10.0": return SqlServerVersion.SQL2008;
                case "9.00": return SqlServerVersion.SQL2005;
                case "8.00": return SqlServerVersion.SQL2000;
                default:
                    if (throwOverflow)
                    {
                        throw new ArgumentException(string.Format("无法识别的SQL版本：{0}", productVersion));
                    }
                    return SqlServerVersion.Unknown;
            }
        }

        #region create schema info view script
        /// <summary>
        /// 创建2000版本数据字典的视图sql脚本
        /// </summary>
        public const String CREATE_DATA_DICT_VIEW_SQL2000 = @"
        CREATE VIEW dbo.v_sys_DataDict
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
        CREATE VIEW dbo.v_sys_DataDict
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


    /// <summary>
    /// SqlServer版本
    /// </summary>
    public enum SqlServerVersion : uint
    {
        Unknown = 0,
        SQL2000 = 1,
        SQL2005 = 2,
        SQL2008 = 3,
        SQL2008R2 = 4,
        SQL2012 = 5,
        SQL2014 = 6,
        SQL2016 = 7,
        SQL2017 = 8,
    }
}
