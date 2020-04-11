using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using FlyingFive.Data;
using FlyingFive.Win;

namespace FlyingFive.Data.Drivers.SqlServer
{
    /// <summary>
    /// SqlServer安装环境检测
    /// </summary>
    public class SqlServerInstallationChecker
    {
        /// <summary>
        /// 检测SqlServer网络实例。
        /// </summary>
        /// <returns></returns>
        private IList<SqlServerInfo> DetectSqlServers()
        {
            var clientFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            //var clientFactory = SqlClientFactory.Instance;
            if (!clientFactory.CanCreateDataSourceEnumerator)
            {
                return new List<SqlServerInfo>();
            }
            var dataTable = clientFactory.CreateDataSourceEnumerator().GetDataSources();
            var list = dataTable.ToList<SqlServerInfo>();
            return list;
        }

        public static readonly string SqlExeFileName = "sqlservr.exe";

        /// <summary>
        /// 检测本地的SqlServer安装实例
        /// </summary>
        /// <returns></returns>
        public List<SqlServerInfo> DetectLocalSqlServers()
        {
            var list = new List<SqlServerInfo>();
            var basePath = @"SOFTWARE\Microsoft\Microsoft SQL Server";
            var rootRegistry = RegistryUtility.OpenRegistryKey(Microsoft.Win32.RegistryHive.LocalMachine, basePath);
            if (rootRegistry == null) { return list; }
            if (!rootRegistry.GetSubKeyNames().Contains("Instance Names")) { return list; }
            var installationPath = string.Format(@"{0}\{1}", basePath, @"Instance Names\SQL");
            var names = RegistryUtility.ReadRegistryNames(Microsoft.Win32.RegistryHive.LocalMachine, installationPath);
            var def = string.Empty;
            foreach (var instanceName in names)
            {
                var server = new SqlServerInfo() { ServerName = Environment.MachineName, InstanceName = instanceName };
                var regName = RegistryUtility.ReadRegistryValue(Microsoft.Win32.RegistryHive.LocalMachine, installationPath, instanceName, def);
                server.IsDefaultInstance = string.Equals(instanceName, "MSSQLSERVER", StringComparison.CurrentCultureIgnoreCase);
                server.SqlSetupRegistryPath = string.Format(@"{0}\{1}\Setup", basePath, regName);
                var binnPath = RegistryUtility.ReadRegistryValue(Microsoft.Win32.RegistryHive.LocalMachine, server.SqlSetupRegistryPath, "SQLBinRoot", def);
                var exeFile = Path.Combine(binnPath, SqlExeFileName);
                if (Directory.Exists(binnPath) && File.Exists(exeFile))
                {
                    server.BinPath = binnPath;
                    var fileInfo = FileVersionInfo.GetVersionInfo(exeFile);
                    server.ProductVersion = fileInfo.ProductVersion;
                    server.SqlVersion = MsSqlHelper.ConvertToSqlVersion(server.ProductVersion);
                }
                else
                {
                    continue;
                }
                server.SqlGroup = RegistryUtility.ReadRegistryValue(Microsoft.Win32.RegistryHive.LocalMachine, server.SqlSetupRegistryPath, "SQLGroup");
                server.Edition = RegistryUtility.ReadRegistryValue(Microsoft.Win32.RegistryHive.LocalMachine, server.SqlSetupRegistryPath, "Edition", def);
                server.Version = RegistryUtility.ReadRegistryValue(Microsoft.Win32.RegistryHive.LocalMachine, server.SqlSetupRegistryPath, "Version", def);
                server.SqlInstanceRegistryPath = string.Format("{0}\\{1}\\MSSQLServer", basePath, regName);
                var defaultDataPath = RegistryUtility.ReadRegistryValue(Microsoft.Win32.RegistryHive.LocalMachine, server.SqlInstanceRegistryPath, "DefaultData", def);
                if (Directory.Exists(defaultDataPath)) { server.DefaultDataPath = defaultDataPath; }
                list.Add(server);
            }
            return list;
        }

