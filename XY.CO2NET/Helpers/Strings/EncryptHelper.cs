using XY.CO2NET.HttpUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;

namespace XY.CO2NET.Helpers
{
    /// <summary>
    /// 安全帮助类，提供SHA-1算法等
    /// </summary>
    public class EncryptHelper
    {
        #region SHA相关
        /// <summary>
        /// 采用SHA-1算法加密字符串（小写）
        /// </summary>
        /// <param name="encypStr">需要加密的字符串</param>
        /// <returns></returns>
        public static string GetSha1(string encypStr)
        {
            var sha1 = SHA1.Create();
            var sha1Arr = sha1.ComputeHash(Encoding.UTF8.GetBytes(encypStr));
            StringBuilder enText = new StringBuilder();
            foreach (var b in sha1Arr)
            {
                enText.AppendFormat("{0:x2}", b);
            }
            return GetSha1(encypStr, false);
        }

        /// <summary>
        /// 采用SHA-1算法加密字符串（默认大写）
        /// </summary>
        /// <param name="encypStr">需要加密的字符串</param>
        /// <param name="toUpper">是否返回大写结果，true：大写，false：小写</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string GetSha1(string encypStr, bool toUpper = true, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var sha1 = SHA1.Create();
            var sha1Arr = sha1.ComputeHash(encoding.GetBytes(encypStr));
            StringBuilder enText = new StringBuilder();
            foreach (var b in sha1Arr)
            {
                enText.AppendFormat("{0:x2}", b);
            }
            if (!toUpper)
            {
                return enText.ToString();
            }
            else
            {
                return enText.ToString().ToUpper();
            }
        }

        /// <summary>
        /// 采用SHA-1算法加密流（默认大写）
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="toUpper">是否返回大写结果，true：大写，false：小写</param>
        /// <returns></returns>
        public static string GetSha1(Stream stream, bool toUpper = true)
        {
            stream.Seek(0, SeekOrigin.Begin);

            var sha1 = SHA1.Create();
            var sha1Arr = sha1.ComputeHash(stream);
            StringBuilder enText = new StringBuilder();
            foreach (var b in sha1Arr)
            {
                enText.AppendFormat("{0:x2}", b);
            }

            var result = enText.ToString();
            if (!toUpper)
            {
                return result;
            }
            else
            {
                return result.ToUpper();
            }
        }

        /// <summary>
        /// HMAC SHA256 加密
        /// </summary>
        /// <param name="message">加密消息原文。当为小程序SessionKey签名提供服务时，其中message为本次POST请求的数据包（通常为JSON）。特别地，对于GET请求，message等于长度为0的字符串。</param>
        /// <param name="secret">秘钥（如小程序的SessionKey）</param>
        /// <returns></returns>
        public static string GetHmacSha256(string message, string secret)
        {
            message = message ?? "";
            secret = secret ?? "";
            byte[] keyByte = Encoding.UTF8.GetBytes(secret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                StringBuilder enText = new StringBuilder();
                foreach (var b in hashmessage)
                {
                    enText.AppendFormat("{0:x2}", b);
                }
                return enText.ToString();
            }
        }
        #endregion

        #region MD5
        /// <summary>
        /// 获取大写的MD5签名结果
        /// </summary>
        /// <param name="encypStr">需要加密的字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string GetMD5(string encypStr, Encoding encoding)
        {
            string retStr;

#if NET48
            MD5CryptoServiceProvider m5 = new MD5CryptoServiceProvider();
#else
            MD5 m5 = MD5.Create();
#endif

            //创建md5对象
            byte[] inputBye;
            byte[] outputBye;

            //使用指定编码方式把字符串转化为字节数组．
            try
            {
                inputBye = encoding.GetBytes(encypStr);
            }
            catch
            {
                inputBye = Encoding.GetEncoding("utf-8").GetBytes(encypStr);
            }
            outputBye = m5.ComputeHash(inputBye);

            retStr = BitConverter.ToString(outputBye);
            retStr = retStr.Replace("-", "").ToUpper();
            return retStr;
        }

        /// <summary>
        /// 获取大写的MD5签名结果
        /// </summary>
        /// <param name="encypStr">需要加密的字符串</param>
        /// <param name="charset">编码</param>
        /// <returns></returns>
        public static string GetMD5(string encypStr, string charset = "utf-8")
        {
            charset = charset ?? "utf-8";
            try
            {
                //使用指定编码
                return GetMD5(encypStr, Encoding.GetEncoding(charset));
            }
            catch
            {
                //使用UTF-8编码
                return GetMD5("utf-8", Encoding.GetEncoding(charset));
            }
        }

