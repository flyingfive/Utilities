using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using MyDBAssistant.Data;

namespace MyDBAssistant.Schema
{
    public class DatabaseSchemaHistory
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string DeveloperId { get; set; }
        public DateTime RecordDate { get; set; }
        public string ClientIp { get; set; }
        public string SqlScript { get; set; }
        public string Remark { get; set; }

        public bool Insert(MsSqlHelper helper)
        {
            bool hasLogTable = Convert.ToInt32(helper.ExecuteQueryAsSingle("SELECT ISNULL(COUNT(0), 0) FROM sysobjects WHERE name= 'sys_DatabaseSchemaHistory'", CommandType.Text)) > 0;
            if (!hasLogTable)
            {
                helper.ExecuteNonQuery(CREATE_SCHEMA_TABLE_SQL, CommandType.Text);
            }
            var address = System.Net.Dns.GetHostAddresses("localhost").FirstOrDefault();
            var ip = address == null ? "127.0.0.1" : address.ToString();
            string insertLogSql = @"INSERT INTO [dbo].[sys_DatabaseSchemaHistory]
               ([Id]
               ,[Description]
               ,[DeveloperId]
               ,[RecordDate]
               ,[ClientIp]
               ,[SqlScript]
               ,[Remark])
         VALUES
               (@Id,
                @Description,
                @DeveloperId,
                @RecordDate, 
                @ClientIp, 
                @SqlScript,
                @Remark)";
            DbParameter[] paras = new DbParameter[] { 
                new SqlParameter("@Id", SequentialGUID(Guid.NewGuid())),
                new SqlParameter("@Description", Description),
                new SqlParameter("@DeveloperId", "admin"),
                new SqlParameter("@RecordDate", DateTime.Now),
                new SqlParameter("@ClientIp", ip),
                new SqlParameter("@SqlScript", SqlScript),
                new SqlParameter("@Remark", DBNull.Value)
            };
            int count = helper.ExecuteNonQuery(insertLogSql, CommandType.Text, paras);
            return count > 0;
        }

        private Guid SequentialGUID(Guid guid)
        {
            if (guid == Guid.Empty) { guid = Guid.NewGuid(); }
            byte[] guidArray = guid.ToByteArray();
            var baseDate = new DateTime(1900, 1, 1);
            DateTime now = DateTime.Now;
            var days = new TimeSpan(now.Ticks - baseDate.Ticks);
            TimeSpan msecs = now.TimeOfDay;
            byte[] daysArray = BitConverter.GetBytes(days.Days);
            byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);
            Guid newId = new Guid(guidArray);
            return newId;
        }

        /*
         SELECT * FROM systypes WHERE xtype = (SELECT xtype FROM systypes WHERE name = 'u_trans_no') AND uid <>1
             SELECT uid, * from systypes where xtype<>xusertype
             select * from sys.types where is_user_defined=1
         */
        public const String CREATE_SCHEMA_TABLE_SQL = @"
            CREATE TABLE sys_DatabaseSchemaHistory
            (
		        Id UNIQUEIDENTIFIER PRIMARY KEY,
		        [Description] VARCHAR(100) NOT NULL,
		        DeveloperId VARCHAR(40) NOT NULL,
		        RecordDate DATETIME NOT NULL,
		        ClientIp VARCHAR(20),
		        SqlScript TEXT,
		        Remark VARCHAR(200)
            )";
    }
}
