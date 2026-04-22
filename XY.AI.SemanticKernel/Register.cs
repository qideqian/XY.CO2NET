using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System;

namespace XY.AI.SemanticKernel
{
    /// <summary>
    /// XY.AI.SemanticKernel 注册扩展。
    /// </summary>
    public static class Register
    {
        /// <summary>
        /// 从配置中按指定服务类型注册单个 AI 聊天服务到 Semantic Kernel。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="configuration">应用配置。</param>
        /// <param name="serviceType">要启用的单个 AI 服务类型。</param>
        /// <param name="aiServicesSectionName">AI 服务配置节名称，默认值为 <see cref="XYAIServicesOptions.DefaultSectionName" />。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="services" /> 或 <paramref name="configuration" /> 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当配置缺少必要字段时抛出。</exception>
        public static IServiceCollection AddXYAIKernelWithAIServices(
            this IServiceCollection services,
            IConfiguration configuration,
            XYAIServiceType serviceType,
            string aiServicesSectionName = XYAIServicesOptions.DefaultSectionName)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var options = new XYAIServicesOptions();
            configuration.GetSection(aiServicesSectionName).Bind(options);

            var kernelBuilder = services.AddKernel();
            AddSelectedService(kernelBuilder, options, serviceType, aiServicesSectionName);
            return services;
        }

        /// <summary>
        /// 一站式注册单个 AI 聊天服务和结构化数据提取服务。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="configuration">应用配置。</param>
        /// <param name="serviceType">要启用的单个 AI 服务类型。</param>
        /// <param name="aiServicesSectionName">AI 服务配置节名称，默认值为 <see cref="XYAIServicesOptions.DefaultSectionName" />。</param>
        /// <param name="extractorSectionName">结构化提取配置节名称，默认值为 <see cref="XYAIStructuredDataOptions.DefaultSectionName" />。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="services" /> 或 <paramref name="configuration" /> 为 null 时抛出。</exception>
        public static IServiceCollection AddXYAIStructuredDataExtractorWithAIServices(
            this IServiceCollection services,
            IConfiguration configuration,
            XYAIServiceType serviceType,
            string aiServicesSectionName = XYAIServicesOptions.DefaultSectionName,
            string extractorSectionName = XYAIStructuredDataOptions.DefaultSectionName)
        {
            return services
                .AddXYAIKernelWithAIServices(configuration, serviceType, aiServicesSectionName)
                .AddXYAIStructuredDataExtractor(configuration, extractorSectionName);
        }

        /// <summary>
        /// 从配置中注册多 AI 聊天服务（Azure OpenAI / QWen / DeepSeek）到 Semantic Kernel。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="configuration">应用配置。</param>
        /// <param name="aiServicesSectionName">AI 服务配置节名称，默认值为 <see cref="XYAIServicesOptions.DefaultSectionName" />。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="services" /> 或 <paramref name="configuration" /> 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当配置缺少必要字段时抛出。</exception>
        public static IServiceCollection AddXYAIKernelWithAIServices(
            this IServiceCollection services,
            IConfiguration configuration,
            string aiServicesSectionName = XYAIServicesOptions.DefaultSectionName)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var options = new XYAIServicesOptions();
            configuration.GetSection(aiServicesSectionName).Bind(options);

            ValidateServiceOptions(options.AzureOpenAI, aiServicesSectionName, nameof(XYAIServicesOptions.AzureOpenAI));
            ValidateServiceOptions(options.QWenPlus, aiServicesSectionName, nameof(XYAIServicesOptions.QWenPlus));
            ValidateServiceOptions(options.DeepSeek, aiServicesSectionName, nameof(XYAIServicesOptions.DeepSeek));

            var kernelBuilder = services.AddKernel();

            AddDefaultChatCompletion(kernelBuilder, options);

            kernelBuilder
                .AddAzureOpenAIChatCompletion(
                    options.AzureOpenAI.ModelId,
                    options.AzureOpenAI.Endpoint,
                    options.AzureOpenAI.ApiKey,
                    options.AzureOpenAI.ServiceId)
                .AddOpenAIChatCompletion(
                    options.QWenPlus.ModelId,
                    new Uri(options.QWenPlus.Endpoint),
                    options.QWenPlus.ApiKey,
                    null,
                    options.QWenPlus.ServiceId)
                .AddOpenAIChatCompletion(
                    options.DeepSeek.ModelId,
                    new Uri(options.DeepSeek.Endpoint),
                    options.DeepSeek.ApiKey,
                    null,
                    options.DeepSeek.ServiceId);

