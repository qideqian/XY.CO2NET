using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using XY.CO2NET.Helpers;
#if !NET45
using Microsoft.Extensions.DependencyInjection;
#endif
using System.Text.RegularExpressions;

namespace XY.CO2NET.HttpUtility
{
    /// <summary>
    /// Get 请求处理
    /// </summary>
    public static class Get
    {
        /// <summary>
        /// 获取随机文件名
        /// </summary>
        /// <returns></returns>
        private static string GetRandomFileName()
        {
            return SystemTime.Now.ToString("yyyyMMdd-HHmmss") + Guid.NewGuid().ToString("n").Substring(0, 6);
        }

        #region 同步方法

        /// <summary>
        /// GET方式请求URL，并返回T类型
        /// </summary>
        /// <typeparam name="T">接收JSON的数据类型</typeparam>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <returns></returns>
        public static T GetJson<T>(
            IServiceProvider serviceProvider,
            string url, Encoding encoding = null, Action<string, string> afterReturnText = null)
        {
            string returnText = RequestUtility.HttpGet(
                 serviceProvider,
                 url, encoding);
            afterReturnText?.Invoke(url, returnText);
            T result = SerializerHelper.GetObject<T>(returnText);
            return result;
        }

        /// <summary>
        /// 从Url下载
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="stream"></param>
        public static void Download(
            IServiceProvider serviceProvider,
            string url, Stream stream)
        {
#if NET45
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            WebClient wc = new WebClient();
            var data = wc.DownloadData(url);
            stream.Write(data, 0, data.Length);
            //foreach (var b in data)
            //{
            //    stream.WriteByte(b);
            //}
#else
            HttpClient httpClient = serviceProvider.GetRequiredService<XYHttpClient>().Client;
            var t = httpClient.GetByteArrayAsync(url);
            t.Wait();
            var data = t.Result;
            stream.Write(data, 0, data.Length);
#endif
        }

        /// <summary>
        /// 从Url下载，并保存到指定目录
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url">需要下载文件的Url</param>
        /// <param name="filePathName">保存文件的路径，如果下载文件包含文件名，按照文件名储存，否则将分配Ticks随机文件名</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns></returns>
        public static string Download(IServiceProvider serviceProvider, string url, string filePathName, int timeOut = 999)
        {
            var dir = Path.GetDirectoryName(filePathName) ?? "/";
            Directory.CreateDirectory(dir);
#if NET45
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = timeOut;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (Stream responseStream = response.GetResponseStream())
            {
                string responseFileName = null;
                //如：content-disposition: inline; filename="WeChatSampleBuilder-2.0.0.zip"; filename*=utf-8''WeChatSampleBuilder-2.0.0.zip
                var contentDescriptionHeader = response.GetResponseHeader("Content-Disposition");

                if (!string.IsNullOrEmpty(contentDescriptionHeader))
                {
                    var fileName = Regex.Match(contentDescriptionHeader, @"(?<=filename="")([\s\S]+)(?="")", RegexOptions.IgnoreCase).Value;
                    responseFileName = Path.Combine(dir, fileName);
                }
                var fullName = responseFileName ?? Path.Combine(dir, GetRandomFileName());
                using (var fs = File.Open(fullName, FileMode.OpenOrCreate))
                {
                    byte[] bArr = new byte[1024];
                    int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                    while (size > 0)
                    {
                        fs.Write(bArr, 0, size);
                        fs.Flush();
                        size = responseStream.Read(bArr, 0, (int)bArr.Length);
                    }
                }
                return fullName;
            }
#else
            HttpClient httpClient = serviceProvider.GetRequiredService<XYHttpClient>().Client;
            using (var responseMessage = httpClient.GetAsync(url).Result)
            {
                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    string responseFileName = null;
                    //ContentDisposition可能会为Null
                    if (responseMessage.Content.Headers.ContentDisposition != null &&
                        responseMessage.Content.Headers.ContentDisposition.FileName != null &&
                        responseMessage.Content.Headers.ContentDisposition.FileName != "\"\"")
                    {
                        responseFileName = Path.Combine(dir, responseMessage.Content.Headers.ContentDisposition.FileName.Trim('"'));
                    }

                    var fullName = responseFileName ?? Path.Combine(dir, GetRandomFileName());
                    using (var fs = File.Open(fullName, FileMode.Create))
                    {
                        using (var responseStream = responseMessage.Content.ReadAsStreamAsync().Result)
                        {
                            responseStream.CopyTo(fs);
                            fs.Flush();
                        }
                    }
                    return fullName;

                }
                else
                {
                    return null;
                }
            }
#endif
        }
        #endregion

        #region 异步方法

        /// <summary>
        /// 【异步方法】异步GetJson
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ErrorJsonResultException"></exception>
        public static async Task<T> GetJsonAsync<T>(
            IServiceProvider serviceProvider,
            string url, Encoding encoding = null, Action<string, string> afterReturnText = null)
        {
            string returnText = await RequestUtility.HttpGetAsync(
                 serviceProvider,
                 url, encoding).ConfigureAwait(false);
            afterReturnText?.Invoke(url, returnText);
            T result = SerializerHelper.GetObject<T>(returnText);
            return result;
        }

        /// <summary>
        /// 【异步方法】异步从Url下载
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task DownloadAsync(
            IServiceProvider serviceProvider,
            string url, Stream stream)
        {
#if NET45
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            WebClient wc = new WebClient();
            var data = await wc.DownloadDataTaskAsync(url).ConfigureAwait(false);
            await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            //foreach (var b in data)
            //{
            //    stream.WriteAsync(b);
            //}
#else
            HttpClient httpClient = serviceProvider.GetRequiredService<XYHttpClient>().Client;
            var data = await httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
            await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// 【异步方法】从Url下载，并保存到指定目录
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url">需要下载文件的Url</param>
        /// <param name="filePathName">保存文件的路径，如果下载文件包含文件名，按照文件名储存，否则将分配Ticks随机文件名</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns></returns>
        public static async Task<string> DownloadAsync(
            IServiceProvider serviceProvider,
            string url, string filePathName, int timeOut = Config.TIME_OUT)
        {
            var dir = Path.GetDirectoryName(filePathName) ?? "/";
            Directory.CreateDirectory(dir);
#if NET45
            HttpClient httpClient = new HttpClient();
#else
            HttpClient httpClient = serviceProvider.GetRequiredService<XYHttpClient>().Client;
#endif
            httpClient.Timeout = TimeSpan.FromMilliseconds(timeOut);
            using (var responseMessage = await httpClient.GetAsync(url).ConfigureAwait(false))
            {
                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    string responseFileName = null;
                    //ContentDisposition可能会为Null
                    if (responseMessage.Content.Headers.ContentDisposition != null &&
                        responseMessage.Content.Headers.ContentDisposition.FileName != null &&
                        responseMessage.Content.Headers.ContentDisposition.FileName != "\"\"")
                    {
                        responseFileName = Path.Combine(dir, responseMessage.Content.Headers.ContentDisposition.FileName.Trim('"'));
                    }

                    var fullName = responseFileName ?? Path.Combine(dir, GetRandomFileName());
                    using (var fs = File.Open(fullName, FileMode.Create))
                    {
                        using (var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            await responseStream.CopyToAsync(fs).ConfigureAwait(false);
                            await fs.FlushAsync().ConfigureAwait(false);
                        }
                    }
                    return fullName;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion
    }
}
