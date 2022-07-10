using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XY.CO2NET.Helpers
{
    /// <summary>
    /// 文件帮助类
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// 根据完整文件路径获取FileStream
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static FileStream GetFileStream(string fileName)
        {
            FileStream fileStream = null;
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            return fileStream;
        }

        /// <summary>
        /// 从Url下载文件
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="url"></param>
        /// <param name="fullFilePathAndName"></param>
        public static void DownLoadFileFromUrl(IServiceProvider serviceProvider, string url, string fullFilePathAndName)
        {
            using (FileStream fs = new FileStream(fullFilePathAndName, FileMode.OpenOrCreate))
            {
                HttpUtility.Get.Download(
                    serviceProvider,
                    url, fs);
                fs.Flush(true);
            }
        }

        /// <summary>
        /// 判断文件是否正在被使用
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static bool FileInUse(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) // The path might also be invalid.
                {
                    return false;
                }

                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// 如果目录不存在，则创建目录
        /// </summary>
        /// <param name="dir">目录绝对路径</param>
        public static void TryCreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        #region 文件指纹

        /// <summary>
        /// 获取文件的 HASH 值
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="type">SHA1 或 MD5，必须为大写</param>
        /// <param name="toUpper">是否返回大写结果，true：大写，false：小写</param>
        /// <param name="encoding">默认为：utf8</param>
        public static string GetFileHash(string filePath, string type = "SHA1", bool toUpper = true)
        {
            using var stream = GetFileStream(filePath);
            try
            {
                return GetFileHash(stream, type, toUpper);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                stream.Close();
            }
        }

        /// <summary>
        /// 获取文件的 HASH 值
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type">SHA1 或 MD5 或 CRC32，必须为大写</param>
        /// <param name="toUpper">是否返回大写结果，true：大写，false：小写</param>
        public static string GetFileHash(Stream stream, string type = "SHA1", bool toUpper = true)
        {
            switch (type)
            {
                case "SHA1":
                    {
                        return EncryptHelper.GetSha1(stream, toUpper);
                    }
                case "MD5":
                    {
                        return EncryptHelper.GetMD5(stream, toUpper);
                    }
                case "CRC32":
                    {
                        return EncryptHelper.GetCrc32(stream, toUpper);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
        #endregion
    }
}
