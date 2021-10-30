#if !NET45
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace XY.CO2NET.HttpUtility
{
    /// <summary>
    /// XYHttpClient，用于提供 HttpClientFactory 的自定义类
    /// </summary>
    public class XYHttpClient
    {
        /// <summary>
        /// XYHttpClient 构造函数
        /// </summary>
        /// <param name="httpClient"></param>
        public XYHttpClient(HttpClient httpClient)
        {
            //httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            //httpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            Client = httpClient;
        }

        /// <summary>
        /// HttpClient 对象
        /// </summary>
        public HttpClient Client { get; private set; }

        /// <summary>
        /// 从 HttpClientFactory 的唯一名称中获取 HttpClient 对象，并加载到 XYHttpClient 中
        /// </summary>
        /// <param name="httpClientName"></param>
        /// <returns></returns>
        public static XYHttpClient GetInstanceByName(IServiceProvider serviceProvider, string httpClientName)
        {
            if (!string.IsNullOrEmpty(httpClientName))
            {
                var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                var httpClient = clientFactory.CreateClient(httpClientName);
                return new XYHttpClient(httpClient);
            }
            return serviceProvider.GetRequiredService<XYHttpClient>();
        }

        public void SetCookie(Uri uri, CookieContainer cookieContainer)
        {
            if (cookieContainer == null)
            {
                return;
            }
            var cookieHeader = cookieContainer.GetCookieHeader(uri);
            Client.DefaultRequestHeaders.Add(HeaderNames.Cookie, cookieHeader);
        }

        ///// <summary>
        ///// Read web cookies
        ///// </summary>
        //public static CookieContainer ReadCookies(this HttpResponseMessage response)
        //{
        //    var pageUri = response.RequestMessage.RequestUri;
        //    var cookieContainer = new CookieContainer();
        //    IEnumerable<string> cookies;
        //    if (response.Headers.TryGetValues("set-cookie", out cookies))
        //    {
        //        foreach (var c in cookies)
        //        {
        //            cookieContainer.SetCookies(pageUri, c);
        //        }
        //    }
        //    return cookieContainer;
        //}
    }
}
#endif
