using System;
using System.Collections.Generic;
using System.Text;

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

        /// <summary>
        /// 立即使用 HashSet 方式储存的 Redis 缓存策略
        /// </summary>
        public static void UseHashRedisNow()
        {
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);//Hash格式储存的Redis
        }
    }
}
