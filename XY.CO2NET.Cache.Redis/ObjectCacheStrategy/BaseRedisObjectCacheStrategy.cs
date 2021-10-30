using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XY.CO2NET.Cache.Redis
{
    /// <summary>
    /// 所有Redis基础缓存策略的基类
    /// </summary>
    public abstract class BaseRedisObjectCacheStrategy : BaseCacheStrategy, IBaseObjectCacheStrategy
    {
        public ConnectionMultiplexer RedisClient { get; set; }
        protected IDatabase _cacheDatabase;

        protected BaseRedisObjectCacheStrategy()
        {
            RedisClient = RedisManager.Manager;
            _cacheDatabase = RedisClient.GetDatabase();
        }

        static BaseRedisObjectCacheStrategy()
        {
            //自动注册连接字符串信息
            if (string.IsNullOrEmpty(RedisManager.ConfigurationOption) &&
                !string.IsNullOrEmpty(Config.XYSetting.Cache_Redis_Configuration) &&
                Config.XYSetting.Cache_Redis_Configuration != "Redis配置" &&
                Config.XYSetting.Cache_Redis_Configuration != "#{Cache_Redis_Configuration}#")
            {
                RedisManager.ConfigurationOption = Config.XYSetting.Cache_Redis_Configuration;
            }
        }

        /// <summary>
        /// Redis 缓存策略析构函数，用于 _client 资源回收
        /// </summary>
        ~BaseRedisObjectCacheStrategy()
        {
            RedisClient?.Dispose();//释放
        }

        /// <summary>
        /// 获取 Server 对象
        /// </summary>
        /// <returns></returns>
        protected IServer GetServer()
        {
            //https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/KeysScan.md
            var server = RedisClient.GetServer(RedisClient.GetEndPoints()[0]);
            return server;
        }

        #region 同步方法

        public abstract void Set(string key, object value, TimeSpan? expiry = null, bool isFullKey = false);

        public abstract void RemoveFromCache(string key, bool isFullKey = false);

        public abstract object Get(string key, bool isFullKey = false);

        public abstract T Get<T>(string key, bool isFullKey = false);

        public abstract IDictionary<string, object> GetAll();

        public abstract bool CheckExisted(string key, bool isFullKey = false);

        public abstract long GetCount();

        public abstract void Update(string key, object value, TimeSpan? expiry = null, bool isFullKey = false);

        #endregion

        #region 异步方法

        public abstract Task SetAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false);

        public abstract Task RemoveFromCacheAsync(string key, bool isFullKey = false);

        public abstract Task<object> GetAsync(string key, bool isFullKey = false);

        public abstract Task<T> GetAsync<T>(string key, bool isFullKey = false);

        public abstract Task<IDictionary<string, object>> GetAllAsync();

        public abstract Task<bool> CheckExistedAsync(string key, bool isFullKey = false);

        public abstract Task<long> GetCountAsync();

        public abstract Task UpdateAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false);

        #endregion

        public override ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return RedisCacheLock.CreateAndLock(this, resourceName, key, retryCount, retryDelay);
        }

        public override async Task<ICacheLock> BeginCacheLockAsync(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return await RedisCacheLock.CreateAndLockAsync(this, resourceName, key, retryCount, retryDelay).ConfigureAwait(false);
        }
    }
}
