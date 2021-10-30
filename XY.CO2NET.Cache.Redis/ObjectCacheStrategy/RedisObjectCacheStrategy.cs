using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XY.CO2NET.MessageQueue;

namespace XY.CO2NET.Cache.Redis
{
    /// <summary>
    /// Redis的Object类型容器缓存（Key为String类型），Key-Value 类型储存
    /// </summary>
    public class RedisObjectCacheStrategy : BaseRedisObjectCacheStrategy
    {
        #region 单例

        /// <summary>
        /// Redis 缓存策略
        /// </summary>
        RedisObjectCacheStrategy() : base() { }

        //静态SearchCache
        public static RedisObjectCacheStrategy Instance
        {
            get
            {
                return Nested.instance;//返回Nested类中的静态成员instance
            }
        }

        class Nested
        {
            static Nested()
            {
            }
            //将instance设为一个初始化的BaseCacheStrategy新实例
            internal static readonly RedisObjectCacheStrategy instance = new RedisObjectCacheStrategy();
        }

        #endregion

        #region 实现 IBaseObjectCacheStrategy 接口

        #region 同步接口

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey">是否已经是完整的Key</param>
        /// <returns></returns>
        public override bool CheckExisted(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            return _cacheDatabase.KeyExists(cacheKey);
        }

        public override object Get(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (!CheckExisted(key, isFullKey))
            {
                return null;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            var value = _cacheDatabase.StringGet(cacheKey);
            if (value.HasValue)
            {
                return value.ToString().DeserializeFromCache();
            }
            return value;
        }

        public override T Get<T>(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }

            if (!CheckExisted(key, isFullKey))
            {
                return default(T);
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            var value = _cacheDatabase.StringGet(cacheKey);
            if (value.HasValue)
            {
                return value.ToString().DeserializeFromCache<T>();
            }

            return default(T);
        }

        /// <summary>
        /// 注意：此方法获取的object为直接储存在缓存中，序列化之后的Value
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, object> GetAll()
        {
            var keyPrefix = GetFinalKey("");//获取带XY:DefaultCache:前缀的Key（[DefaultCache]可配置）
            var dic = new Dictionary<string, object>();

            var keys = GetServer().Keys(database: _cacheDatabase.Database, pattern: keyPrefix + "*", pageSize: 99999);
            foreach (var redisKey in keys)
            {
                dic[redisKey] = Get(redisKey, true);
            }
            return dic;
        }

        public override long GetCount()
        {
            var keyPattern = GetFinalKey("*");//获取带XY:DefaultCache:前缀的Key（[DefaultCache]
            var count = GetServer().Keys(database: _cacheDatabase.Database, pattern: keyPattern, pageSize: 99999).Count();
            return count;
        }

        public override void Set(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            var json = value.SerializeToCache();
            _cacheDatabase.StringSet(cacheKey, json, expiry);
        }

        public override void RemoveFromCache(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            XYMessageQueue.OperateQueue();//延迟缓存立即生效
            _cacheDatabase.KeyDelete(cacheKey);//删除键
        }

        public override void Update(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            Set(key, value, expiry, isFullKey);
        }

        #endregion

        #region 异步方法

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey">是否已经是完整的Key</param>
        /// <returns></returns>
        public override async Task<bool> CheckExistedAsync(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            return await _cacheDatabase.KeyExistsAsync(cacheKey).ConfigureAwait(false);
        }

        public override async Task<object> GetAsync(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (!await CheckExistedAsync(key, isFullKey).ConfigureAwait(false))
            {
                return null;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            var value = await _cacheDatabase.StringGetAsync(cacheKey).ConfigureAwait(false);
            if (value.HasValue)
            {
                return value.ToString().DeserializeFromCache();
            }
            return value;
        }

        public override async Task<T> GetAsync<T>(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }

            if (!await CheckExistedAsync(key, isFullKey).ConfigureAwait(false))
            {
                return default(T);
                //InsertToCache(key, new ContainerItemCollection());
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            var value = await _cacheDatabase.StringGetAsync(cacheKey).ConfigureAwait(false);
            if (value.HasValue)
            {
                return value.ToString().DeserializeFromCache<T>();
            }

            return default(T);
        }

        /// <summary>
        /// 注意：此方法获取的object为直接储存在缓存中，序列化之后的Value（最多 99999 条）
        /// </summary>
        /// <returns></returns>
        public override async Task<IDictionary<string, object>> GetAllAsync()
        {
            var keyPrefix = GetFinalKey("");//获取带XY:DefaultCache:前缀的Key（[DefaultCache]可配置）
            var dic = new Dictionary<string, object>();

            var keys = GetServer().Keys(database: _cacheDatabase.Database, pattern: keyPrefix + "*", pageSize: 99999);
            foreach (var redisKey in keys)
            {
                dic[redisKey] = await GetAsync(redisKey, true).ConfigureAwait(false);
            }
            return dic;
        }

        public override Task<long> GetCountAsync()
        {
            return Task.Factory.StartNew(() => GetCount());
        }

        public override async Task SetAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            var json = value.SerializeToCache();
            await _cacheDatabase.StringSetAsync(cacheKey, json, expiry).ConfigureAwait(false);
        }

        public override async Task RemoveFromCacheAsync(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            XYMessageQueue.OperateQueue();//延迟缓存立即生效
            await _cacheDatabase.KeyDeleteAsync(cacheKey).ConfigureAwait(false);//删除键
        }

        public override async Task UpdateAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await SetAsync(key, value, expiry, isFullKey).ConfigureAwait(false);
        }

        #endregion

        #endregion

        /// <summary>
        /// 根据 key 的前缀获取对象列表（最多 99999 条）
        /// </summary>
        public IList<T> GetAllByPrefix<T>(string key)
        {
            var keyPattern = GetFinalKey("*");//获取带XY:DefaultCache:前缀的Key（[DefaultCache]
            var keys = GetServer().Keys(database: _cacheDatabase.Database, pattern: keyPattern, pageSize: 99999);
            List<T> list = new List<T>();
            foreach (var fullKey in keys)
            {
                var obj = Get<T>(fullKey, true);
                if (obj != null)
                {
                    list.Add(obj);
                }
            }

            return list;
        }

        /// <summary>
        /// 【异步方法】根据 key 的前缀获取对象列表（最多 99999 条）
        /// </summary>
        public async Task<IList<T>> GetAllByPrefixAsync<T>(string key)
        {
            var keyPattern = GetFinalKey("*");//获取带XY:DefaultCache:前缀的Key（[DefaultCache]         
            var keys = GetServer().Keys(database: _cacheDatabase.Database, pattern: keyPattern, pageSize: 99999);
            List<T> list = new List<T>();
            foreach (var fullKey in keys)
            {
                var obj = await GetAsync<T>(fullKey, true).ConfigureAwait(false);
                if (obj != null)
                {
                    list.Add(obj);
                }
            }

            return list;
        }
    }
}
