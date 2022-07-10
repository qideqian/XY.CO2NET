using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using XY.CO2NET.Helpers;

#if !NET48
using System.Net.Http;
using System.Net.Http.Headers;
using XY.CO2NET.Exceptions;
#endif

namespace XY.CO2NET.HttpUtility
{
    /// <summary>
    /// HTTP 请求工具类
    /// </summary>
    public static partial class RequestUtility
    {
        #region 静态公共方法
#if NET48
        /// <summary>
        /// 给.NET Framework使用的HttpPost请求公共设置方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">需要上传的文件，Key：对应要上传的Name，Value：本地文件名，或文件内容的Base64编码</param>
        /// <param name="refererUrl"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static HttpWebRequest HttpPost_Common_Net45(string url, CookieContainer cookieContainer = null,
            Stream postStream = null, Dictionary<string, string> fileDictionary = null, string refererUrl = null,
            Encoding encoding = null, X509Certificate2 cer = null, bool useAjax = false, Dictionary<string, string> headerAddition = null,
            int timeOut = Config.TIME_OUT, bool checkValidationResult = false, string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = timeOut;
            request.Proxy = _webproxy;
            if (cer != null)
            {
                request.ClientCertificates.Add(cer);
            }

            if (checkValidationResult)
            {
                ServicePointManager.ServerCertificateValidationCallback =
                  new RemoteCertificateValidationCallback(CheckValidationResult);
            }

