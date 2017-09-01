using System;
using System.Linq;
using FlyingFive;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using FlyingFive.Data;
using FlyingFive.Data.Mapping;
using FlyingFive.Data.Drivers.SqlServer;
using FlyingFive.Tests.Entities;
using FlyingFive.Data.Interception;
using System.Diagnostics;
using System.Text;
using FlyingFive.Data.Infrastructure;

namespace FlyingFive.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var rx = EntityMappingCollection.Mappings;
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

        [TestMethod]
        public void TestMsSqlContext()
        {
            IDbContext context = new MsSqlContext("Data Source=173.31.15.53,2012;Initial Catalog=XSK1_TANGGP;User Id=sa;Password=sa;");
            context.Session.CommandTimeout = 60;
            FlyingFive.Data.Interception.GlobalDbInterception.Add(new DbCommandInterceptor());
            //var list = context.SqlQuery<Dish>("select *  from cybr_bt_dish").ToList();
            var dish = new Dish() { DishNo = "test01", Build = DateTime.Now, CurFlag = "N", DeductType = "1", DishName = "test01", Electscale = "N", HalfFlag = "N", ItemFlag = "N", OutPrice = 12, Price1 = 10, Discount = "Y", SeriesNo = "01", TypeNo = "01", Spec = "大", Spell = "", UnitNo = "02", SuitFlag = "N", StopFlag = "N", DownFlag = "Y", DcbCode = "test", DeductAmount = 1, Explain = "just a test", SubNo = "t01", DishFlag = "Y" };
            var rows1 = context.Insert(dish);
            //var d2 = context.QueryByKey<Dish>("test01");
            var dish2 = context.Query<Dish>().Where(u => u.DishNo.Equals("test01")).FirstOrDefault();
            if (dish2 != null)
            {
                dish2.DishName = "aaaabbb";
                var rows = context.Update(dish2);
                var count = context.Delete(dish2);
            }
            var s = context.Query<Dish>().Select(d => new { Name = d.DishName, No = d.DishNo }).ToList();
            try
            {
                var x = context.Query<Dish>().Where(d => d.DishNo.Contains("01")).Select(a => new { Count = AggregateFunctions.Count(), Max = AggregateFunctions.Max(a.Build) }).FirstOrDefault();
            }
            catch (Exception ex)
            {

            }

        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? BirthDay { get; set; }
    }

    public class DbCommandInterceptor : IDbCommandInterceptor
    {
        public void ReaderExecuting(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            //interceptionContext.DataBag.Add("startTime", DateTime.Now);
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
            AmendParameter(command);
        }
        public void ReaderExecuted(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            //DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
            //Console.WriteLine(DateTime.Now.Subtract(startTime).TotalMilliseconds);
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result.FieldCount);
        }

        public void NonQueryExecuting(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
            AmendParameter(command);
        }
        public void NonQueryExecuted(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result);
        }

        public void ScalarExecuting(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            //interceptionContext.DataBag.Add("startTime", DateTime.Now);
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
            AmendParameter(command);
        }
        public void ScalarExecuted(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            //DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
            //Console.WriteLine(DateTime.Now.Subtract(startTime).TotalMilliseconds);
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result);
        }


        static void AmendParameter(IDbCommand command)
        {
            foreach (var parameter in command.Parameters)
            {
                //if (parameter is OracleParameter)
                //{
                //    OracleParameter oracleParameter = (OracleParameter)parameter;
                //    if (oracleParameter.Value is string)
                //    {
                //        /* 针对 oracle 长文本做处理 */
                //        string value = (string)oracleParameter.Value;
                //        if (value != null && value.Length > 4000)
                //        {
                //            oracleParameter.OracleDbType = OracleDbType.NClob;
                //        }
                //    }
                //}
            }
        }

        public static string AppendDbCommandInfo(IDbCommand command)
        {
            StringBuilder sb = new StringBuilder();

            foreach (IDbDataParameter param in command.Parameters)
            {
                if (param == null)
                    continue;

                object value = null;
                if (param.Value == null || param.Value == DBNull.Value)
                {
                    value = "NULL";
                }
                else
                {
                    value = param.Value;

                    if (param.DbType == DbType.String || param.DbType == DbType.AnsiString || param.DbType == DbType.DateTime)
                        value = "'" + value + "'";
                }

                sb.AppendFormat("{3} {0} {1} = {2};", Enum.GetName(typeof(DbType), param.DbType), param.ParameterName, value, Enum.GetName(typeof(ParameterDirection), param.Direction));
                sb.AppendLine();
            }

            sb.AppendLine(command.CommandText);

            return sb.ToString();
        }
    }
}
