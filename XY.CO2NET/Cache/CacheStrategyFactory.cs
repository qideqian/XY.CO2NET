using System;
using System.Collections.Generic;
using System.Text;

namespace XY.CO2NET.Cache
{
    /// <summary>
    /// 缓存策略工厂。
    /// <para>缓存策略的注册（立即启用）和当前缓存策略获取</para>
    /// </summary>
    public static class CacheStrategyFactory
    {
        internal static Func<IBaseObjectCacheStrategy> ObjectCacheStrateFunc { get; set; }
        internal static IBaseObjectCacheStrategy ObjectCacheStrategy { get; set; }

        /// <summary>
        /// 注册当前全局环境下的缓存策略，并立即启用。
        /// </summary>
        /// <param name="func">如果为 null，将使用默认的本地缓存策略（LocalObjectCacheStrategy.Instance）</param>
        public static void RegisterObjectCacheStrategy(Func<IBaseObjectCacheStrategy> func)
        {
            ObjectCacheStrateFunc = func;

            if (func != null)
            {
                ObjectCacheStrategy = func();//提前运行一次，否则第一次运行开销比较大（400毫秒以上）
            }
        }

        /// <summary>
        /// 获取全局缓存策略
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IBaseObjectCacheStrategy GetObjectCacheStrategyInstance(this IServiceProvider serviceProvider)
        {
            return GetObjectCacheStrategyInstance();
        }

        /// <summary>
        /// 获取全局缓存策略
        /// </summary>
        /// <returns></returns>
        public static IBaseObjectCacheStrategy GetObjectCacheStrategyInstance()
        {
            if (ObjectCacheStrateFunc == null)
            {
                //默认状态
                return LocalObjectCacheStrategy.Instance;
            }
            else
            {
                //自定义类型
                var instance = ObjectCacheStrateFunc();
                return instance;
            }
        }

        /// <summary>
        /// 获取指定领域缓存的缓存策略
        /// </summary>
        /// <param name="cacheStrategyDomain">领域缓存信息（需要为单例）CacheStrategyDomain</param>
        /// <returns></returns>
        public static IDomainExtensionCacheStrategy GetExtensionCacheStrategyInstance(ICacheStrategyDomain cacheStrategyDomain)
        {
            var currentObjectCacheStrategy = GetObjectCacheStrategyInstance();
            var domianExtensionCacheStrategy = CacheStrategyDomainWarehouse.GetDomainExtensionCacheStrategy(currentObjectCacheStrategy, cacheStrategyDomain);
            return domianExtensionCacheStrategy;
        }
    }
}