            #region 处理Form表单文件上传
            var formUploadFile = fileDictionary != null && fileDictionary.Count > 0;//是否用Form上传文件
            if (formUploadFile)
            {
                contentType = "multipart/form-data";

                //通过表单上传文件
                string boundary = "----" + SystemTime.Now.Ticks.ToString("x");

                postStream = postStream ?? new MemoryStream();
                //byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                string fileFormdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
                string dataFormdataTemplate = "\r\n--" + boundary +
                                                "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

                foreach (var file in fileDictionary)
                {
                    try
                    {
                        var fileNameOrFileData = file.Value;

                        var formFileData = new FormFileData(fileNameOrFileData);
                        string formdata = null;
                        using (var memoryStream = new MemoryStream())
                        {
                            if (formFileData.TryLoadStream(memoryStream).ConfigureAwait(false).GetAwaiter().GetResult())
                            {
                                //fileNameOrFileData 中储存的储存的是 Stream
                                var fileName = Path.GetFileName(formFileData.GetAvaliableFileName(SystemTime.NowTicks.ToString()));
                                formdata = string.Format(fileFormdataTemplate, file.Key, fileName);
                            }
                            else
                            {
                                //准备文件流
                                using (var fileStream = FileHelper.GetFileStream(fileNameOrFileData))
                                {
                                    if (fileStream != null)
                                    {
                                        //存在文件
                                        memoryStream.Seek(0, SeekOrigin.Begin);
                                        fileStream.CopyTo(memoryStream);
                                        formdata = string.Format(fileFormdataTemplate, file.Key, Path.GetFileName(fileNameOrFileData));
                                        fileStream.Dispose();
                                    }
                                    else
                                    {
                                        //不存在文件或只是注释
                                        formdata = string.Format(dataFormdataTemplate, file.Key, file.Value);
                                    }
                                }
                            }
                            //统一处理
                            var formdataBytes = Encoding.UTF8.GetBytes(postStream.Length == 0 ? formdata.Substring(2, formdata.Length - 2) : formdata);//第一行不需要换行
                            postStream.Write(formdataBytes, 0, formdataBytes.Length);
                            //写入文件
                            if (memoryStream.Length > 0)
                            {
                                memoryStream.Seek(0, SeekOrigin.Begin);

                                byte[] buffer = new byte[1024];
                                int bytesRead = 0;
                                while ((bytesRead = memoryStream.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    postStream.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                //结尾
                var footer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                postStream.Write(footer, 0, footer.Length);

                //request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);//request.ContentType在下方统一设置
                contentType = string.Format("multipart/form-data; boundary={0}", boundary);
            }
            else
            {
                if (postStream.Length > 0)
                {
                    if (contentType == HttpClientHelper.DEFAULT_CONTENT_TYPE)
                    {
                        //如果ContentType是默认值，则设置成为二进制流
                        contentType = "application/octet-stream";
                    }
                    //contentType = "application/x-www-form-urlencoded";
                }
            }
            #endregion

            request.ContentType = contentType;
            request.ContentLength = postStream != null ? postStream.Length : 0;

            HttpClientHeader(request, refererUrl, useAjax, headerAddition, timeOut);

            if (cookieContainer != null)
            {
                request.CookieContainer = cookieContainer;
            }
            return request;
        }
#endif

#if !NET48
        /// <summary>
        /// 给.NET Core使用的HttpPost请求公共设置方法
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="hc"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">需要上传的文件，Key：对应要上传的Name，Value：本地文件名，或文件内容的Base64编码</param>
        /// <param name="refererUrl"></param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static HttpClient HttpPost_Common_NetCore(
            IServiceProvider serviceProvider,
            string url, out HttpContent hc, CookieContainer cookieContainer = null,
            Stream postStream = null, Dictionary<string, string> fileDictionary = null, string refererUrl = null,
            Encoding encoding = null, string certName = null, bool useAjax = false, Dictionary<string, string> headerAddition = null,
            int timeOut = Config.TIME_OUT, bool checkValidationResult = false, string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
            //HttpClientHandler handler = HttpClientHelper.GetHttpClientHandler(cookieContainer, XYHttpClientWebProxy, DecompressionMethods.GZip);

            //if (checkValidationResult)
            //{
            //    handler.ServerCertificateCustomValidationCallback = new Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>(CheckValidationResult);
            //}

            //if (cer != null)
            //{
            //    handler.ClientCertificates.Add(cer);
            //}
            var xyHttpClient = XYHttpClient.GetInstanceByName(serviceProvider, certName);
            xyHttpClient.SetCookie(new Uri(url), cookieContainer);//设置Cookie

            HttpClient client = xyHttpClient.Client;
            HttpClientHeader(client, refererUrl, useAjax, headerAddition, timeOut);

			#region 处理Form表单文件上传
			var formUploadFile = fileDictionary != null && fileDictionary.Count > 0;//是否用Form上传文件
            if (formUploadFile)
            {
                contentType = "multipart/form-data";

                //通过表单上传文件
                string boundary = "----" + SystemTime.Now.Ticks.ToString("x");

                var multipartFormDataContent = new MultipartFormDataContent(boundary);
                hc = multipartFormDataContent;

                foreach (var file in fileDictionary)
                {
                    try
                    {
                        var fileNameOrFileData = file.Value;
                        var formFileData = new FormFileData(fileNameOrFileData);
                        string fileName = null;

                        //准备文件流
                        var memoryStream = new MemoryStream();//这里不能释放，否则如在请求的时候 memoryStream 已经关闭会发生错误
                        if (formFileData.TryLoadStream(memoryStream).ConfigureAwait(false).GetAwaiter().GetResult())
                        {
                            //fileNameOrFileData 中储存的储存的是 Stream
                            fileName = Path.GetFileName(formFileData.GetAvaliableFileName(SystemTime.NowTicks.ToString()));
                        }
                        else
                        {
                            //fileNameOrFileData 中储存的储存的可能是文件地址或备注
                            using (var fileStream = FileHelper.GetFileStream(fileNameOrFileData))
                            {
                                if (fileStream != null)
                                {
                                    //存在文件
                                    fileStream.CopyTo(memoryStream);//TODO:可以使用异步方法
                                    fileName = Path.GetFileName(fileNameOrFileData);
                                    fileStream.Dispose();
                                }
                                else
                                {
                                    //只是注释
                                    multipartFormDataContent.Add(new StringContent(file.Value), "\"" + file.Key + "\"");
                                }
                            }
                        }

                        if (memoryStream.Length > 0)
                        {
                            //有文件内容
                            //multipartFormDataContent.Add(new StreamContent(memoryStream), file.Key, Path.GetFileName(fileName)); //报流已关闭的异常

                            memoryStream.Seek(0, SeekOrigin.Begin);
                            multipartFormDataContent.Add(CreateFileContent(memoryStream, file.Key, fileName), file.Key, fileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                hc.Headers.ContentType = MediaTypeHeaderValue.Parse(string.Format("multipart/form-data; boundary={0}", boundary));
            }
            else
            {
                if (postStream.Length > 0)
                {
                    if (contentType == HttpClientHelper.DEFAULT_CONTENT_TYPE)
                    {
                        //如果ContentType是默认值，则设置成为二进制流
                        contentType = "application/octet-stream";
                    }

                    //contentType = "application/x-www-form-urlencoded";
                }

                hc = new StreamContent(postStream);

                hc.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                //使用Url格式Form表单Post提交的时候才使用application/x-www-form-urlencoded
                //去掉注释以测试Request.Body为空的情况
                //hc.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            }
        #endregion
            if (!string.IsNullOrEmpty(refererUrl))
            {
                client.DefaultRequestHeaders.Referrer = new Uri(refererUrl);
            }
            return client;
        }
#endif
        #endregion

        #region 同步方法
        /// <summary>
        /// 使用Post方法获取字符串结果，常规提交
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="formData"></param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header 附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <returns></returns>
        public static string HttpPost(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> formData = null,
            Encoding encoding = null,
#if !NET48
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif            
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT,
            bool checkValidationResult = false)
        {
            MemoryStream ms = new MemoryStream();
            formData.FillFormDataStream(ms);//填充formData

            string contentType = HttpClientHelper.GetContentType(formData);

            return HttpPost(
                serviceProvider,
                url, cookieContainer, ms, null, null, encoding,
#if !NET48
                certName,
#else
                cer,
#endif
                useAjax, headerAddition, timeOut, checkValidationResult, contentType);
        }

        /// <summary>
        /// 使用Post方法获取字符串结果
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">需要上传的文件，Key：对应要上传的Name，Value：本地文件名，或文件内容的Base64编码</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header 附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <param name="contentType"></param>
        /// <param name="refererUrl"></param>
        /// <returns></returns>
        public static string HttpPost(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream postStream = null,
            Dictionary<string, string> fileDictionary = null, string refererUrl = null, Encoding encoding = null,
#if !NET48
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT, bool checkValidationResult = false,
            string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }

            var xyResponse = HttpResponsePost(
                serviceProvider,
                url, cookieContainer, postStream, fileDictionary, refererUrl, encoding,
#if !NET48
                certName,
#else
                cer,
#endif
                useAjax, headerAddition, timeOut, checkValidationResult, contentType);

            var response = xyResponse.Result;//获取响应信息

#if NET48
            response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            using (Stream responseStream = response.GetResponseStream() ?? new MemoryStream())
            {
                using (StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.GetEncoding("utf-8")))
                {
                    string retString = myStreamReader.ReadToEnd();
                    return retString;
                }
            }
#else
            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie

            if (response.Content.Headers.ContentType != null &&
                response.Content.Headers.ContentType.CharSet != null &&
                response.Content.Headers.ContentType.CharSet.ToLower().Contains("utf8"))
            {
                response.Content.Headers.ContentType.CharSet = "utf-8";
            }

            var retString = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            return retString;
#endif
        }

        /// <summary>
        /// 使用Post方法获取HttpWebResponse或HttpResponseMessage对象，本方法独立使用时通常用于测试）
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">需要上传的文件，Key：对应要上传的Name，Value：本地文件名，或文件内容的Base64编码</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <param name="contentType"></param>
        /// <param name="refererUrl"></param>
        /// <returns></returns>
        public static XYHttpResponse HttpResponsePost(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream postStream = null,
            Dictionary<string, string> fileDictionary = null, string refererUrl = null, Encoding encoding = null,
#if !NET48
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT,
            bool checkValidationResult = false, string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }

            var postStreamIsDefaultNull = postStream == null;
            if (postStreamIsDefaultNull)
            {
                postStream = new MemoryStream();
            }

#if NET48
            var request = HttpPost_Common_Net45(url, cookieContainer, postStream, fileDictionary, refererUrl, encoding, cer, useAjax, headerAddition, timeOut, checkValidationResult, contentType);

            #region 输入二进制流
            if (postStream != null && postStream.Length > 0)
            {
                postStream.Position = 0;
                //直接写入流
                Stream requestStream = request.GetRequestStream();
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = postStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                }
                postStream.Close();//关闭文件访问
            }
            #endregion

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return new XYHttpResponse(response);
#else
            HttpContent hc;
            var client = HttpPost_Common_NetCore(serviceProvider, url, out hc, cookieContainer, postStream, fileDictionary, refererUrl, encoding, certName, useAjax, headerAddition, timeOut, checkValidationResult, contentType);
            var response = client.PostAsync(url, hc).ConfigureAwait(false).GetAwaiter().GetResult();//获取响应信息
            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie
            try
            {
                if (postStreamIsDefaultNull && postStream.Length > 0)
                {
                    postStream.Close();
                }
                hc.Dispose();//关闭HttpContent（StreamContent）
            }
            catch (BaseException ex)
            {
            }
            return new XYHttpResponse(response);
#endif
        }
        #endregion

        #region 异步方法
        /// <summary>
        /// 使用Post方法获取字符串结果，常规提交
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="formData"></param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header 附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <returns></returns>
        public static async Task<string> HttpPostAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null,
            Dictionary<string, string> formData = null, Encoding encoding = null,
#if !NET48
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT,
            bool checkValidationResult = false)
        {
            MemoryStream ms = new MemoryStream();
            await formData.FillFormDataStreamAsync(ms).ConfigureAwait(false);//填充formData

            string contentType = HttpClientHelper.GetContentType(formData);

            return await HttpPostAsync(
                serviceProvider,
                url, cookieContainer, ms, null, null, encoding,
#if !NET48
                certName,
#else
                cer,
#endif
                useAjax, headerAddition, timeOut, checkValidationResult, contentType).ConfigureAwait(false);
        }

        /// <summary>
        /// 使用Post方法获取字符串结果
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">需要上传的文件，Key：对应要上传的Name，Value：本地文件名，或文件内容的Base64编码</param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer"></param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <param name="contentType"></param>
        /// <param name="refererUrl"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<string> HttpPostAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream postStream = null,
            Dictionary<string, string> fileDictionary = null, string refererUrl = null, Encoding encoding = null,
#if !NET48
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT, bool checkValidationResult = false,
            string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }
            var postStreamIsDefaultNull = postStream == null;
            if (postStreamIsDefaultNull)
            {
                postStream = new MemoryStream();
            }
            var xyResponse = await HttpResponsePostAsync(
                serviceProvider,
                url, cookieContainer, postStream, fileDictionary, refererUrl, encoding,
#if !NET48
                certName,
#else
                cer,
#endif
                useAjax, headerAddition, timeOut, checkValidationResult, contentType).ConfigureAwait(false);

            var response = xyResponse.Result;//获取响应信息

#if NET48
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
            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie
            var retString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return retString;
#endif
        }

        /// <summary>
        /// 使用Post方法获取HttpWebResponse或HttpResponseMessage对象，本方法独立使用时通常用于测试）
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">需要上传的文件，Key：对应要上传的Name，Value：本地文件名，或文件内容的Base64编码</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <param name="contentType"></param>
        /// <param name="refererUrl"></param>
        /// <returns></returns>
        public static async Task<XYHttpResponse> HttpResponsePostAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream postStream = null,
            Dictionary<string, string> fileDictionary = null, string refererUrl = null, Encoding encoding = null,
#if !NET48
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT,
            bool checkValidationResult = false, string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }

            var postStreamIsDefaultNull = postStream == null;
            if (postStreamIsDefaultNull)
            {
                postStream = new MemoryStream();
            }

#if NET48
            var request = HttpPost_Common_Net45(url, cookieContainer, postStream, fileDictionary, refererUrl, encoding, cer, useAjax, headerAddition, timeOut, checkValidationResult, contentType);

            #region 输入二进制流
            if (postStream != null && postStream.Length > 0)
            {
                postStream.Position = 0;
                //直接写入流
                Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false);
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = await postStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) != 0)
                {
                    await requestStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                }
                postStream.Close();//关闭文件访问
            }
            #endregion

            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false));
            return new XYHttpResponse(response);
#else
            HttpContent hc;
            var client = HttpPost_Common_NetCore(serviceProvider, url, out hc, cookieContainer, postStream, fileDictionary, refererUrl, encoding, certName, useAjax, headerAddition, timeOut, checkValidationResult, contentType);
            var response = await client.PostAsync(url, hc).ConfigureAwait(false);//获取响应信息
            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie
            try
            {
                if (postStreamIsDefaultNull && postStream.Length > 0)
                {
                    postStream.Close();
                }
                hc.Dispose();//关闭HttpContent（StreamContent）
            }
            catch (BaseException ex)
            {
            }
            return new XYHttpResponse(response);
#endif
        }
        #endregion
    }
}
