using System;
using FlyingFive;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using FlyingFive.Data;

namespace FlyingFive.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[] { new DataColumn("Id"), new DataColumn("Name", typeof(string)), new DataColumn("BirthDay", typeof(DateTime)) });
            for (int i = 0; i < 10; i++)
            {
                var row = dt.NewRow();
                row.ItemArray = new object[] { i, "test" + i.ToString(), i % 2 == 0 ? (object)DateTime.Now.AddYears(-i) : DBNull.Value };
                dt.Rows.Add(row);
            }
            try
            {
                var list = dt.CreateDataReader().ToList<User>();
            }
            catch (Exception ex)
            {

                throw;
            }
            //Type t = typeof(Int32);
            //var flag = t.IsNullable();
            //var code = Type.GetTypeCode(typeof(Guid));
            //var t2 = typeof(Int32?);
            //var methdod = Data.Extensions.DataReaderMethods.GetReaderMethod(typeof(Object));
            //flag = t2.IsNullable();
            var md5 = "123".MD5();
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? BirthDay { get; set; }
    }
}