        /// <summary>
        /// 检测本地安装最新版本的SqlServer实例
        /// </summary>
        /// <returns></returns>
        public SqlServerInfo DetectLocalNewestVersionSqlServer()
        {
            var list = DetectLocalSqlServers();
            if (list.Count == 0) { return null; }
            var server = list.OrderByDescending(s => Version.Parse(s.Version)).FirstOrDefault();
            return server;
        }

        public static bool CheckPathIsSqlRoot(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { return false; }
            if (!Directory.Exists(path)) { return false; }
            var exePath = Path.Combine(path, SqlExeFileName);
            if (!File.Exists(exePath)) { return false; }
            return true;
        }
    }

    /// <summary>
    /// SqlServer实例安装信息
    /// </summary>
    public class SqlServerInfo
    {
        /// <summary>
        /// 服务器(机器)名称
        /// </summary>
        public string ServerName { get; set; }
        /// <summary>
        /// SqlServer实例名
        /// </summary>
        public string InstanceName { get; set; }
        /// <summary>
        /// 是否默认实例
        /// </summary>
        public bool IsDefaultInstance { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }
        public string Edition { get; set; }
        /// <summary>
        /// Sql安装bin目录路径（sqlservr.exe目录）
        /// </summary>
        public string BinPath { get; set; }
        /// <summary>
        /// 此实例配置的默认DB存放路径
        /// </summary>
        public string DefaultDataPath { get; set; }
        /// <summary>
        /// SqlServer产品版本
        /// </summary>
        public string ProductVersion { get; set; }
        /// <summary>
        /// sa密码
        /// </summary>
        public string SaPassword { get; set; }

        public SqlServerVersion SqlVersion { get; set; }
        /// <summary>
        /// 是否64位版本，True:64位实例，False:32位实例
        /// </summary>
        public bool Is64Bit { get; set; }
        /// <summary>
        /// 实例的Windows服务名称
        /// </summary>
        public string WinServiceName { get; set; }
        /// <summary>
        /// SQL安装信息目录
        /// </summary>
        public string SqlSetupRegistryPath { get; set; }
        /// <summary>
        /// SQL实例注册表路径
        /// </summary>
        public string SqlInstanceRegistryPath { get; set; }
        /// <summary>
        /// SQL安装Windows服务注册表路径
        /// </summary>
        public string SqlServiceRegistryPath { get; set; }
        /// <summary>
        /// 此实例监听的端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// sql账户组SID
        /// </summary>
        public string SqlGroup { get; set; }

        public override string ToString()
        {
            if (this.SqlVersion == SqlServerVersion.Unknown) { return "未知"; }
            var name = string.Format("Microsoft SQL Server {0}{1}", this.SqlVersion.ToString().Substring(3, 4), this.SqlVersion.ToString().EndsWith("R2") ? " R2" : string.Empty);
            return string.Format("{0} {1}", name, Edition);
        }


