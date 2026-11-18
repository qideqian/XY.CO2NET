using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using XY.CO2NET.Cache;
using XY.CO2NET.RegisterServices;
using XY.CO2NET.Threads;

#if !net48
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
#if !net48
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
        /// 调用 ThreadUtility.Register；在线程容器为空时创建并启动后台队列线程
        /// </summary>
        /// <returns>传入的服务集合；此方法不添加 DI 服务描述。</returns>
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
        /// 遍历手动提供的扩展缓存策略，并按开关执行反射扫描
        /// </summary>
        /// <remarks>登记依赖提供器或策略初始化的副作用；此包装器不保证策略已登记，也不返回收集到的类型列表。</remarks>
        /// <param name="registerService">现有注册服务对象</param>
        /// <param name="autoScanExtensionCacheStrategies">是否反射扫描已加载程序集中的扩展缓存策略类型，默认为 false；独立于手动提供器是否为 null</param>
        /// <param name="extensionCacheStrategiesFunc">手动提供扩展缓存策略实例的委托；委托或其返回值为 null 时跳过手动列表，不会因此开启反射扫描。</param>
        /// <returns>传入的注册服务对象。</returns>
        public static IRegisterService UseXYGlobal(this IRegisterService registerService, bool autoScanExtensionCacheStrategies = false, Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {
            //遍历手动实例并按开关扫描；登记取决于提供器或策略初始化的副作用
            CacheStrategyDomainWarehouse.AutoScanDomainCacheStrategy(autoScanExtensionCacheStrategies, extensionCacheStrategiesFunc);
            return registerService;
        }

#if !net48
        /// <summary>
        /// 创建全局注册服务、保存设置、执行可选配置回调，再处理扩展缓存策略
        /// </summary>
        /// <remarks>RegisterService.Start 会调用线程注册；扩展缓存策略的登记仍依赖提供器或策略初始化的副作用。</remarks>
        /// <param name="xySetting">XYSetting 对象；为 null 时由 RegisterService 创建默认设置</param>
        /// <param name="registerConfigure">RegisterService 设置</param>
        /// <param name="autoScanExtensionCacheStrategies">是否反射扫描已加载程序集中的扩展缓存策略类型，默认为 false；独立于手动提供器是否为 null</param>
        /// <param name="extensionCacheStrategiesFunc">手动提供扩展缓存策略实例的委托；委托或其返回值为 null 时跳过手动列表，不会因此开启反射扫描。</param>
        /// <returns>本次创建的注册服务对象。</returns>
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

#if !net48
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
