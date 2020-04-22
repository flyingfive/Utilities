using System;
using System.Linq;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using FlyingFive.Tests.Entities;
using System.Collections.Generic;
using FlyingFive.Data;
using FlyingFive.Utils;
using FlyingFive.Data.Drivers.SqlServer;
using System.Data;
using FlyingFive.Data.Interception;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using FlyingFive.DynamicProxy;

namespace FlyingFive.Tests
{
    public enum A
    {
        [System.ComponentModel.Description("这是测试枚举项1")]
        Field1,
        [System.ComponentModel.Description("this is test enum item 2")]
        Field2
    }
    [TestClass]
    public class UnitTest2
    {
        private static Type _invocationHandlerType = null;
        [TestMethod]
        public void TestSnowflakeId()
        {
            var t1 = Interlocked.CompareExchange<Type>(ref _invocationHandlerType, typeof(Int32), null);
            var idCreator = SnowflakeId.Default;
            var batchSize = 100000;
            var count = 40;
            var connectionString = @"Data Source=10.0.0.18;Initial Catalog=Northwind;User Id=sa;Password=sa.;Connect Timeout=5;";

            System.Threading.Tasks.Parallel.For(0, count, (x) =>
            {
                var session = new MsSqlHelper(connectionString);
                var dt = new DataTable("ID");
                dt.Columns.AddRange(new DataColumn[] {
                    new DataColumn("ID",typeof(long)){ AllowDBNull=false }
                });
                dt.PrimaryKey = new DataColumn[] { dt.Columns[0] };
                for (int i = 0; i < batchSize; i++)
                {
                    var row = dt.NewRow();
                    row["ID"] = idCreator.CreateUniqueId();
                    dt.Rows.Add(row);
                }
                session.BulkCopy(dt);
            });
            Debug.WriteLine("complete");
        }
        [TestMethod]
        public void TestSBCMethod()
        {
            var tt1 = typeof(byte[]).ToSqlDbType();
            var tt2 = typeof(Int32?).GetUnderlyingType();
            var m = 7.125698520546M.TruncateDec(4);
            var list = new List<a>();
            var d = typeof(IList<>).IsAssignableFrom(list.GetType());
            var success = typeof(Int16?).IsNullableType();
            var xx = list.GetType().GetGenericTypeDefinition().GetInterfaces().Any(tt => tt.IsGenericType && tt.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            var t = typeof(Int16?).GetUnderlyingType();
            var s = System.Globalization.CultureInfo.GetCultureInfo("zh-CN");
            var a = "ABC".ToSBC();
            var b = a.ToDBC();
            var flag = a.GetType().IsCustomType();
            var text = A.Field1.GetEnumDescription();
            var t2 = A.Field2.GetEnumDescription();
        }
        [TestMethod]
        public void TestMapping()
        {
            var list = new List<Employee>();
            try
            {
                var connectionString = "Data Source=173.31.15.53,2012;Initial Catalog=Northwind;User Id=sa;Password=sa;";
                var dbHelper = new MsSqlHelper(connectionString);
                var result = CodePerformanceTester.Test((() =>
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = "select  TOP  1000 * from Employees";
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            list = reader.AsEnumerable<Employee>().ToList();
                            //var data = list.ToDataTable();
                        }
                        command.CommandText = "SELECT TOP  1000 * FROM Employees";
                        var da = new SqlDataAdapter(command);
                        var dt = new DataTable();
                        da.Fill(dt);
                        var list1 = dt.ToList<Employee>();
                        var list2 = dt.ToList();
                    }
                }));
                Debug.WriteLine(result.ToString());
                Console.WriteLine(result.ToString());
            }
            catch (Exception ex)
            {
            }
        }

        [TestMethod]
        public void TestFlying()
        {
            var connectionString = @"Data Source=173.31.15.53,2012;Initial Catalog=sxposflag10;User Id=sa;Password=sa;Connect Timeout=5;";

            FlyingFive.Data.Interception.GlobalDbInterception.Add(new a());

            var session = new MsSqlHelper(connectionString);
            var obj = session.ExecuteScalar("SELECT count(*) FROM t_Item");
            var list = session.SqlQuery<Model>("select * from t_Item where FNumber like @number", new { number = "1.1%" }).ToList();
        }

        [TestMethod]
        public void TestDynamicProxy()
        {
            ProxyObjectFactory.Default.SetupProxyHandlerType(typeof(TestHandler));
            var id = Guid.NewGuid();
            var types = new Type[] { typeof(TestProxyInterceptor) };
            var test = ProxyObjectFactory.Default.CreateInterfaceProxyWithoutTarget<ITestData>(new object[] { "test", id, new Model() { FItemID = 2351, FName = "Password", FNumber = "2.3.5.1" } }, types);
            Debug.WriteLine("返回结果：" + test.test());
        }

        [TestMethod]
        public void TestEncription()
        {
            var pwd = "Sixun#20adsfFaeQ";
            var pwd2 = "adsfFaeQidl2308didl2308didl2308d";
            var des = new Security.TripleDesCryptographicProvider(pwd);
            var aes = new Security.AesCryptographicProvider(pwd2);
            var text = "liubqliubqliubqliubqliubqliubqliubq";
            var cipherText = des.Encrypt(text);
            var plainText = des.Decrypt(cipherText);
            Assert.AreEqual(text, plainText);
            var c2 = aes.Encrypt(text);
            var p2 = aes.Decrypt(c2);
            Assert.AreEqual(p2, text);
        }
    }

    public class TestProxyInterceptor : IProxyExecutionInterceptor
    {
        public void PostProceed(ProxyExecutionContext executionContext)
        {
            Debug.WriteLine("PostProceed");
            Debug.WriteLine(executionContext.Proxy.GetType().FullName);
            Debug.WriteLine((executionContext.Addition["aaa"] as Model).FName);
            Debug.WriteLine("ReturnValue=" + executionContext.ReturnValue.ToString());
        }

        public void PreProceed(ProxyExecutionContext executionContext)
        {
            Debug.WriteLine("PreProceed");
            Debug.WriteLine(executionContext.Method.Name);
            Debug.WriteLine(executionContext.Proxy.GetType().FullName);
            executionContext.Addition.Add("aaa", new Model() { FName = "Model", UUID = Guid.NewGuid() });
        }
    }

    public class TestHandler : BaseInvocationHandler
    {
        public TestHandler(string name, Guid guid, Model model)
        {
        }

        public override void PerformProceed(ProxyExecutionContext context)
        {
            context.ReturnValue = "测试结果";
        }
    }

    public interface ITestData { string test(); }

    public class Model
    {
        public int FItemID { get; set; }
        public int FItemClassID { get; set; }
        public string FNumber { get; set; }
        public string FName { get; set; }
        public string FFullName { get; set; }
        public Guid UUID { get; set; }
        public byte[] FModifyTime { get; set; }
    }
    public class a : FlyingFive.Data.Interception.IDbCommandInterceptor
    {
        public void NonQueryExecuted(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            Debug.WriteLine("NonQueryExecuted:" + command.CommandText);
        }

        public void NonQueryExecuting(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            Debug.WriteLine("NonQueryExecuting:" + command.CommandText);
        }

        public void ReaderExecuted(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            Debug.WriteLine("ReaderExecuted:" + command.CommandText);
        }

        public void ReaderExecuting(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            Debug.WriteLine("ReaderExecuting:" + command.CommandText);
        }

        public void ScalarExecuted(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            Debug.WriteLine("ScalarExecuted:" + command.CommandText);
        }

        public void ScalarExecuting(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            Debug.WriteLine("ScalarExecuting:" + command.CommandText);
        }
    }
}