        /// <summary>
        /// 读取SqlServer实例安装信息（产品版本、路径等信息）
        /// </summary>
        /// <param name="sqlServer"></param>
        /// <param name="saPwd"></param>
        public void ReadInstallationInfo(string saPwd)
        {
            this.SaPassword = saPwd;
            var builder = new SqlConnectionStringBuilder() { DataSource = this.IsDefaultInstance ? this.ServerName : string.Format(@"{0}\{1}", this.ServerName, this.InstanceName), InitialCatalog = "master", UserID = "sa", Password = saPwd, ConnectTimeout = 5 };
            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                if (string.IsNullOrEmpty(this.BinPath))
                {
                    command.CommandText = @"exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'SOFTWARE\Microsoft\MSSQLServer\Setup', N'SQLBinRoot', @SmoRoot OUTPUT SELECT @SmoRoot";
                    command.CommandType = CommandType.Text;
                    command.Parameters.Clear();
                    var parameter = new SqlParameter("@SmoRoot", "") { SqlDbType = SqlDbType.NVarChar, Size = 512, Direction = ParameterDirection.Output };
                    command.Parameters.AddRange(new SqlParameter[] { parameter });
                    var binnPath = command.ExecuteScalar().ToString();
                    var exeFile = Path.Combine(binnPath, SqlServerInstallationChecker.SqlExeFileName);
                    if (Directory.Exists(binnPath) && File.Exists(exeFile))
                    {
                        this.BinPath = binnPath;
                        var fileInfo = FileVersionInfo.GetVersionInfo(exeFile);
                        this.ProductVersion = fileInfo.ProductVersion;
                        this.SqlVersion = MsSqlHelper.ConvertToSqlVersion(this.ProductVersion);
                    }
                }
                //注册表没取到DB配置的默认文件存放路径，则取master相同位置
                if (string.IsNullOrEmpty(this.DefaultDataPath))
                {
                    command.CommandText = @"select substring(physical_name, 1, len(physical_name) - charindex('\', reverse(physical_name))) from master.sys.database_files where name=N'master'";
                    command.Parameters.Clear();
                    var defaultDbFilePath = command.ExecuteScalar().ToString();
                    if (Directory.Exists(defaultDbFilePath))
                    {
                        this.DefaultDataPath = defaultDbFilePath;
                    }
                }
                if (true)//string.IsNullOrEmpty(this.ProductVersion))
                {
                    command.CommandText = "SELECT SERVERPROPERTY('ProductVersion')";
                    command.Parameters.Clear();
                    this.ProductVersion = command.ExecuteScalar().ToString();
                    this.SqlVersion = MsSqlHelper.ConvertToSqlVersion(this.ProductVersion);
                }

                command.CommandText = "SELECT SERVERPROPERTY('Edition')";
                command.Parameters.Clear();
                var edition = command.ExecuteScalar().ToString();
                this.Is64Bit = edition.Contains("64-bit");

                command.CommandText = "SELECT @@SERVICENAME";
                command.Parameters.Clear();
                var serviceName = command.ExecuteScalar().ToString();
                serviceName = this.IsDefaultInstance ? serviceName : string.Format("MSSQL${0}", serviceName);
                var registryPath = string.Format(@"SYSTEM\CurrentControlSet\Services\{0}", serviceName);
                var imagePath = RegistryUtility.ReadRegistryValue(Microsoft.Win32.RegistryHive.LocalMachine, registryPath, "ImagePath", "");
                if (!string.IsNullOrEmpty(imagePath) && imagePath.IndexOf("\"") >= 0 && imagePath.IndexOf("\"") != imagePath.LastIndexOf("\""))
                {
                    imagePath = imagePath.Substring(imagePath.IndexOf("\"") + 1, imagePath.LastIndexOf("\"") - 1);
                    var exeFile = Path.Combine(this.BinPath, SqlServerInstallationChecker.SqlExeFileName);
                    if (File.Exists(exeFile) && new Uri(imagePath) == new Uri(exeFile))
                    {
                        this.WinServiceName = serviceName;
                        this.SqlServiceRegistryPath = registryPath;
                    }
                }
                //从日志中解析此实例使用的端口
                command.CommandText = "exec sys.sp_readerrorlog 0, 1, 'server is listening on [ ''any''', 'ipv4'";
                command.Parameters.Clear();
                using (var reader = command.ExecuteReader())
                {
                    var ordinal = reader.GetOrdinal("Text");
                    if (reader.Read())
                    {
                        var text = reader.GetString(ordinal);
                        var reg = new System.Text.RegularExpressions.Regex(@"<ipv4> \d+]");
                        var match = reg.Match(text);
                        if (match.Success)
                        {
                            var port = match.Value.Replace("<ipv4> ", "").Replace("]", "");
                            if (port.IsInt(false))
                            {
                                this.Port = Convert.ToInt32(port);
                            }
                        }
                    }
                }
                connection.Close();
            }
        }
    }
}
