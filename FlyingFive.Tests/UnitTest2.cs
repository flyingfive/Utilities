﻿using System;
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
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestSBCMethod()
        {
            var s = System.Globalization.CultureInfo.GetCultureInfo("zh-CN");
            var a = "ABC".ToSBC();
            var b = a.ToDBC();
            var flag = a.GetType().IsCustomType();
        }
        [TestMethod]
        public void TestMapping()
        {
            var list = new List<Employee>();
            try
            {
                var result = CodeTimer.Time("test", 10, (() =>
                {
                    using (var connection = new SqlConnection("Data Source=173.31.15.53,2012;Initial Catalog=Northwind;User Id=sa;Password=sa;"))
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = "select * from Employees";
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            list = reader.AsEnumerable<Employee>().ToList();
                        }
                    }
                }));
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
