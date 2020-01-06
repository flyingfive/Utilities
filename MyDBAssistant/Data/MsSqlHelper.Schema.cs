/* ==============================================================================
 * 功能描述：MsSqlHelper  
 * 创 建 者：liubq
 * 创建日期：2015/10/22 12:42:48
 * ==============================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Data.SqlClient;
using MyDBAssistant.Schema;
using MyDBAssistant.Utils;
using MyDBAssistant.Caching;

namespace MyDBAssistant.Data
{
    /// <summary>
    /// MsSqlHelper
    /// </summary>
    public partial class MsSqlHelper
    {
        #region create schema script
        /// <summary>
        /// 创建2000版本数据字典的视图sql脚本
        /// </summary>
        public const String CREATE_DATA_DICT_VIEW_SQL2000 = @"
        CREATE VIEW v_sys_DataDict
        AS
            SELECT  d.name AS TableName ,
                    d.id AS TableId ,
                    CASE d.type
                      WHEN 'U' THEN 0
                      ELSE 1
                    END AS IsView ,
                    ISNULL(f.value, '') AS TableDescription ,
                    a.colorder AS [ColumnOrder] ,
                    a.name AS ColumnName ,
                    CASE WHEN COLUMNPROPERTY(a.id, a.name, 'IsIdentity') = 1 THEN 1
                         ELSE 0
                    END AS IsIdentity ,
                    CASE WHEN EXISTS ( SELECT   1
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
                    END AS IsPrimaryKey ,
                    --LOWER(b.name) AS SqlType ,
                    CASE WHEN b.xtype = b.xusertype THEN 0 ELSE 1 END AS IsUserType ,
                    b.name AS UserTypeName ,
                    LOWER(CASE WHEN b.xtype = b.xusertype THEN  LOWER(b.name) ELSE b2.name END) AS SqlType ,
                    a.length AS Size ,
                    COLUMNPROPERTY(a.id, a.name, 'PRECISION') AS [Precision] ,
                    ISNULL(COLUMNPROPERTY(a.id, a.name, 'Scale'), 0) AS Scale ,
                    CASE WHEN a.isnullable = 1 THEN 1
                         ELSE 0
                    END AS IsNullable ,
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
                    CASE d.type
                      WHEN 'U' THEN 0
                      ELSE 1
                    END AS IsView ,
                    ISNULL(f.value, '') AS TableDescription ,
                    a.colorder AS [ColumnOrder] ,
                    a.name AS ColumnName ,
                    CASE WHEN COLUMNPROPERTY(a.id, a.name, 'IsIdentity') = 1 THEN 1
                         ELSE 0
                    END AS IsIdentity ,
                    CASE WHEN EXISTS ( SELECT   1
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
                    END AS IsPrimaryKey ,
                    --LOWER(b.name) AS SqlType ,
                    CASE WHEN b.xtype = b.xusertype THEN 0 ELSE 1 END AS IsUserType ,
                    b.name AS UserTypeName ,
                    LOWER(CASE WHEN b.xtype = b.xusertype THEN b.NAME ELSE b2.NAME END) AS SqlType ,
                    a.length AS Size ,
                    COLUMNPROPERTY(a.id, a.name, 'PRECISION') AS [Precision] ,
                    ISNULL(COLUMNPROPERTY(a.id, a.name, 'Scale'), 0) AS Scale ,
                    CASE WHEN a.isnullable = 1 THEN 1
                         ELSE 0
                    END AS IsNullable ,
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

        private bool _hasSchemaViewCreated = false;
        /// <summary>
        /// 获取DB数据库构架信息的缓存KEY
        /// </summary>
        public const string DB_SCHEMA_CACHE_KEY_PATTERN = "load_database_schema_{0}";

        /// <summary>
        /// 创建系统数据字典视图
        /// </summary>
        public void CreateDataDictView()
        {
            if (!_hasSchemaViewCreated)
            {
                var id = ExecuteQueryAsSingle("SELECT OBJECT_ID('v_sys_DataDict', 'V')", CommandType.Text);
                _hasSchemaViewCreated = id != null && id != DBNull.Value;
                if (!_hasSchemaViewCreated)
                {
                    var version = ExecuteQueryAsSingle("SELECT @@VERSION", CommandType.Text).ToString().Replace(" ", string.Empty);
                    if (version.StartsWith("MicrosoftSQLServer2000", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ExecuteNonQuery(CREATE_DATA_DICT_VIEW_SQL2000, CommandType.Text);
                    }
                    else
                    {
                        ExecuteNonQuery(CREATE_DATA_DICT_VIEW_SQL2005, CommandType.Text);
                    }
                    _hasSchemaViewCreated = true;
                }
                else
                {
                    try
                    {
                        var dt = ExecuteQueryAsDataTable("SELECT TOP 1 TableName,TableId  FROM v_sys_DataDict", CommandType.Text);
                    }
                    catch (Exception ex)                //跨版本还原数据库（2000还原到2008上）后，视图绑定会出错，需要删除重建
                    {
                        ExecuteNonQuery("DROP VIEW v_sys_DataDict", CommandType.Text);
                        _hasSchemaViewCreated = false;
                        CreateDataDictView();
                    }
                }
            }
        }

        private IList<Table> _tables = null;
        private static object _locker = new object();

        /// <summary>
        /// 加载当前连接信息的DB构架
        /// </summary>
        public void LoadSchema()
        {
            //lock (_locker)
            //{
            //    if (_tables == null || _tables.Count == 0)
            //    {
            //        var dt = ExecuteQueryAsDataTable("SELECT * FROM dbo.v_sys_DataDict", CommandType.Text);
            //        var dtTables = dt.Copy();
            //        var dtColumns = dt.Copy();
            //        var tables = dtTables.DefaultView.ToTable(true, new string[] { "TableId", "TableName", "IsView", "TableDescription" });
            //        var lst = DataReaderConverter.ToList<Table>(tables.CreateDataReader()).ToList();
            //        var columns = DataReaderConverter.ToList<Column>(dtColumns.Copy().CreateDataReader()).ToList();
            //        foreach (var table in lst)
            //        {
            //            table.Columns = columns.Where(c => c.TableId == table.TableId).ToList();
            //        }
            //        _tables = lst;
            //    }
            //}
            var cache = Singleton<ICacheManager>.Instance;
            lock (_locker)
            {
                if (Singleton<ICacheManager>.Instance == null)
                {
                    Singleton<ICacheManager>.Instance = new MemoryCacheManager();
                }
            }
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            int cacheHours = 100 * 24 * 60;         //默认缓存100天
            var key = string.Format(DB_SCHEMA_CACHE_KEY_PATTERN, builder.InitialCatalog.ToLower());
            var tables = cache.TryGet<IList<Table>>(key, cacheHours, () =>
            {
                CreateDataDictView();
                var dt = ExecuteQueryAsDataTable("SELECT TableName, TableId, IsView, TableDescription, ColumnOrder, ColumnName, IsIdentity, IsPrimaryKey, SqlType, Size, [Precision], Scale, IsNullable, DefaultValue, ColumnDescription FROM dbo.v_sys_DataDict", CommandType.Text);
                var dtTables = dt.Copy();
                var dtColumns = dt.Copy();
                dtTables = dtTables.DefaultView.ToTable(true, new string[] { "TableId", "TableName", "IsView", "TableDescription" });
                var lst = DataReaderConverter.ToList<Table>(dtTables.CreateDataReader()).ToList();
                var columns = DataReaderConverter.ToList<Column>(dtColumns.Copy().CreateDataReader()).ToList();
                foreach (var table in lst)
                {
                    table.Columns = columns.Where(c => c.TableId == table.TableId).ToList();
                }
                return lst;
            });
            _tables = tables;
        }

        /// <summary>
        /// 数据库表构架
        /// </summary>
        public IList<Table> Tables
        {
            get
            {
                if (_tables == null || _tables.Count == 0)
                {
                    LoadSchema();
                }
                return _tables;
            }
        }


        static DbAccessException WrapException(Exception ex)
        {
            return new DbAccessException("数据库操作异常.", ex);
        }
    }
}
