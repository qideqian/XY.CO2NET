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
        /// Base64编码
        /// </summary>
        /// <param name="strSource">待编码字串</param>
        /// <returns>编码后的字串</returns>
        public static string Encrypt(string source)
        {
            byte[] bytes = Encoding.Default.GetBytes(source);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="EncValue">待解码字串</param>
        /// <returns></returns>
        public static string Decrypt(string EncValue)
        {
            byte[] outputb = Convert.FromBase64String(EncValue);
            return Encoding.Default.GetString(outputb);
        }

        #endregion Base64
    }
}
