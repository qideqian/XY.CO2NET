using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using XY.CO2NET.Exceptions;

namespace XY.CO2NET.HttpUtility
{
    /// <summary>
    /// HttpClient 相关帮助类
    /// </summary>
    public static class HttpClientHelper
    {
        internal const string DEFAULT_CONTENT_TYPE = "text/xml";

        /// <summary>
        /// 获取 Content
        /// </summary>
        /// <param name="formData">提交表单字段</param>
        /// <returns></returns>
        internal static string GetContentType(Dictionary<string, string> formData)
        {
            string contentType = DEFAULT_CONTENT_TYPE;
            if (formData != null && formData.Count > 0)
            {
                contentType = "application/x-www-form-urlencoded";//如果需要提交表单，则使用特定的ContentType
            }
            return contentType;
        }

#if !NET48
        /// <summary>
        /// 获取 HttpClientHandler 对象
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <param name="webProxy"></param>
        /// <returns></returns>
        public static HttpClientHandler GetHttpClientHandler(CookieContainer cookieContainer = null, IWebProxy webProxy = null, DecompressionMethods decompressionMethods = DecompressionMethods.None)
        {
            var httpClientHandler = new HttpClientHandler()
            {
                UseProxy = webProxy != null,
                Proxy = webProxy,
                UseCookies = cookieContainer != null,
                //CookieContainer = cookieContainer,//如果为null，赋值的时候会出现异常
                AutomaticDecompression = decompressionMethods
            };

            if (cookieContainer != null)
            {
                httpClientHandler.CookieContainer = cookieContainer;
            }
            return httpClientHandler;
        }

        /// <summary>
        /// 从 Response 中设置 Cookie 到 CookieContainer
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <param name="response"></param>
        public static void SetResponseCookieContainer(CookieContainer cookieContainer, HttpResponseMessage response)
        {
            if (cookieContainer == null || response == null)
            {
                return;
            }

            //收集Cookie
            try
            {
                IEnumerable<string> setCookieHeaders = null;
                if (cookieContainer != null && response.Headers != null &&
                    response.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.SetCookie) && response.Headers.TryGetValues(Microsoft.Net.Http.Headers.HeaderNames.SetCookie, out setCookieHeaders))
                {
                    if (setCookieHeaders == null)
                    {
                        throw new Exception("setCookieHeaders is null");
                    }

                    foreach (var header in setCookieHeaders)
                    {
                        //Console.WriteLine("Header:" + header.ToJson());
                        cookieContainer.SetCookies(response.RequestMessage.RequestUri, header);
                    }
                }
            }
            catch (Exception ex)
            {
                //理论上这里不应该抛出异常
                _ = new HttpException($"SetResponseCookieContainer 过程失败！{ex.Message}", ex);
            }
        }
#endif
    }
}
