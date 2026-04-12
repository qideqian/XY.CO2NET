using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace XY.Encrypt
{
    /// <summary>
    /// 可逆对称加密  密钥长度8
    /// </summary>
    public class DesEncrypt
    {
        //8位长度
        private const string KEY = "yunedu123";
        private static readonly byte[] key = ASCIIEncoding.ASCII.GetBytes(KEY.Substring(0, 8));
        private static readonly byte[] iv = ASCIIEncoding.ASCII.GetBytes(KEY.Insert(0, "w").Substring(0, 8));

        /// <summary>
        /// DES 加密
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        [Obsolete("DES 已不建议用于安全敏感场景，建议改用 AES。")]
        public static string Encrypt(string strValue)
        {
            strValue ??= string.Empty;

            using var dsp = DES.Create();
            using var memStream = new MemoryStream();
            using var crypStream = new CryptoStream(memStream, dsp.CreateEncryptor(key, iv), CryptoStreamMode.Write);
            using var sWriter = new StreamWriter(crypStream);

            sWriter.Write(strValue);
            sWriter.Flush();
            crypStream.FlushFinalBlock();
            memStream.Flush();
            return Convert.ToBase64String(memStream.GetBuffer(), 0, (int)memStream.Length);
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="EncValue"></param>
        /// <returns></returns>
        [Obsolete("DES 已不建议用于安全敏感场景，建议改用 AES。")]
        public static string Decrypt(string EncValue)
        {
            if (string.IsNullOrWhiteSpace(EncValue)) return string.Empty;

            using var dsp = DES.Create();
            byte[] buffer = Convert.FromBase64String(EncValue);
            using var memStream = new MemoryStream();
            using var crypStream = new CryptoStream(memStream, dsp.CreateDecryptor(key, iv), CryptoStreamMode.Write);

            crypStream.Write(buffer, 0, buffer.Length);
            crypStream.FlushFinalBlock();
            return ASCIIEncoding.UTF8.GetString(memStream.ToArray());
        }
    }
}
