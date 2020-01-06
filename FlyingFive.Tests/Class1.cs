using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var flag =  x.IsDoubleByte();
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

        public void Test2()
        {
        }
    }
}
