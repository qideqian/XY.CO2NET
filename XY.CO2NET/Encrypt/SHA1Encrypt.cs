using System;
using System.Text;

namespace XY.Encrypt
{
    public class SHA1Encrypt
    {
        /// <summary>
        /// SHA1 加密，返回大写字符串
        /// </summary>
        /// <param name="content">需要加密字符串</param>
        /// <returns>返回40位UTF8 大写</returns>
        [Obsolete("SHA1 已不建议用于安全敏感场景。建议优先使用 SHA256()。")]
        public static string SHA1(string content)
        {
            return SHA1(content, Encoding.UTF8);
        }

        /// <summary>
        /// SHA1 加密，返回大写字符串
        /// </summary>
        /// <param name="content">需要加密字符串</param>
        /// <param name="encode">指定加密编码</param>
        /// <returns>返回40位大写字符串</returns>
        [Obsolete("SHA1 已不建议用于安全敏感场景。建议优先使用 SHA256()。")]
        public static string SHA1(string content, Encoding encode)
        {
            encode ??= Encoding.UTF8;
            content ??= string.Empty;

            using var sha1 = System.Security.Cryptography.SHA1.Create();
            var bytesIn = encode.GetBytes(content);
            var bytesOut = sha1.ComputeHash(bytesIn);
            var result = BitConverter.ToString(bytesOut);
            return result.Replace("-", "");
        }

        public static string SHA256(string data)
        {
            data ??= string.Empty;
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hash = sha256.ComputeHash(bytes);

            StringBuilder builder = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("X2"));
            }

            return builder.ToString();
        }

        [Obsolete("请使用 SHA256()。")]
        public static string sha256(string data)
        {
            return SHA256(data);
        }
    }
}
