using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace XY.OCR.AlibabaCloud
{
    /// <summary>
    /// XY.OCR.AlibabaCloud 注册扩展。
    /// </summary>
    public static class Register
    {
        /// <summary>
        /// 从配置绑定并注册阿里云 OCR 服务。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="configuration">应用配置。</param>
        /// <param name="sectionName">配置节名称，默认值为 <see cref="XYAlibabaCloudOCROptions.DefaultSectionName" />。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        public static IServiceCollection AddXYAlibabaCloudOCR(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = XYAlibabaCloudOCROptions.DefaultSectionName)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.Configure<XYAlibabaCloudOCROptions>(configuration.GetSection(sectionName));
            services.AddSingleton<IAlibabaCloudOCRService, AlibabaCloudOCRService>();
            return services;
        }

        /// <summary>
        /// 使用委托配置并注册阿里云 OCR 服务。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="configure">阿里云 OCR 配置委托。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        public static IServiceCollection AddXYAlibabaCloudOCR(
            this IServiceCollection services,
            Action<XYAlibabaCloudOCROptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.Configure(configure);
            services.AddSingleton<IAlibabaCloudOCRService, AlibabaCloudOCRService>();
            return services;
        }

        /// <summary>
        /// 通过直接传参注册阿里云 OCR 服务。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="accessKeyId">阿里云 AccessKeyId。</param>
        /// <param name="accessKeySecret">阿里云 AccessKeySecret。</param>
        /// <param name="endpoint">OCR API Endpoint。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        public static IServiceCollection AddXYAlibabaCloudOCR(
            this IServiceCollection services,
            string accessKeyId,
            string accessKeySecret,
            string endpoint = "ocr-api.cn-hangzhou.aliyuncs.com")
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrWhiteSpace(accessKeyId))
            {
                throw new ArgumentException("AccessKeyId 不能为空。", nameof(accessKeyId));
            }

            if (string.IsNullOrWhiteSpace(accessKeySecret))
            {
                throw new ArgumentException("AccessKeySecret 不能为空。", nameof(accessKeySecret));
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint 不能为空。", nameof(endpoint));
            }

            return services.AddXYAlibabaCloudOCR(options =>
            {
                options.AccessKeyId = accessKeyId;
                options.AccessKeySecret = accessKeySecret;
                options.Endpoint = endpoint;
            });
        }
    }
}