            return services;
        }

        /// <summary>
        /// 一站式注册多 AI 聊天服务和结构化数据提取服务。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="configuration">应用配置。</param>
        /// <param name="aiServicesSectionName">AI 服务配置节名称，默认值为 <see cref="XYAIServicesOptions.DefaultSectionName" />。</param>
        /// <param name="extractorSectionName">结构化提取配置节名称，默认值为 <see cref="XYAIStructuredDataOptions.DefaultSectionName" />。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="services" /> 或 <paramref name="configuration" /> 为 null 时抛出。</exception>
        public static IServiceCollection AddXYAIStructuredDataExtractorWithAIServices(
            this IServiceCollection services,
            IConfiguration configuration,
            string aiServicesSectionName = XYAIServicesOptions.DefaultSectionName,
            string extractorSectionName = XYAIStructuredDataOptions.DefaultSectionName)
        {
            return services
                .AddXYAIKernelWithAIServices(configuration, aiServicesSectionName)
                .AddXYAIStructuredDataExtractor(configuration, extractorSectionName);
        }

        /// <summary>
        /// 一站式注册 OpenAI Kernel 和 AI 结构化数据提取服务。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="modelId">OpenAI 聊天模型名称，例如 gpt-4o-mini。</param>
        /// <param name="apiKey">OpenAI API Key。</param>
        /// <param name="configure">结构化提取服务选项配置委托。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="services" /> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当 <paramref name="modelId" /> 或 <paramref name="apiKey" /> 为空时抛出。</exception>
        public static IServiceCollection AddXYAIStructuredDataExtractorWithOpenAI(
            this IServiceCollection services,
            string modelId,
            string apiKey,
            Action<XYAIStructuredDataOptions> configure = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrWhiteSpace(modelId))
            {
                throw new ArgumentException("OpenAI 模型名称不能为空。", nameof(modelId));
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("OpenAI API Key 不能为空。", nameof(apiKey));
            }

            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddOpenAIChatCompletion(modelId, apiKey);
            services.AddSingleton(kernelBuilder.Build());

            return services.AddXYAIStructuredDataExtractor(configure);
        }

        /// <summary>
        /// 一站式注册 OpenAI Kernel 和 AI 结构化数据提取服务（从配置读取 OpenAI 参数）。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="configuration">应用配置。</param>
        /// <param name="openAISectionName">OpenAI 配置节名称，默认值为 <see cref="XYAIOpenAIOptions.DefaultSectionName" />。</param>
        /// <param name="extractorSectionName">结构化提取配置节名称，默认值为 <see cref="XYAIStructuredDataOptions.DefaultSectionName" />。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="services" /> 或 <paramref name="configuration" /> 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当配置中缺少 ModelId 或 ApiKey 时抛出。</exception>
        public static IServiceCollection AddXYAIStructuredDataExtractorWithOpenAI(
            this IServiceCollection services,
            IConfiguration configuration,
            string openAISectionName = XYAIOpenAIOptions.DefaultSectionName,
            string extractorSectionName = XYAIStructuredDataOptions.DefaultSectionName)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var openAIOptions = new XYAIOpenAIOptions();
            configuration.GetSection(openAISectionName).Bind(openAIOptions);

            if (string.IsNullOrWhiteSpace(openAIOptions.ModelId))
            {
                throw new InvalidOperationException($"配置节 '{openAISectionName}' 的 ModelId 不能为空。");
            }

            if (string.IsNullOrWhiteSpace(openAIOptions.ApiKey))
            {
                throw new InvalidOperationException($"配置节 '{openAISectionName}' 的 ApiKey 不能为空。");
            }

            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddOpenAIChatCompletion(openAIOptions.ModelId, openAIOptions.ApiKey);
            services.AddSingleton(kernelBuilder.Build());

            return services.AddXYAIStructuredDataExtractor(configuration, extractorSectionName);
        }

        /// <summary>
        /// 注册 AI 结构化数据提取服务。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="configure">结构化提取服务选项配置委托。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="services" /> 为 null 时抛出。</exception>
        public static IServiceCollection AddXYAIStructuredDataExtractor(
            this IServiceCollection services,
            Action<XYAIStructuredDataOptions> configure = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure != null)
            {
                services.Configure(configure);
            }
            else
            {
                services.Configure<XYAIStructuredDataOptions>(_ => { });
            }

            services.AddSingleton<IAIStructuredDataExtractor, AIStructuredDataExtractor>();
            return services;
        }

        /// <summary>
        /// 从配置绑定并注册 AI 结构化数据提取服务。
        /// </summary>
        /// <param name="services">依赖注入服务集合。</param>
        /// <param name="configuration">应用配置。</param>
        /// <param name="sectionName">配置节名称，默认值为 <see cref="XYAIStructuredDataOptions.DefaultSectionName" />。</param>
        /// <returns>返回同一个 <see cref="IServiceCollection" />，便于链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="services" /> 或 <paramref name="configuration" /> 为 null 时抛出。</exception>
        public static IServiceCollection AddXYAIStructuredDataExtractor(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = XYAIStructuredDataOptions.DefaultSectionName)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.Configure<XYAIStructuredDataOptions>(configuration.GetSection(sectionName));
            services.AddSingleton<IAIStructuredDataExtractor, AIStructuredDataExtractor>();
            return services;
        }

        private static void AddDefaultChatCompletion(IKernelBuilder kernelBuilder, XYAIServicesOptions options)
        {
            switch (options.DefaultUseServiceId)
            {
                case var value when string.Equals(value, options.AzureOpenAI.ServiceId, StringComparison.OrdinalIgnoreCase):
                    kernelBuilder.AddAzureOpenAIChatCompletion(
                        options.AzureOpenAI.ModelId,
                        options.AzureOpenAI.Endpoint,
                        options.AzureOpenAI.ApiKey);
                    break;
                case var value when string.Equals(value, options.QWenPlus.ServiceId, StringComparison.OrdinalIgnoreCase):
                    kernelBuilder.AddOpenAIChatCompletion(
                        options.QWenPlus.ModelId,
                        new Uri(options.QWenPlus.Endpoint),
                        options.QWenPlus.ApiKey);
                    break;
                case var value when string.Equals(value, options.DeepSeek.ServiceId, StringComparison.OrdinalIgnoreCase):
                    kernelBuilder.AddOpenAIChatCompletion(
                        options.DeepSeek.ModelId,
                        new Uri(options.DeepSeek.Endpoint),
                        options.DeepSeek.ApiKey);
                    break;
                default:
                    kernelBuilder.AddAzureOpenAIChatCompletion(
                        options.AzureOpenAI.ModelId,
                        options.AzureOpenAI.Endpoint,
                        options.AzureOpenAI.ApiKey);
                    break;
            }
        }

        private static void ValidateServiceOptions(XYAIServiceEndpointOptions options, string sectionRoot, string childName)
        {
            if (string.IsNullOrWhiteSpace(options.ModelId))
            {
                throw new InvalidOperationException($"配置节 '{sectionRoot}:{childName}' 的 ModelId 不能为空。");
            }

            if (string.IsNullOrWhiteSpace(options.Endpoint))
            {
                throw new InvalidOperationException($"配置节 '{sectionRoot}:{childName}' 的 Endpoint 不能为空。");
            }

            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new InvalidOperationException($"配置节 '{sectionRoot}:{childName}' 的 ApiKey 不能为空。");
            }

            if (string.IsNullOrWhiteSpace(options.ServiceId))
            {
                throw new InvalidOperationException($"配置节 '{sectionRoot}:{childName}' 的 ServiceId 不能为空。");
            }
        }

        private static void AddSelectedService(
            IKernelBuilder kernelBuilder,
            XYAIServicesOptions options,
            XYAIServiceType serviceType,
            string sectionRoot)
        {
            switch (serviceType)
            {
                case XYAIServiceType.AzureOpenAI:
                    ValidateServiceOptions(options.AzureOpenAI, sectionRoot, nameof(XYAIServicesOptions.AzureOpenAI));
                    kernelBuilder
                        .AddAzureOpenAIChatCompletion(
                            options.AzureOpenAI.ModelId,
                            options.AzureOpenAI.Endpoint,
                            options.AzureOpenAI.ApiKey)
                        .AddAzureOpenAIChatCompletion(
                            options.AzureOpenAI.ModelId,
                            options.AzureOpenAI.Endpoint,
                            options.AzureOpenAI.ApiKey,
                            options.AzureOpenAI.ServiceId);
                    break;
                case XYAIServiceType.QWenPlus:
                    ValidateServiceOptions(options.QWenPlus, sectionRoot, nameof(XYAIServicesOptions.QWenPlus));
                    kernelBuilder
                        .AddOpenAIChatCompletion(
                            options.QWenPlus.ModelId,
                            new Uri(options.QWenPlus.Endpoint),
                            options.QWenPlus.ApiKey)
                        .AddOpenAIChatCompletion(
                            options.QWenPlus.ModelId,
                            new Uri(options.QWenPlus.Endpoint),
                            options.QWenPlus.ApiKey,
                            null,
                            options.QWenPlus.ServiceId);
                    break;
                case XYAIServiceType.DeepSeek:
                    ValidateServiceOptions(options.DeepSeek, sectionRoot, nameof(XYAIServicesOptions.DeepSeek));
                    kernelBuilder
                        .AddOpenAIChatCompletion(
                            options.DeepSeek.ModelId,
                            new Uri(options.DeepSeek.Endpoint),
                            options.DeepSeek.ApiKey)
                        .AddOpenAIChatCompletion(
                            options.DeepSeek.ModelId,
                            new Uri(options.DeepSeek.Endpoint),
                            options.DeepSeek.ApiKey,
                            null,
                            options.DeepSeek.ServiceId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, "不支持的 AI 服务类型。");
            }
        }
    }
}
