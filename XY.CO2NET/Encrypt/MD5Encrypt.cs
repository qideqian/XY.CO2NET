using System;
using System.Security.Cryptography;
using System.Text;

namespace XY.Encrypt
{
    /// <summary>
    /// 不可逆加密，限于字母和数字
    /// </summary>
    public class MD5Encrypt
    {
        #region MD5
        /// <summary>
        /// MD5加密,和动网上的16/32位MD5加密结果相同
        /// </summary>
        /// <param name="strSource">待加密字串</param>
        /// <param name="length">16或32值之一,其它则采用.net默认MD5加密算法</param>
        /// <returns>加密后的字串</returns>
        [Obsolete("MD5 已不建议用于安全敏感场景，建议改用 SHA256。")]
        public static string Encrypt(string source, int length = 32)
        {
            if (string.IsNullOrEmpty(source)) return string.Empty;

            using var provider = MD5.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(source);
            byte[] hashValue = provider.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            switch (length)
            {
                case 16://16位密文是32位密文的9到24位字符
                    for (int i = 4; i < 12; i++)
                        sb.Append(hashValue[i].ToString("x2"));
                    break;
                case 32:
                    for (int i = 0; i < 16; i++)
                    {
                        sb.Append(hashValue[i].ToString("x2"));
                    }
                    break;
                default:
                    for (int i = 0; i < hashValue.Length; i++)
                    {
                        sb.Append(hashValue[i].ToString("x2"));
                    }
                    break;
            }
            return sb.ToString();
        }
        #endregion MD5
    }
}
