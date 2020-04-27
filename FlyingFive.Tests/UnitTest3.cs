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
using System.Numerics;
using System.ComponentModel;
using FlyingFive.Comparing;

namespace FlyingFive.Tests
{
    [TestClass]
    public class UnitTest3
    {
        [TestMethod]
        public void TestSerializationHelper()
        {
            var x = new Model() { FFullName = "liubq", FItemClassID = 12, FItemID = 11243, FModifyTime = new byte[] { 0x3f, 0x58, 0xe2 }, FName = "siss", UUID = Guid.NewGuid(), FNumber = "2.3.6.2.5" };
            var data = SerializationHelper.SerializeObjectToBytes(x);
            var x2 = SerializationHelper.DeserializeObjectFromBytes<Model>(data);
            var base64 = SerializationHelper.SerializeObjectToBase64(x2);
            var x3 = SerializationHelper.DeserializeObjectFromBase64<Model>(base64);
        }
    }
}
