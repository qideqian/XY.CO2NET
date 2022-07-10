using System;
using XY.CO2NET.HttpUtility;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;

#if !NET45
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#endif

namespace XY.CO2NET.RegisterServices
{
    /// <summary>
    /// 快捷注册类，RegisterService 扩展类
    /// </summary>
    public static class RegisterServiceExtension
    {
#if !NET45
        /// <summary>
        /// 是否已经进行过全局注册
        /// </summary>
        public static bool XYGlobalServicesRegistered { get; set; }

        /// <summary>
        /// 注册 IServiceCollection，并返回 RegisterService，开始注册流程（必须）
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        /// <param name="configuration">IConfiguration</param>
        /// <returns></returns>
        public static IServiceCollection AddXYGlobalServices(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            XYDI.GlobalServiceCollection = serviceCollection;
            serviceCollection.Configure<XYSetting>(configuration.GetSection("XYSetting"));

            // .net core 3.0 HttpClient 文档参考：https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.0
            //配置 HttpClient，可使用 Head 自定义 Cookie
            serviceCollection.AddHttpClient<XYHttpClient>()
            .ConfigurePrimaryHttpMessageHandler((c) =>
            {
                var httpClientHandler = HttpClientHelper.GetHttpClientHandler(null, RequestUtility.XYHttpClientWebProxy, System.Net.DecompressionMethods.GZip);
                return httpClientHandler;
            });

            XYGlobalServicesRegistered = true;

            return serviceCollection;
        }

        /// <summary>
        /// 注册 IServiceCollection，并返回 RegisterService，开始注册流程（必须）
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        /// <param name="certName">证书名称，必须全局唯一，并且确保在全局 HttpClientFactory 内唯一</param>
        /// <param name="certSecret">证书密码</param>
        /// <param name="certPath">证书路径（物理路径）</param>
        /// <param name="checkValidationResult">设置</param>
        /// <returns></returns>
        public static IServiceCollection AddXYHttpClientWithCertificate(this IServiceCollection serviceCollection,
            string certName, string certSecret, string certPath, bool checkValidationResult = false)
        {
            //添加注册
            if (!string.IsNullOrEmpty(certPath))
            {
                if (File.Exists(certPath))
                {
                    try
                    {
                        var cert = new X509Certificate2(certPath, certSecret, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);
                        return AddXYHttpClientWithCertificate(serviceCollection, certName, cert, checkValidationResult);
                    }
                    catch (Exception ex)
                    {
                        XY.CO2NET.Trace.XYTrace.SendCustomLog($"添加微信支付证书发生异常", $"certName:{certName},certPath:{certPath}");
                        XY.CO2NET.Trace.XYTrace.BaseExceptionLog(ex);
                        return serviceCollection;
                    }
                }
                else
                {
                    XY.CO2NET.Trace.XYTrace.SendCustomLog($"已设置微信支付证书，但无法找到文件", $"certName:{certName},certPath:{certPath}");
                    return serviceCollection;
                }
            }
            return serviceCollection;
        }

        /// <summary>
        /// 注册 IServiceCollection，并返回 RegisterService，开始注册流程（必须）
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        /// <param name="certName">证书名称，必须全局唯一，并且确保在全局 HttpClientFactory 内唯一</param>
        /// <param name="cert">证书对象，也可以是 X509Certificate2</param>
        /// <param name="checkValidationResult">设置</param>
        /// <returns></returns>
        public static IServiceCollection AddXYHttpClientWithCertificate(this IServiceCollection serviceCollection,
            string certName, X509Certificate cert, bool checkValidationResult = false)
        {
            serviceCollection.AddHttpClient<XYHttpClient>(certName)
                         .ConfigurePrimaryHttpMessageHandler(() =>
                         {
                             var httpClientHandler = HttpClientHelper.GetHttpClientHandler(null, RequestUtility.XYHttpClientWebProxy, System.Net.DecompressionMethods.GZip);

                             httpClientHandler.ClientCertificates.Add(cert);

                             if (checkValidationResult)
                             {
                                 httpClientHandler.ServerCertificateCustomValidationCallback = new Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>(RequestUtility.CheckValidationResult);
                             }

                             return httpClientHandler;
                         });

            //serviceCollection.ResetGlobalIServiceProvider();//重置 GlobalIServiceProvider
            return serviceCollection;
        }

        /// <summary>
        /// 添加作用于 XYHttpClient 的 WebProxy（需要在 AddXYGlobalServices 之前定义）
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static IServiceCollection AddXYHttpClientProxy(this IServiceCollection serviceCollection, string host, string port, string username, string password)
        {
            RequestUtility.SetHttpProxy(host, port, username, password);
            return serviceCollection;
        }
#endif
    }
}
