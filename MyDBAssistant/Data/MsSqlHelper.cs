using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace MyDBAssistant.Data
{
    /// <summary>
    /// MsSql数据库操作辅助工具
    /// </summary>
    //[Intercept("数据访问拦截")]
    public partial class MsSqlHelper// : /*ContextBoundObject,*/ IDatabaseHelper
    {
        private string _logFile = string.Empty;
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; private set; }
        /// <summary>
        /// 命令超时时间,以秒为单位(默认60秒)
        /// </summary>
        public int CommandTimeout { get; set; }

        /// <summary>
        /// 实例化一个MsSql数据库辅助工具对象
        /// </summary>
        /// <param name="connectionString"></param>
        public MsSqlHelper(string connectionString)
        {
            _logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Error.Log");
            var timeout = ConfigurationManager.AppSettings["DatabaseCommandTimeout"];
            timeout = timeout ?? "60";
            var flag = System.Text.RegularExpressions.Regex.IsMatch(timeout, @"^\d+$");
            if (!string.IsNullOrEmpty(timeout) && flag)
            {
                CommandTimeout = Convert.ToInt32(timeout);
            }
            else
            {
                CommandTimeout = 60;
            }
            ConnectionString = connectionString;
            if (_tables == null || _tables.Count == 0)
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    LoadSchema();
                }).ContinueWith((t) =>
                {
                    if (t.Exception.InnerException != null)
                    {
                        RecordException(t.Exception.InnerException);
                    }
                    //Environment.Exit(0);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        private void RecordException(Exception exception)
        {
            try
            {
                if (File.Exists(_logFile))
                {
                    var fileInfo = new FileInfo(_logFile);
                    var fileSize = fileInfo.Length / (1024 * 1024);
                    if (fileSize >= 5)
                    {
                        File.Delete(_logFile);
                    }
                }

                var content = string.Format("[{0}] {1}{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), exception.ToString(), Environment.NewLine);
                System.IO.File.AppendAllText(_logFile, content);
            }
            catch (Exception ex)
            {

            }
        }

        public static IList<string> FindQueryParmeters(string sql)
        {
            IList<string> lst = new List<string>();
            //修正正则表达式匹配参数时，Sql中包括@@rowcount之类的变量的情况，不应该算作参数
            Regex paramReg = new Regex(@"[^@@](?<p>@\w+)");
            MatchCollection matches = paramReg.Matches(String.Concat(sql, " "));
            foreach (Match m in matches)
            {
                lst.Add(m.Groups["p"].Value.Trim());
            }
            return lst;
        }

        /// <summary>
        /// 验证数据，防止SQL注入
        /// </summary>
        /// <param name="sqlString">sql语句</param>
        /// <returns></returns>
        public static string InjectSQL(string sqlString)
        {
            if (string.IsNullOrWhiteSpace(sqlString)) { return ""; }
            //sqlString = sqlString.Replace("select ", "");
            //sqlString = sqlString.Replace("insert ", "");
            //sqlString = sqlString.Replace("update ", "");
            //sqlString = sqlString.Replace("delete from", "");
            sqlString = sqlString.Replace("drop table", "");
            sqlString = sqlString.Replace("exec master", "");
            sqlString = sqlString.Replace("net localgroup administrators", "");
            sqlString = sqlString.Replace("net user", "");
            sqlString = sqlString.Replace("truncate ", "");
            //sqlString = sqlString.Replace("exec ", "");
            //sqlString = sqlString.Replace(" or ", "");
            //sqlString = sqlString.Replace(" and ", "");
            sqlString = sqlString.Replace("copy", "");
            //str = str.Replace(":", "");
            //str = str.Replace("：", "");
            //sqlString = sqlString.Replace("count(", "");
            sqlString = sqlString.Replace("asc(", "");
            sqlString = sqlString.Replace("mid(", "");
            //sqlString = sqlString.Replace("char(", "");
            sqlString = sqlString.Replace("xp_cmdshell", "");
            //str = str.Replace(";", "");
            sqlString = sqlString.Replace("'", "");
            return sqlString;
        }

        /// <summary>
        /// 切换数据库
        /// </summary>
        /// <param name="database">要切换的数据库名称</param>
        public void ChangeDatabase(string database)
        {
            if (string.IsNullOrEmpty(ConnectionString)) { return; }
            using (var connection = new SqlConnection(ConnectionString))
            {
                string currentDb = connection.Database;
                ConnectionString = ConnectionString.Replace(currentDb, database);
            }
        }

        /// <summary>
        /// 执行非查询语句
        /// </summary>
        /// <param name="cmdText">查询命令</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="paras">参数</param>
        /// <returns>受影响行数</returns>
        public int ExecuteNonQuery(string cmdText, CommandType cmdType, params DbParameter[] paras)
        {
            int ret = 0;
            using (DbConnection connection = new SqlConnection(ConnectionString))
            {
                DbCommand cmd = connection.CreateCommand();
                cmd.CommandText = cmdText;
                cmd.CommandType = cmdType;
                cmd.CommandTimeout = CommandTimeout;
                if (paras != null && paras.Length > 0)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(paras);
                }
                connection.Open();
                ret = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                connection.Close();
                connection.Dispose();
            }
            return ret;
        }

        /// <summary>
        /// 事物批处理非查询语句
        /// </summary>
        /// <param name="commands">命令对象集合</param>
        /// <returns></returns>
        public bool ExecuteNonQuery(IList<SqlCommandModel> commands)
        {
            bool flag = false;
            using (DbConnection connection = new SqlConnection(ConnectionString))
            {
                DbTransaction trans = null;
                DbCommand command = connection.CreateCommand();
                try
                {
                    connection.Open();
                    trans = connection.BeginTransaction();
                    command.Transaction = trans;
                    foreach (var item in commands)
                    {
                        command.CommandText = item.Sql;
                        command.CommandType = item.CommandType;
                        command.CommandTimeout = item.CommandTimeout > 0 ? item.CommandTimeout : this.CommandTimeout;
                        if (item.Parameters != null && item.Parameters.Count > 0)
                        {
                            command.Parameters.Clear();
                            command.Parameters.AddRange(item.Parameters.ToArray());
                        }
                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    trans.Commit();
                    flag = true;
                }
                catch (Exception ex)
                {
                    flag = false;
                    if (trans != null && connection.State == ConnectionState.Open)
                    {
                        trans.Rollback();
                    }
                    RecordException(new DataException(command.CommandText, ex));
                    throw WrapException(ex);
                }
            }
            return flag;

        }

        /// <summary>
        /// 事物批处理非查询语句(仅支持Text类型的CommandType,存储过程请在数据库实现事物管理)
        /// </summary>
        /// <param name="cmdDic">命令集合,键:查询语句,值:参数</param>
        /// <param name="commandTimeout">执行超时时间,单位:秒</param>
        /// <returns></returns>
        [Obsolete("请使用ExecuteNonQuery(IList<SqlCommandModel> commands)方法替换")]
        public bool ExecuteNonQuery(IDictionary<string, DbParameter[]> cmdDic, int commandTimeout = 60)
        {
            bool flag = false;
            using (DbConnection connection = new SqlConnection(ConnectionString))
            {
                DbTransaction trans = null;
                DbCommand command = connection.CreateCommand();
                try
                {
                    connection.Open();
                    trans = connection.BeginTransaction();
                    command.CommandTimeout = commandTimeout;
                    command.Transaction = trans;
                    foreach (var cmdText in cmdDic.Keys)
                    {
                        command.CommandText = cmdText;
                        command.CommandType = CommandType.Text;
                        if (cmdDic[cmdText] != null && cmdDic[cmdText].Length > 0)
                        {
                            command.Parameters.Clear();
                            command.Parameters.AddRange(cmdDic[cmdText]);
                        }
                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    trans.Commit();
                    flag = true;
                }
                catch (Exception ex)
                {
                    flag = false;
                    if (trans != null && connection.State == ConnectionState.Open)
                    {
                        trans.Rollback();
                    }
                    RecordException(new DataException(command.CommandText, ex));
                    throw WrapException(ex);
                }
            }
            return flag;
        }

        /// <summary>
        /// 查询,返回结果集
        /// </summary>
        /// <param name="cmdText">查询命令</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="paras">参数</param>
        /// <returns></returns>
        public DataSet ExecuteQueryAsDataSet(string cmdText, CommandType cmdType, params DbParameter[] paras)
        {
            DataSet ds = new DataSet("DataSet");
            using (DbDataAdapter da = new SqlDataAdapter(cmdText, ConnectionString))
            {
                da.SelectCommand.CommandType = cmdType;
                if (paras != null && paras.Length > 0)
                {
                    da.SelectCommand.Parameters.Clear();
                    da.SelectCommand.Parameters.AddRange(paras);
                }
                da.SelectCommand.CommandTimeout = CommandTimeout;
                da.Fill(ds);
                da.SelectCommand.Parameters.Clear();
            }
            return ds;
        }


        /// <summary>
        /// 查询,返回结果表
        /// </summary>
        /// <param name="cmdText">查询命令</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="paras">参数</param>
        /// <returns></returns>
        public DataTable ExecuteQueryAsDataTable(string cmdText, CommandType cmdType, params DbParameter[] paras)
        {
            DataTable dt = new DataTable("DataTable");
            using (DbDataAdapter da = new SqlDataAdapter(cmdText, ConnectionString))
            {
                da.SelectCommand.CommandType = cmdType;
                if (paras != null && paras.Length > 0)
                {
                    da.SelectCommand.Parameters.Clear();
                    da.SelectCommand.Parameters.AddRange(paras);
                }
                da.SelectCommand.CommandTimeout = CommandTimeout;
                da.Fill(dt);
                da.SelectCommand.Parameters.Clear();
            }
            return dt;
        }

        /// <summary>
        /// 以List泛型集合方式返回查询数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="cmdText">查询命令</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="paras">查询参数</param>
        /// <returns></returns>
        public IList<T> ExecuteQueryAsList<T>(string cmdText, CommandType cmdType, params DbParameter[] paras) where T : class//, new()
        {
            IList<T> lst = new List<T>();
            var dt = ExecuteQueryAsDataTable(cmdText, cmdType, paras);
            if (dt == null || dt.Rows.Count <= 0) { return lst; }
            lst = DataReaderConverter.ToList<T>(dt.CreateDataReader()).ToList();
            return lst;
        }

        /// <summary>
        /// 查询,返回单一值
        /// </summary>
        /// <param name="cmdText">查询命令</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="paras">参数</param>
        /// <returns></returns>
        public object ExecuteQueryAsSingle(string cmdText, CommandType cmdType, params DbParameter[] paras)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = cmdText;
                command.CommandType = cmdType;
                if (paras != null && paras.Length > 0)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddRange(paras);
                }
                connection.Open();
                command.CommandTimeout = CommandTimeout;
                var single = command.ExecuteScalar();
                command.Parameters.Clear();
                connection.Close();
                return single;
            }
        }

        /// <summary>
        /// 向数据库中批量插入数据
        /// </summary>
        /// <param name="dt">要插入的数据表(注意TableName和ColumnName和数据库表一致)</param>
        /// <param name="batchSize">批次插入记录的行数</param>
        /// <param name="timeout">超时时间，单位：秒</param>
        /// <param name="options">copy选项</param>
        /// <returns></returns>
        public bool BulkWriteData(DataTable dt, int batchSize, int timeout, SqlBulkCopyOptions options = SqlBulkCopyOptions.Default)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                SqlTransaction trans = null;
                try
                {
                    if (timeout <= 0) { timeout = 180; }                //默认3分钟
                    if (batchSize <= 0) { batchSize = dt.Rows.Count; }
                    connection.Open();
                    trans = connection.BeginTransaction();
                    using (SqlBulkCopy copy = new SqlBulkCopy(connection, options, trans))
                    {
                        copy.BatchSize = batchSize;
                        copy.BulkCopyTimeout = timeout;
                        copy.DestinationTableName = dt.TableName;
                        foreach (DataColumn col in dt.Columns)
                        {
                            copy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                        }
                        copy.WriteToServer(dt);
                        trans.Commit();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    if (trans != null && connection.State == ConnectionState.Open)
                    {
                        trans.Rollback();
                    }
                    RecordException(ex);
                    throw WrapException(ex);
                }
            }
        }

        /// <summary>
        /// 判断表中的列是否存在
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public bool HasColumnExists(string tableName, string columnName)
        {
            DataTable columns = ExecuteQueryAsDataTable("sp_columns", CommandType.StoredProcedure, new DbParameter[] { new SqlParameter("@table_name", tableName) { SqlDbType = SqlDbType.NVarChar, Size = 384 } });
            var flag = columns.Rows.OfType<DataRow>()
                .Any(r => r["name"].ToString().Equals(columnName, StringComparison.CurrentCultureIgnoreCase));
            return flag;
        }

        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="databaseName">要创建的数据库名称</param>
        public void CreateDatabase(string databaseName)
        {
            string sql = string.Format("CREATE DATABASE [{0}]", databaseName);
            ExecuteNonQuery(sql, CommandType.Text);
        }



        /// <summary>
        /// 开始一个事务处理
        /// </summary>
        /// <param name="level">事务隔离级别</param>
        /// <returns></returns>
        public DbTransaction BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            var transaction = connection.BeginTransaction(level);
            return transaction;
        }
    }

}
