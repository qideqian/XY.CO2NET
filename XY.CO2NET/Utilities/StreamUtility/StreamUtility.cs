using System;
using System.IO;
using System.Threading.Tasks;

namespace XY.CO2NET.Utilities
{
    /// <summary>
    /// 流工具类
    /// </summary>
    public static class StreamUtility
    {
        #region 同步方法
        /// <summary>
        /// 从流起始位置读取至结束并返回 Base64 字符串；不关闭输入流。
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string GetBase64String(Stream stream)
        {
            stream.Position = 0;
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var arr = ms.ToArray();
#if net48
            return Convert.ToBase64String(arr, Base64FormattingOptions.None);
#else
            return Convert.ToBase64String(arr);
#endif
        }

        /// <summary>
        /// 将 Base64 解码到内存流，可选保存到文件；返回流的位置为 0，由调用方释放。
        /// </summary>
        /// <param name="base64String"></param>
        /// <param name="savePath">为 null 或空字符串时不保存；否则从文件起始位置写入，不截断已有文件。</param>
        /// <returns></returns>
        public static Stream GetStreamFromBase64String(string base64String, string savePath)
        {
            byte[] bytes = Convert.FromBase64String(base64String);

            var memoryStream = new MemoryStream(bytes, 0, bytes.Length);
            memoryStream.Write(bytes, 0, bytes.Length);

            if (!string.IsNullOrEmpty(savePath))
            {
                SaveFileFromStream(memoryStream, savePath);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        /// <summary>
        /// 从文件起始位置写入 memoryStream 的全部内容；创建不存在的文件，但不截断已有文件，较长旧文件的尾部会保留。
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <param name="savePath"></param>
        public static void SaveFileFromStream(MemoryStream memoryStream, string savePath)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            using (var localFile = new FileStream(savePath, FileMode.OpenOrCreate))
            {
                localFile.Write(memoryStream.ToArray(), 0, (int)memoryStream.Length);
            }
        }
        #endregion

        #region 异步方法
        /// <summary>
        /// 【异步方法】从流起始位置读取至结束并返回 Base64 字符串；不关闭输入流。
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<string> GetBase64StringAsync(Stream stream)
        {
            stream.Position = 0;
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);
            var arr = ms.ToArray();
#if net48
            return Convert.ToBase64String(arr, Base64FormattingOptions.None);
#else
            return Convert.ToBase64String(arr);
#endif
        }

        /// <summary>
        /// 【异步方法】将 Base64 解码到内存流，可选保存到文件；返回流的位置为 0，由调用方释放。
        /// </summary>
        /// <param name="base64String"></param>
        /// <param name="savePath">为 null 或空字符串时不保存；否则从文件起始位置写入，不截断已有文件。</param>
        /// <returns></returns>
        public static async Task<Stream> GetStreamFromBase64StringAsync(string base64String, string savePath)
        {
            byte[] bytes = Convert.FromBase64String(base64String);

            var memoryStream = new MemoryStream(bytes, 0, bytes.Length);
            await memoryStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(savePath))
            {
                await SaveFileFromStreamAsync(memoryStream, savePath).ConfigureAwait(false);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        /// <summary>
        /// 【异步方法】从文件起始位置写入 memoryStream 的全部内容；创建不存在的文件，但不截断已有文件，较长旧文件的尾部会保留。
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <param name="savePath"></param>
        public static async Task SaveFileFromStreamAsync(MemoryStream memoryStream, string savePath)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            using (var localFile = new FileStream(savePath, FileMode.OpenOrCreate))
            {
                await localFile.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length).ConfigureAwait(false);
            }
        }
        #endregion
    }
}
