using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using XY.CO2NET.Helpers;
using System.Net.Http;

#if NET45
using System.Security.Cryptography.X509Certificates;
#else
using Microsoft.Extensions.DependencyInjection;
#endif

namespace XY.CO2NET.HttpUtility
{
    /// <summary>
    /// Post 请求处理
    /// </summary>
    public static class Post
    {
        #region 同步方法

        /// <summary>
        /// 发起Post请求，可上传文件
        /// </summary>
        /// <typeparam name="T">返回数据类型（Json对应的实体）</typeparam>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url">请求Url</param>
        /// <param name="cookieContainer">CookieContainer，如果不需要则设为null</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <param name="fileDictionary">需要Post的文件（Dictionary 的 Key=name，Value=绝对路径）</param>
        /// <param name="postDataDictionary">需要Post的键值对（name,value）</param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <returns></returns>
        public static T PostFileGetJson<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> fileDictionary = null,
            Dictionary<string, string> postDataDictionary = null,
            Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false,
            Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                postDataDictionary.FillFormDataStream(ms); //填充formData

                string returnText = RequestUtility.HttpPost(
                    serviceProvider,
                    url, cookieContainer, ms, fileDictionary, null, encoding,
#if !NET45
                    certName,
#else
                    cer,
#endif
                    useAjax, null, timeOut);

                afterReturnText?.Invoke(url, returnText);

