using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlyingFive.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlyingFive.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test()
        {
            var x = 'Ｃ';
            var flag = x.IsDoubleByte();
            if (flag)
            {
                var a = x.ToDBCChar();
            }
            x = '你';
            flag = x.IsDoubleByte();
            if (flag)
            {
                flag = x.IsChinese();
                var b = x.ToDBCChar();
            }
            x = '。';
            flag = x.IsDoubleByte();
            if (flag)
            {
                var t = x.IsChinese();
                var b = x.ToDBCChar();
            }
            x = '．';
            flag = x.IsDoubleByte();
            if (flag)
            {
                var b = x.ToDBCChar();
            }
            Assert.AreEqual(flag, true);

            flag = 'C'.IsDoubleByte();

        }

        [TestMethod]
        public void Test2()
        {
            var xml = SerializationHelper.SerializeObjectToXml(new Model() { FFullName = "test", FItemClassID = 12, FItemID = 10658, FModifyTime = new byte[] { 0x26, 0x00, 0x59, 0x04, 0x26, 0x00, 0x59, 0x04 }, FName = "刘碧清", FNumber = "2.3.1.5", UUID = Guid.NewGuid() });
        }
    }
}
