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

namespace FlyingFive.Tests
{
    [TestClass]
    public class UnitTest2
    {
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
                            list = reader.ToList<Employee>();
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
            using (var context = new MsSqlContext("Data Source=173.31.15.53,2012;Initial Catalog=Northwind;User Id=sa;Password=sa;"))
            {
                //var cnt = context.SqlQuery("INSERT INTO Employees([LastName],[FirstName]) VALUES(@LastName, @FirstName)", new { FirstName = "LIU", LastName = "BIQING" });
                var list = context.SqlQuery<Employee>("select * from employees where [EmployeeID] = @EmployeeID", new { EmployeeID = 1 }).ToList();
            }

        }
    }
}