        /// <summary>
        /// 获取MD5签名结果
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="toUpper">是否返回大写结果，true：大写，false：小写</param>
        /// <returns></returns>
        public static string GetMD5(Stream stream, bool toUpper = true)
        {
            stream.Position = 0;
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] ret = md5.ComputeHash(stream);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ret.Length; i++)
            {
                sb.Append(ret[i].ToString(toUpper ? "X2" : "x2"));
            }
            string md5str = sb.ToString();
            return md5str;
        }

        /// <summary>
        /// 获取小写的MD5签名结果
        /// </summary>
        /// <param name="encypStr">需要加密的字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string GetLowerMD5(string encypStr, Encoding encoding)
        {
            return GetMD5(encypStr, encoding).ToLower();
        }

        #endregion

        #region CRC32

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encypStr">需要加密的字符串</param>
        /// <param name="toUpper">是否返回大写结果，true：大写，false：小写</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GetCrc32(string encypStr, bool toUpper = true, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            Crc32 calculator = new Crc32();
            byte[] buffer = calculator.ComputeHash(encoding.GetBytes(encypStr));
            calculator.Clear();
            //将字节数组转换成十六进制的字符串形式
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                sb.Append(buffer[i].ToString("x2"));
            }
            return toUpper ? toUpper.ToString().ToUpper() : sb.ToString();
        }

        /// <summary>
        /// 获取 CRC32 加密字符串
        /// </summary>
        /// <param name="encypStr">需要加密的字符串</param>
        /// <param name="toUpper">是否返回大写结果，true：大写，false：小写</param>
        /// <returns></returns>
        public static string GetCrc32(Stream stream, bool toUpper = true)
        {
            Crc32 calculator = new Crc32();
            byte[] buffer = calculator.ComputeHash(stream);
            calculator.Clear();
            //将字节数组转换成十六进制的字符串形式
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                sb.Append(buffer[i].ToString("x2"));
            }
            return toUpper ? toUpper.ToString().ToUpper() : sb.ToString();
        }

        #endregion

        #region AES - CBC
        /// <summary>
        /// AES加密（默认为CBC模式）
        /// </summary>
        /// <param name="inputdata">输入的数据</param>
        /// <param name="iv">向量</param>
        /// <param name="strKey">加密密钥</param>
        /// <returns></returns>
        public static byte[] AESEncrypt(byte[] inputdata, byte[] iv, string strKey)
        {
            //分组加密算法
#if NET48
            SymmetricAlgorithm des = Rijndael.Create();
#else
            SymmetricAlgorithm des = Aes.Create();
#endif
            byte[] inputByteArray = inputdata;//得到需要加密的字节数组
                                              //设置密钥及密钥向量
            des.Key = Encoding.UTF8.GetBytes(strKey.PadRight(32));
            des.IV = iv;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    byte[] cipherBytes = ms.ToArray();//得到加密后的字节数组
                    return cipherBytes;
                }
            }
        }

        /// <summary>
        /// AES解密（默认为CBC模式）
        /// </summary>
        /// <param name="inputdata">输入的数据</param>
        /// <param name="iv">向量</param>
        /// <param name="strKey">key</param>
        /// <returns></returns>
        public static byte[] AESDecrypt(byte[] inputdata, byte[] iv, string strKey)
        {
#if NET48
            SymmetricAlgorithm des = Rijndael.Create();
#else
            SymmetricAlgorithm des = Aes.Create();
#endif
            des.Key = Encoding.UTF8.GetBytes(strKey.PadRight(32));
            des.IV = iv;
            byte[] decryptBytes = null;// new byte[inputdata.Length];
            using (MemoryStream ms = new MemoryStream(inputdata))
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (MemoryStream originalMemory = new MemoryStream())
                    {
                        Byte[] Buffer = new Byte[1024];
                        Int32 readBytes = 0;
                        while ((readBytes = cs.Read(Buffer, 0, Buffer.Length)) > 0)
                        {
                            originalMemory.Write(Buffer, 0, readBytes);
                        }

                        decryptBytes = originalMemory.ToArray();
                    }
                }
            }
            return decryptBytes;
        }
        #endregion

        #region AES - CEB

        /// <summary>
        ///  AES 加密（无向量，CEB模式，秘钥长度=128）
        /// </summary>
        /// <param name="str">明文（待加密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        public static string AESEncrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);

            RijndaelManaged rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key.PadRight(32)),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// AES 解密（无向量，CEB模式，秘钥长度=128）
        /// </summary>
        /// <param name="data">被加密的明文（注意：为Base64编码）</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static string AESDecrypt(string data, string key)
        {
            byte[] encryptedBytes = Convert.FromBase64String(data);
            byte[] bKey = new byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);

            MemoryStream mStream = new MemoryStream(encryptedBytes);
#if NET48
            SymmetricAlgorithm aes = Rijndael.Create();
#else
            SymmetricAlgorithm aes = Aes.Create();
#endif
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            aes.Key = bKey;
            CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using (var sr = new StreamReader(cryptoStream))
            {
                var str = sr.ReadToEnd();
                return str;
            }
        }
        #endregion
    }
}
