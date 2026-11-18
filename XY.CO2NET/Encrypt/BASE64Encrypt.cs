using System;
using System.Text;

namespace XY.Encrypt
{
    /// <summary>
    /// Base64编码
    /// </summary>
    public class Base64Encrypt
    {
        #region Base64
        /// <summary>
        /// 使用 Encoding.Default 将文本转换为字节，再进行 Base64 编码。
        /// </summary>
        /// <param name="source">待编码文本；null 不会被转换为空串。</param>
        /// <returns>编码后的字串</returns>
        public static string Encrypt(string source)
        {
            byte[] bytes = Encoding.Default.GetBytes(source);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 将 Base64 字符串还原为字节，再使用 Encoding.Default 解码为文本。
        /// </summary>
        /// <param name="EncValue">Base64 字符串；null 或无效格式由底层转换方法报告异常。</param>
        /// <returns>使用当前运行环境默认编码得到的文本。</returns>
        public static string Decrypt(string EncValue)
        {
            byte[] outputb = Convert.FromBase64String(EncValue);
            return Encoding.Default.GetString(outputb);
        }

        #endregion Base64
    }
}
