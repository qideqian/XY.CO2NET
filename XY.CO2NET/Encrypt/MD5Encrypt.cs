using System;
using System.Security.Cryptography;
using System.Text;

namespace XY.Encrypt
{
    /// <summary>
    /// 对 UTF-8 文本计算 MD5 哈希，并返回小写十六进制表示。
    /// </summary>
    public class MD5Encrypt
    {
        #region MD5
        /// <summary>
        /// 对 UTF-8 文本计算 MD5 哈希；null 或空串直接返回空串。
        /// </summary>
        /// <param name="source">待计算哈希的文本，不限于字母和数字。</param>
        /// <param name="length">为 16 时取完整哈希十六进制表示的第 9 至 24 个字符；其他值均返回完整的 32 个字符。</param>
        /// <returns>小写十六进制哈希字符串；输入为 null 或空串时返回空串。</returns>
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
                case 16://取32字符十六进制哈希的第9到24个字符
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
        /// 对 UTF-8 文本计算 MD5 哈希（小写 32 字符十六进制）；null 或空串直接返回空串。
        /// </summary>
        /// <param name="input">待计算哈希的文本。</param>
        /// <returns>小写十六进制哈希字符串；输入为 null 或空串时返回空串。</returns>
        public static string Md5Hex(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = ComputeMd5Hash(bytes);
            return ToLowerHex(hash);
        }

        /// <summary>
        /// 先对原文调用 Md5Hex，再对返回的小写十六进制文本调用 Md5Hex；null 或空串直接返回空串。
        /// </summary>
        /// <param name="input">待计算哈希的文本。</param>
        /// <returns>第二次哈希的小写十六进制字符串；输入为 null 或空串时返回空串。</returns>
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
