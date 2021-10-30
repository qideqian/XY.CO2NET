using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XY.CO2NET.Cache;
using XY.CO2NET.RegisterServices;
using XY.CO2NET.Threads;

namespace XY.CO2NET
{
    /// <summary>
    /// XY.CO2NET 基础信息注册
    /// </summary>
    public static class Register
    {
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

#if !NET45
        /// <summary>
        /// 开始 XY.CO2NET 初始化参数流程
        /// </summary>
        /// <param name="senparcSetting">XYSetting 对象</param>
        /// <param name="registerConfigure">RegisterService 设置</param>
        /// <param name="autoScanExtensionCacheStrategies">是否自动扫描全局的扩展缓存（会增加系统启动时间）</param>
        /// <param name="extensionCacheStrategiesFunc"><para>需要手动注册的扩展缓存策略</para>
        /// <para>（LocalContainerCacheStrategy、RedisContainerCacheStrategy、MemcacheContainerCacheStrategy已经自动注册），</para>
        /// <para>如果设置为 null（注意：不适委托返回 null，是整个委托参数为 null），则自动使用反射扫描所有可能存在的扩展缓存策略</para></param>
        /// <returns></returns>
        public static IRegisterService UseXYGlobal(
            XYSetting senparcSetting,
            Action<RegisterService> registerConfigure,
            bool autoScanExtensionCacheStrategies = false,
            Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {
            //初始化全局 RegisterService 对象，并储存 XYSetting 信息
            var register = RegisterService.Start(senparcSetting);
            RegisterService.Object = register;

            registerConfigure?.Invoke(register);

            return register.UseXYGlobal(autoScanExtensionCacheStrategies, extensionCacheStrategiesFunc);
        }
#endif
    }
}
