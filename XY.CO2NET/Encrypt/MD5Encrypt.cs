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

        /// <summary>
        /// Md5加密（小写32位十六进制）。
        /// </summary>
        /// <param name="input">待加密的字符串</param>
        /// <returns>加密后的十六进制字符串</returns>
        public static string Md5Hex(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = ComputeMd5Hash(bytes);
            return ToLowerHex(hash);
        }

        /// <summary>
        /// 双重 Md5 加密（先对原文做 Md5Hex，再对结果做一次 Md5Hex）。
        /// </summary>
        /// <param name="input">待加密的字符串</param>
        /// <returns>双重加密后的十六进制字符串</returns>
        public static string Md5HexDouble(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            return Md5Hex(Md5Hex(input));
        }

        /// <summary>
        /// 计算 MD5 哈希值，按目标框架选择可用实现。
        /// </summary>
        /// <param name="bytes">待哈希字节数组</param>
        /// <returns>MD5 哈希字节数组</returns>
        private static byte[] ComputeMd5Hash(byte[] bytes)
        {
#if NET6_0_OR_GREATER
            return MD5.HashData(bytes);
#else
            using var md5 = MD5.Create();
            return md5.ComputeHash(bytes);
#endif
        }

        /// <summary>
        /// 将字节数组转换为小写十六进制字符串。
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>小写十六进制字符串</returns>
        private static string ToLowerHex(byte[] bytes)
        {
#if NET6_0_OR_GREATER
            return Convert.ToHexString(bytes).ToLowerInvariant();
#else
            var sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }

            return sb.ToString();
#endif
        }
    }
}
