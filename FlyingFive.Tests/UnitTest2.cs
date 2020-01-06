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
        [TestMethod]
        public void TestSBCMethod()
        {
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
                var result = CodeTimer.Time("test", 1, (() =>
                {
                    using (var connection = new SqlConnection("Data Source=173.31.15.53,2012;Initial Catalog=Northwind;User Id=sa;Password=sa;"))
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
            FlyingFive.Data.Interception.GlobalDbInterception.Add(new a());

            var connectionString = @"Data Source=10.0.0.18;Initial Catalog=AIS20121019142414;User Id=sa;Password=sa.;Connect Timeout=5;";
            var session = new MsSqlHelper(connectionString);
            var obj = session.ExecuteScalar("SELECT count(*) FROM t_Item");
            var list = session.SqlQuery<Model>("select * from t_Item where FNumber like @number", new { number = "1.1%" }).ToList();
        }
    }

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
