#if !NET48
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
#endif

namespace XY.CO2NET.Cache.Redis
{
    /// <summary>
    /// Redis 注册
    /// </summary>
    public static class Register
    {
        /// <summary>
        /// 设置连接字符串（不立即启用）
        /// </summary>
        /// <param name="redisConfigurationString"></param>
        public static void SetConfigurationOption(string redisConfigurationString)
        {
            RedisManager.ConfigurationOption = redisConfigurationString;
        }

        /// <summary>
        /// 立即使用键值对方式储存的 Redis（推荐）
        /// </summary>
        public static void UseKeyValueRedisNow()
        {
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);//键值Redis
        }

#if !NET48
        /// <summary>
        /// 立即使用键值对方式储存的 Redis（推荐），并注册 .NET Core DI
        /// </summary>
        public static IServiceCollection UseKeyValueRedisNow(this IServiceCollection serviceCollection)
        {
            return UseKeyValueRedisNow(serviceCollection, false);
        }

        /// <summary>
        /// 立即使用键值对方式储存的 Redis（推荐），并注册 .NET Core DI
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="useTryAddSingleton">是否使用 TryAddSingleton 避免重复注册</param>
        public static IServiceCollection UseKeyValueRedisNow(this IServiceCollection serviceCollection, bool useTryAddSingleton)
        {
            UseKeyValueRedisNow();

            if (useTryAddSingleton)
            {
                serviceCollection.TryAddSingleton<IRedisObjectCacheStrategy>(_ => RedisObjectCacheStrategy.Instance);
                serviceCollection.TryAddSingleton<IBaseObjectCacheStrategy>(sp => sp.GetRequiredService<IRedisObjectCacheStrategy>());
            }
            else
            {
                serviceCollection.AddSingleton<IRedisObjectCacheStrategy>(_ => RedisObjectCacheStrategy.Instance);
                serviceCollection.AddSingleton<IBaseObjectCacheStrategy>(sp => sp.GetRequiredService<IRedisObjectCacheStrategy>());
            }

            return serviceCollection;
        }

        /// <summary>
        /// 从配置文件 section "RedisSetting" 绑定并注册 RedisSetting 到 DI 容器，并设置 RedisManager.ConfigurationOption
        /// </summary>
        public static IServiceCollection AddRedisSetting(this IServiceCollection services, IConfiguration configuration)
        {
            var redisSetting = new RedisSetting();
            configuration.GetSection("RedisSetting").Bind(redisSetting);
            services.AddSingleton(redisSetting);
            if (!string.IsNullOrEmpty(redisSetting.ConfigurationOption))
            {
                RedisManager.ConfigurationOption = redisSetting.ConfigurationOption;
            }
            return services;
        }

        /// <summary>
        /// 立即使用键值对方式储存的 Redis（推荐），并使用 TryAddSingleton 避免重复注册
        /// </summary>
        public static IServiceCollection UseKeyValueRedisNowTryAdd(this IServiceCollection serviceCollection)
        {
            return UseKeyValueRedisNow(serviceCollection, true);
        }
#endif

        /// <summary>
        /// 立即使用 HashSet 方式储存的 Redis 缓存策略
        /// </summary>
        public static void UseHashRedisNow()
        {
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);//Hash格式储存的Redis
        }

#if !NET48
        /// <summary>
        /// 立即使用 HashSet 方式储存的 Redis 缓存策略，并注册 .NET Core DI
        /// </summary>
        public static IServiceCollection UseHashRedisNow(this IServiceCollection serviceCollection)
        {
            return UseHashRedisNow(serviceCollection, false);
        }

        /// <summary>
        /// 立即使用 HashSet 方式储存的 Redis 缓存策略，并注册 .NET Core DI
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="useTryAddSingleton">是否使用 TryAddSingleton 避免重复注册</param>
        public static IServiceCollection UseHashRedisNow(this IServiceCollection serviceCollection, bool useTryAddSingleton)
        {
            UseHashRedisNow();

            if (useTryAddSingleton)
            {
                serviceCollection.TryAddSingleton<IRedisObjectCacheStrategy>(_ => RedisHashSetObjectCacheStrategy.Instance);
                serviceCollection.TryAddSingleton<IBaseObjectCacheStrategy>(sp => sp.GetRequiredService<IRedisObjectCacheStrategy>());
            }
            else
            {
                serviceCollection.AddSingleton<IRedisObjectCacheStrategy>(_ => RedisHashSetObjectCacheStrategy.Instance);
                serviceCollection.AddSingleton<IBaseObjectCacheStrategy>(sp => sp.GetRequiredService<IRedisObjectCacheStrategy>());
            }

            return serviceCollection;
        }

        /// <summary>
        /// 立即使用 HashSet 方式储存的 Redis 缓存策略，并使用 TryAddSingleton 避免重复注册
        /// </summary>
        public static IServiceCollection UseHashRedisNowTryAdd(this IServiceCollection serviceCollection)
        {
            return UseHashRedisNow(serviceCollection, true);
        }
#endif
    }
}
