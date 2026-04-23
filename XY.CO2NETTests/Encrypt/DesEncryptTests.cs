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