                var result = SerializerHelper.GetObject<T>(returnText);
                return result;
            }
        }

        /// <summary>
        /// 发起Post请求，可包含文件流
        /// </summary>
        /// <typeparam name="T">返回数据类型（Json对应的实体）</typeparam>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url">请求Url</param>
        /// <param name="cookieContainer">CookieContainer，如果不需要则设为null</param>
        /// <param name="fileStream">文件流</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax">是否使用Ajax请求</param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <returns></returns>
        public static T PostGetJson<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream fileStream = null, Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, bool checkValidationResult = false, Action<string, string> afterReturnText = null,
            int timeOut = Config.TIME_OUT)
        {
            string returnText = RequestUtility.HttpPost(
                serviceProvider,
                url, cookieContainer, fileStream, null, null, encoding,
#if !NET45
                certName,
#else
                cer,
#endif
                useAjax, null, timeOut, checkValidationResult);

            //XYTrace.SendApiLog(url, returnText);
            afterReturnText?.Invoke(url, returnText);

            var result = SerializerHelper.GetObject<T>(returnText);
            return result;
        }

        /// <summary>
        /// Form表单Post数据，获取JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer">CookieContainer，如果不需要则设为null</param>
        /// <param name="formData">表单数据，Key对应name，Value对应value</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax">是否使用Ajax请求</param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <returns></returns>
        public static T PostGetJson<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> formData = null, Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            string returnText = RequestUtility.HttpPost(
                serviceProvider,
                url, cookieContainer, formData, encoding,
#if !NET45
                certName,
#else
                cer,
#endif
                useAjax, null, timeOut);
            //XYTrace.SendApiLog(url, returnText);
            afterReturnText?.Invoke(url, returnText);

            var result = SerializerHelper.GetObject<T>(returnText);
            return result;
        }

        /// <summary>
        /// 使用Post方法上传数据并下载文件或结果
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        public static void Download(
            IServiceProvider serviceProvider,
            string url, string data, Stream stream)
        {
#if NET45
            WebClient wc = new WebClient();
            var file = wc.UploadData(url, "POST", Encoding.UTF8.GetBytes(string.IsNullOrEmpty(data) ? "" : data));
            stream.Write(file, 0, file.Length);

            //foreach (var b in file)
            //{
            //    stream.WriteByte(b);
            //}
#else
            HttpClient httpClient = serviceProvider.GetRequiredService<XYHttpClient>().Client;
            HttpContent hc = new StringContent(data);
            var ht = httpClient.PostAsync(url, hc);
            ht.Wait();
            var ft = ht.Result.Content.ReadAsByteArrayAsync();
            ft.Wait();
            var file = ft.Result;
            stream.Write(file, 0, file.Length);
#endif
        }

        #endregion

        #region 异步方法
        /// <summary>
        /// 【异步方法】发起Post请求，可上传文件
        /// </summary>
        /// <typeparam name="T">返回数据类型（Json对应的实体）</typeparam>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url">请求Url</param>
        /// <param name="cookieContainer">CookieContainer，如果不需要则设为null</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <param name="fileDictionary">需要Post的文件（Dictionary 的 Key=name，Value=绝对路径）</param>
        /// <param name="postDataDictionary">需要Post的键值对（name,value）</param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <returns></returns>
        public static async Task<T> PostFileGetJsonAsync<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> fileDictionary = null, Dictionary<string, string> postDataDictionary = null,
            Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false,
            Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                postDataDictionary.FillFormDataStream(ms); //填充formData

                string returnText = await RequestUtility.HttpPostAsync(
                    serviceProvider,
                    url, cookieContainer, ms, fileDictionary, null, encoding,
#if !NET45
                    certName,
#else
                    cer,
#endif
                    useAjax, null, timeOut).ConfigureAwait(false);
                afterReturnText?.Invoke(url, returnText);
                var result = SerializerHelper.GetObject<T>(returnText);
                return result;
            }
        }

        /// <summary>
        /// 【异步方法】发起Post请求，可包含文件流
        /// </summary>
        /// <typeparam name="T">返回数据类型（Json对应的实体）</typeparam>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url">请求Url</param>
        /// <param name="cookieContainer">CookieContainer，如果不需要则设为null</param>
        /// <param name="fileStream">文件流</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax">是否使用Ajax请求</param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <returns></returns>
        public static async Task<T> PostGetJsonAsync<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream fileStream = null, Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, bool checkValidationResult = false, Action<string, string> afterReturnText = null,
            int timeOut = Config.TIME_OUT)
        {
            string returnText = await RequestUtility.HttpPostAsync(
                serviceProvider,
                url, cookieContainer, fileStream, null, null, encoding,
#if !NET45
                certName,
#else
                cer,
#endif
                useAjax, null, timeOut, checkValidationResult).ConfigureAwait(false);

            //XYTrace.SendApiLog(url, returnText);
            afterReturnText?.Invoke(url, returnText);

            var result = SerializerHelper.GetObject<T>(returnText);
            return result;
        }

        /// <summary>
        /// 【异步方法】Form表单Post数据，获取JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer">CookieContainer，如果不需要则设为null</param>
        /// <param name="formData">表单数据，Key对应name，Value对应value</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax">是否使用Ajax请求</param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <returns></returns>
        public static async Task<T> PostGetJsonAsync<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> formData = null, Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            string returnText = await RequestUtility.HttpPostAsync(
                serviceProvider,
                url, cookieContainer, formData, encoding,
#if !NET45
                certName,
#else
                cer,
#endif
                useAjax, null, timeOut).ConfigureAwait(false);
            //XYTrace.SendApiLog(url, returnText);
            afterReturnText?.Invoke(url, returnText);
            var result = SerializerHelper.GetObject<T>(returnText);
            return result;
        }

        /// <summary>
        /// 【异步方法】使用Post方法上传数据并下载文件或结果
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        public static async Task DownloadAsync(
            IServiceProvider serviceProvider,
            string url, string data, Stream stream)
        {
#if NET45
            WebClient wc = new WebClient();
            var fileBytes = await wc.UploadDataTaskAsync(url, "POST", Encoding.UTF8.GetBytes(string.IsNullOrEmpty(data) ? "" : data)).ConfigureAwait(false);
            await stream.WriteAsync(fileBytes, 0, fileBytes.Length).ConfigureAwait(false);//也可以分段写入
#else
            HttpClient httpClient = serviceProvider.GetRequiredService<XYHttpClient>().Client;
            HttpContent hc = new StringContent(data);
            var ht = await httpClient.PostAsync(url, hc).ConfigureAwait(false);
            var fileBytes = await ht.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            await stream.WriteAsync(fileBytes, 0, fileBytes.Length).ConfigureAwait(false);//也可以分段写入
#endif
        }
        #endregion
    }
}
