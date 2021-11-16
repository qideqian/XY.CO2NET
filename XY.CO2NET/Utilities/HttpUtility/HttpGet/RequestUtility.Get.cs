using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using XY.CO2NET.Helpers;
#if NET45
using System.Web;
#else
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using XY.CO2NET.WebProxy;
#endif

namespace XY.CO2NET.HttpUtility
{
    /// <summary>
    /// HTTP 请求工具类
    /// </summary>
    public static partial class RequestUtility
    {
        #region 公用静态方法

#if NET45
        /// <summary>
        /// .NET 4.5 版本的HttpWebRequest参数设置
        /// </summary>
        /// <returns></returns>
        private static HttpWebRequest HttpGet_Common_Net45(string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, int timeOut = Config.TIME_OUT)
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = timeOut;
            request.Proxy = _webproxy;
            if (cer != null)
            {
                request.ClientCertificates.Add(cer);
            }
            if (cookieContainer != null)
            {
                request.CookieContainer = cookieContainer;
            }
            HttpClientHeader(request, refererUrl, useAjax, null, timeOut);//设置头信息
            return request;
        }
#endif

#if !NET45
        /// <summary>
        /// .NET Core 版本的HttpWebRequest参数设置
        /// </summary>
        /// <returns></returns>
        private static HttpClient HttpGet_Common_NetCore(IServiceProvider serviceProvider, string url, CookieContainer cookieContainer = null,
            Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT)
        {
            var handler = HttpClientHelper.GetHttpClientHandler(cookieContainer, RequestUtility.XYHttpClientWebProxy, DecompressionMethods.GZip);
            if (cer != null)
            {
                handler.ClientCertificates.Add(cer);
            }
            HttpClient httpClient = serviceProvider.GetRequiredService<XYHttpClient>().Client;
            HttpClientHeader(httpClient, refererUrl, useAjax, headerAddition, timeOut);
            return httpClient;
        }
#endif

        #endregion

        #region 同步方法

        /// <summary>
        /// 使用Get方法获取字符串结果（没有加入Cookie）
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string HttpGet(
            IServiceProvider serviceProvider,
            string url, Encoding encoding = null)
        {
#if NET45
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            WebClient wc = new WebClient();
            wc.Proxy = _webproxy;
            wc.Encoding = encoding ?? Encoding.UTF8;
            return wc.DownloadString(url);
#else
            var handler = HttpClientHelper.GetHttpClientHandler(null, XYHttpClientWebProxy, DecompressionMethods.GZip);
            HttpClient httpClient = serviceProvider.GetRequiredService<XYHttpClient>().Client;
            return httpClient.GetStringAsync(url).Result;
#endif
        }

        /// <summary>
        /// 使用Get方法获取字符串结果（加入Cookie）
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="refererUrl">referer参数</param>
        /// <param name="useAjax">是否使用Ajax</param>
        /// <param name="headerAddition"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static string HttpGet(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT)
        {
#if NET45
            HttpWebRequest request = HttpGet_Common_Net45(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }
            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.GetEncoding("utf-8")))
                {
                    string retString = myStreamReader.ReadToEnd();
                    return retString;
                }
            }
#else
            var httpClient = HttpGet_Common_NetCore(serviceProvider, url, cookieContainer, encoding, cer, refererUrl, useAjax, headerAddition, timeOut);
            var response = httpClient.GetAsync(url).GetAwaiter().GetResult();//获取响应信息
            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
#endif
        }

#if NET45

        /// <summary>
        /// 获取HttpWebResponse或HttpResponseMessage对象，本方法通常用于测试）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static HttpWebResponse HttpResponseGet(string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
    string refererUrl = null, bool useAjax = false, int timeOut = Config.TIME_OUT)
        {
            HttpWebRequest request = HttpGet_Common_Net45(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }
            return response;
        }
#else
        /// <summary>
        /// 获取HttpWebResponse或HttpResponseMessage对象，本方法通常用于测试）
        /// </summary>
        /// <param name="serviceProvider">NetCore的服务器提供程序</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax">是否使用Ajax请求</param>
        /// <param name="headerAddition"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static HttpResponseMessage HttpResponseGet(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
   string refererUrl = null, bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT)
        {
            var httpClient = HttpGet_Common_NetCore(serviceProvider, url, cookieContainer, encoding, cer, refererUrl, useAjax, headerAddition, timeOut);
            var task = httpClient.GetAsync(url);
            HttpResponseMessage response = task.Result;
            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie
            return response;
        }

#endif

        #endregion

        #region 异步方法

        /// <summary>
        /// 使用Get方法获取字符串结果（没有加入Cookie）
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> HttpGetAsync(
            IServiceProvider serviceProvider,
            string url, Encoding encoding = null)
        {
#if NET45
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            WebClient wc = new WebClient();
            wc.Proxy = _webproxy;
            wc.Encoding = encoding ?? Encoding.UTF8;
            return await wc.DownloadStringTaskAsync(url).ConfigureAwait(false);
#else
            var handler = new HttpClientHandler
            {
                UseProxy = XYHttpClientWebProxy != null,
                Proxy = XYHttpClientWebProxy,
            };

            HttpClient httpClient = serviceProvider.GetRequiredService<XYHttpClient>().Client;
            return await httpClient.GetStringAsync(url).ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// 使用Get方法获取字符串结果（加入Cookie）
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="timeOut"></param>
        /// <param name="refererUrl">referer参数</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition"></param>
        /// <returns></returns>
        public static async Task<string> HttpGetAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT)
        {
#if NET45
            HttpWebRequest request = HttpGet_Common_Net45(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);
            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false));
            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }
            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.GetEncoding("utf-8")))
                {
                    string retString = await myStreamReader.ReadToEndAsync().ConfigureAwait(false);
                    return retString;
                }
            }
#else
            var httpClient = HttpGet_Common_NetCore(serviceProvider, url, cookieContainer, encoding, cer, refererUrl, useAjax, headerAddition, timeOut);
            var response = await httpClient.GetAsync(url).ConfigureAwait(false);//获取响应信息
            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie
            var retString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return retString;
#endif
        }

#if NET45
        /// <summary>
        /// 获取HttpWebResponse或HttpResponseMessage对象，本方法通常用于测试）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static async Task<HttpWebResponse> HttpResponseGetAsync(string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
    string refererUrl = null, bool useAjax = false, int timeOut = Config.TIME_OUT)
        {
            HttpWebRequest request = HttpGet_Common_Net45(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);
            HttpWebResponse response =  (HttpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false));
            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }
            return response;
        }
#else
        /// <summary>
        /// 获取HttpWebResponse或HttpResponseMessage对象，本方法通常用于测试）
        /// </summary>
        /// <param name="serviceProvider">NetCore的服务器提供程序</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax">是否使用Ajax请求</param>
        /// <param name="headerAddition"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> HttpResponseGetAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
   string refererUrl = null, bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT)
        {
            var httpClient = HttpGet_Common_NetCore(serviceProvider, url, cookieContainer, encoding, cer, refererUrl, useAjax, headerAddition, timeOut);
            var task = httpClient.GetAsync(url);
            HttpResponseMessage response = await task;
            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie
            return response;
        }
#endif

        #endregion
    }
}
