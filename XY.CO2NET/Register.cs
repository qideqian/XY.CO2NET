using System;
using System.Collections.Generic;
using XY.CO2NET.Cache;
using XY.CO2NET.RegisterServices;
using XY.CO2NET.Threads;
using Microsoft.Extensions.Configuration;

#if !NET48
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
#endif

namespace XY.CO2NET
{
    /// <summary>
    /// XY.CO2NET 基础信息注册
    /// </summary>
    public static class Register
    {
        #if !NET48
        /// <summary>
        /// 注册 XYSetting 到 DI 容器
        /// </summary>
        public static IServiceCollection AddXYSetting(this IServiceCollection services, XYSetting xySetting)
        {
            services.AddSingleton(xySetting);
            return services;
        }

        /// <summary>
        /// 从配置文件 section "XYSetting" 绑定并注册 XYSetting 到 DI 容器
        /// </summary>
        public static IServiceCollection AddXYSetting(this IServiceCollection services, IConfiguration configuration)
        {
            var xySetting = new XYSetting();
            configuration.GetSection("XYSetting").Bind(xySetting);
            services.AddSingleton(xySetting);
            return services;
        }

        /// <summary>
        /// 注册线程服务到 DI 容器（调用 ThreadUtility.Register）
        /// </summary>
        public static IServiceCollection AddXYThreads(this IServiceCollection services)
        {
            ThreadUtility.Register();
            return services;
        }

        /// <summary>
        /// 注册日志服务到 DI 容器（如 ITraceLog 实现）
        /// </summary>
        public static IServiceCollection AddXYTraceLog<T>(this IServiceCollection services) where T : class
        {
            services.AddSingleton(typeof(T));
            return services;
        }

        /// <summary>
        /// 注册自定义缓存策略到 DI 容器
        /// </summary>
        public static IServiceCollection AddCacheStrategy<T>(this IServiceCollection services) where T : class, IBaseObjectCacheStrategy
        {
            services.AddSingleton<IBaseObjectCacheStrategy, T>();
            return services;
        }
        #endif
        /// <summary>
        /// 修改默认缓存命名空间
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <param name="customNamespace">自定义命名空间名称</param>
        /// <returns></returns>
        public static IRegisterService ChangeDefaultCacheNamespace(this IRegisterService registerService, string customNamespace)
        {
            Config.DefaultCacheNamespace = customNamespace;
            return registerService;
        }

        /// <summary>
        /// 修改默认缓存命名空间
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <param name="customNamespace">自定义命名空间名称</param>
        /// <returns></returns>
        public static void ChangeDefaultCacheNamespace(string customNamespace)
        {
            Config.DefaultCacheNamespace = customNamespace;
        }

        /// <summary>
        /// 注册 Threads 的方法（如果不注册此线程，则AccessToken、JsTicket等都无法使用SDK自动储存和管理）
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <returns></returns>
        public static IRegisterService RegisterThreads(this IRegisterService registerService)
        {
            ThreadUtility.Register();//如果不注册此线程，则AccessToken、JsTicket等都无法使用SDK自动储存和管理。
            return registerService;
        }

        /// <summary>
        /// 注册 TraceLog 的方法
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IRegisterService RegisterTraceLog(this IRegisterService registerService, Action action)
        {
            action();
            return registerService;
        }

        /// <summary>
        /// 开始 XY.CO2NET 初始化参数流程
        /// </summary>
        /// <param name="registerService"></param>
        /// <param name="autoScanExtensionCacheStrategies">是否自动扫描全局的扩展缓存（会增加系统启动时间）</param>
        /// <param name="extensionCacheStrategiesFunc"><para>需要手动注册的扩展缓存策略</para>
        /// <para>（LocalContainerCacheStrategy、RedisContainerCacheStrategy、MemcacheContainerCacheStrategy已经自动注册），</para>
        /// <para>如果设置为 null（注意：不适委托返回 null，是整个委托参数为 null），则自动使用反射扫描所有可能存在的扩展缓存策略</para></param>
        /// <returns></returns>
        public static IRegisterService UseXYGlobal(this IRegisterService registerService, bool autoScanExtensionCacheStrategies = false, Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {
            //注册扩展缓存策略
            CacheStrategyDomainWarehouse.AutoScanDomainCacheStrategy(autoScanExtensionCacheStrategies, extensionCacheStrategiesFunc);
            return registerService;
        }

#if !NET48
        /// <summary>
        /// 开始 XY.CO2NET 初始化参数流程
        /// </summary>
        /// <param name="xySetting">XYSetting 对象</param>
        /// <param name="registerConfigure">RegisterService 设置</param>
        /// <param name="autoScanExtensionCacheStrategies">是否自动扫描全局的扩展缓存（会增加系统启动时间）</param>
        /// <param name="extensionCacheStrategiesFunc"><para>需要手动注册的扩展缓存策略</para>
        /// <para>（LocalContainerCacheStrategy、RedisContainerCacheStrategy、MemcacheContainerCacheStrategy已经自动注册），</para>
        /// <para>如果设置为 null（注意：不适委托返回 null，是整个委托参数为 null），则自动使用反射扫描所有可能存在的扩展缓存策略</para></param>
        /// <returns></returns>
        public static IRegisterService UseXYGlobal(
            XYSetting xySetting,
            Action<RegisterService> registerConfigure,
            bool autoScanExtensionCacheStrategies = false,
            Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {
            //初始化全局 RegisterService 对象，并储存 XYSetting 信息
            var register = RegisterService.Start(xySetting);
            RegisterService.Object = register;

            registerConfigure?.Invoke(register);

            return register.UseXYGlobal(autoScanExtensionCacheStrategies, extensionCacheStrategiesFunc);
        }

#endif

        /// <summary>
        /// 立即使用本地对象缓存策略
        /// </summary>
        public static void UseLocalObjectCacheNow()
        {
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => LocalObjectCacheStrategy.Instance);
        }

#if !NET48
        /// <summary>
        /// 立即使用本地对象缓存策略，并注册 .NET Core DI
        /// </summary>
        public static IServiceCollection UseLocalObjectCacheNow(this IServiceCollection serviceCollection)
        {
            return UseLocalObjectCacheNow(serviceCollection, false);
        }

        /// <summary>
        /// 立即使用本地对象缓存策略，并注册 .NET Core DI
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="useTryAddSingleton">是否使用 TryAddSingleton 避免重复注册</param>
        public static IServiceCollection UseLocalObjectCacheNow(this IServiceCollection serviceCollection, bool useTryAddSingleton)
        {
            UseLocalObjectCacheNow();

            if (useTryAddSingleton)
            {
                serviceCollection.TryAddSingleton<ILocalObjectCacheStrategy>(_ => LocalObjectCacheStrategy.Instance);
                serviceCollection.TryAddSingleton<IBaseObjectCacheStrategy>(sp => sp.GetRequiredService<ILocalObjectCacheStrategy>());
            }
            else
            {
                serviceCollection.AddSingleton<ILocalObjectCacheStrategy>(_ => LocalObjectCacheStrategy.Instance);
                serviceCollection.AddSingleton<IBaseObjectCacheStrategy>(sp => sp.GetRequiredService<ILocalObjectCacheStrategy>());
            }

            return serviceCollection;
        }
#endif
    }
}
