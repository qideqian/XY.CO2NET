using System.Text;

namespace XY.CO2NET.Helpers.Strings.Tests
{
    [TestClass]
    public class EncryptHelperTests
    {
        [TestMethod]
        public void GetCrc32_ShouldReturnHexHash_WhenToUpperTrue()
        {
            var crc32 = EncryptHelper.GetCrc32("abc", true, Encoding.UTF8);

            StringAssert.Matches(crc32, new System.Text.RegularExpressions.Regex("^[A-F0-9]{8}$"));
            Assert.AreEqual("352441C2", crc32);
        }

        [TestMethod]
        public void GetMD5_ShouldFallbackToUtf8_WhenCharsetInvalid()
        {
            const string input = "中文abc";
            var expected = EncryptHelper.GetMD5(input, Encoding.UTF8);

            var actual = EncryptHelper.GetMD5(input, "invalid-charset-name");

            Assert.AreEqual(expected, actual);
        }
    }
}
