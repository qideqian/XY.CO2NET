
#if !NET45
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
#endif
using System;
using System.Collections.Generic;
using XY.CO2NET.Exceptions;

namespace XY.CO2NET.Cache
{
    /// <summary>
    /// 全局静态数据源帮助类
    /// </summary>
    public static class LocalObjectCacheHelper
    {
#if NET45
        /// <summary>
        /// 所有数据集合的列表
        /// </summary>
        public static System.Web.Caching.Cache LocalObjectCache { get; set; }

        static LocalObjectCacheHelper()
        {
            LocalObjectCache = System.Web.HttpRuntime.Cache;
        }
#else
        private static IMemoryCache _localObjectCache;

        /// <summary>
        /// 所有数据集合的列表
        /// </summary>
        public static IMemoryCache LocalObjectCache
        {
            get
            {
                if (_localObjectCache == null)
                {

                    if (_localObjectCache == null)
                    {
                        try
                        {
                            var serviceProvider = XYDI.GetServiceProvider();
                            _localObjectCache = serviceProvider.GetService<IMemoryCache>();
                        }
                        catch
                        {
                            throw new CacheException("IMemoryCache 依赖注入未设置！请在 Startup.cs 中或其调用的函数执行了 LocalObjectCacheStrategy.GenerateMemoryCache() 方法！");
                        }
                    }
                }
                return _localObjectCache;
            }
        }

        /// <summary>
        /// .NET Core 的 MemoryCache 不提供遍历所有项目的方法，因此这里做一个储存Key的地方
        /// </summary>
        public static Dictionary<string, DateTimeOffset> Keys { get; set; } = new Dictionary<string, DateTimeOffset>();

        static LocalObjectCacheHelper()
        {

        }

        /// <summary>
        /// 获取储存Keys信息的缓存键
        /// </summary>
        /// <param name="cacheStrategy"></param>
        /// <returns></returns>
        public static string GetKeyStoreKey(BaseCacheStrategy cacheStrategy)
        {
            var keyStoreFinalKey = cacheStrategy.GetFinalKey("CO2NET_KEY_STORE");
            return keyStoreFinalKey;
        }
#endif
    }
}
