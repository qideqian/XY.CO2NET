using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace XY.OCR.UmiOCR
{
    /// <summary>
    /// XY.OCR.UmiOCR 注册扩展。
    /// </summary>
    public static class Register
    {
        /// <summary>
        /// 从配置绑定并注册 Umi-OCR 服务。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="configuration">应用配置。</param>
        /// <param name="sectionName">配置节名称，默认值为 <see cref="XYUmiOCROptions.DefaultSectionName" />。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        public static IServiceCollection AddXYUmiOCR(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = XYUmiOCROptions.DefaultSectionName)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.Configure<XYUmiOCROptions>(configuration.GetSection(sectionName));
            services.AddHttpClient<IUmiOCRService, UmiOCRService>();
            return services;
        }

        /// <summary>
        /// 使用委托配置并注册 Umi-OCR 服务。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="configure">Umi-OCR 配置委托。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        public static IServiceCollection AddXYUmiOCR(
            this IServiceCollection services,
            Action<XYUmiOCROptions> configure)
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
            services.AddHttpClient<IUmiOCRService, UmiOCRService>();
            return services;
        }

        /// <summary>
        /// 通过直接传参注册 Umi-OCR 服务。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="ocrUrl">图片 OCR 接口地址。</param>
        /// <param name="docUrl">文档 OCR 接口基础地址。</param>
        /// <param name="pollingIntervalMilliseconds">PDF 任务轮询间隔（毫秒）。</param>
        /// <param name="maxPollingAttempts">PDF 任务最大轮询次数。</param>
        /// <param name="lineMergeThreshold">文本合并时按 Y 轴换行的阈值。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        public static IServiceCollection AddXYUmiOCR(
            this IServiceCollection services,
            string ocrUrl,
            string docUrl,
            int pollingIntervalMilliseconds = 1000,
            int maxPollingAttempts = 300,
            double lineMergeThreshold = 10d)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrWhiteSpace(ocrUrl))
            {
                throw new ArgumentException("图片 OCR 接口地址不能为空。", nameof(ocrUrl));
            }

            if (string.IsNullOrWhiteSpace(docUrl))
            {
                throw new ArgumentException("文档 OCR 接口基础地址不能为空。", nameof(docUrl));
            }

            return services.AddXYUmiOCR(options =>
            {
                options.OcrUrl = ocrUrl;
                options.DocUrl = docUrl;
                options.PollingIntervalMilliseconds = pollingIntervalMilliseconds;
                options.MaxPollingAttempts = maxPollingAttempts;
                options.LineMergeThreshold = lineMergeThreshold;
            });
        }
    }
}
