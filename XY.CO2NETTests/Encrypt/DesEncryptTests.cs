using Microsoft.VisualStudio.TestTools.UnitTesting;
using XY.Encrypt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XY.Encrypt.Tests
{
    [TestClass()]
    public class DesEncryptTests
    {
        [TestMethod()]
        public void EncryptTest()
        {
            var aa = DesEncrypt.Encrypt("123");
            var bb = DesEncrypt.Encrypt("123");
            Assert.AreEqual(aa, bb);
        }
    }
}