using XY.Encrypt;

namespace XY.Encrypt.Tests
{
    [TestClass]
    public class SHA1EncryptTests
    {
        [TestMethod]
        public void SHA256_ShouldReturnExpectedUpperHex()
        {
            var actual = SHA1Encrypt.SHA256("abc");
            Assert.AreEqual("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD", actual);
        }
    }
}
