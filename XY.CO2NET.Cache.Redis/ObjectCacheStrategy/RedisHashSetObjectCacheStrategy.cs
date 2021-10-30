using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XY.CO2NET.MessageQueue;

namespace XY.CO2NET.Cache.Redis
{
    /// <summary>
    /// Redis的Object类型容器缓存（Key为String类型）
    /// </summary>
    public class RedisHashSetObjectCacheStrategy : BaseRedisObjectCacheStrategy
    {
        /// <summary>
        /// Hash储存的Key和Field集合
        /// </summary>
        protected class HashKeyAndField
        {
            public string Key { get; set; }
            public string Field { get; set; }
        }

        #region 单例

        /// <summary>
        /// Redis 缓存策略
        /// </summary>
        RedisHashSetObjectCacheStrategy() : base()
        {

        }

        //静态SearchCache
        public static RedisHashSetObjectCacheStrategy Instance
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
            internal static readonly RedisHashSetObjectCacheStrategy instance = new RedisHashSetObjectCacheStrategy();
        }

        #endregion

        /// <summary>
        /// 获取Hash储存的Key和Field
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey"></param>
        /// <returns></returns>
        protected HashKeyAndField GetHashKeyAndField(string key, bool isFullKey = false)
        {
            var finalFullKey = base.GetFinalKey(key, isFullKey);
            var index = finalFullKey.LastIndexOf(":");

            if (index == -1)
            {
                index = 0;
            }

            var hashKeyAndField = new HashKeyAndField()
            {
                Key = finalFullKey.Substring(0, index),
                Field = finalFullKey.Substring(index + 1/*排除:号*/, finalFullKey.Length - index - 1)
            };
            return hashKeyAndField;
        }

        #region 实现 IBaseObjectCacheStrategy 接口

        #region 同步方法

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey">是否已经是完整的Key</param>
        /// <returns></returns>
        public override bool CheckExisted(string key, bool isFullKey = false)
        {
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            return _cacheDatabase.HashExists(hashKeyAndField.Key, hashKeyAndField.Field);
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

            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            var value = _cacheDatabase.HashGet(hashKeyAndField.Key, hashKeyAndField.Field);
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

            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            var value = _cacheDatabase.HashGet(hashKeyAndField.Key, hashKeyAndField.Field);
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
        public override IDictionary<string, object> GetAll()
        {
            var keyPrefix = GetFinalKey("");//XY:DefaultCache:前缀的Key（[DefaultCache]可配置）
            var dic = new Dictionary<string, object>();

            var hashKeys = GetServer().Keys(database: _cacheDatabase.Database, pattern: keyPrefix + "*", pageSize: 99999);
            foreach (var redisKey in hashKeys)
            {
                var list = _cacheDatabase.HashGetAll(redisKey);

                foreach (var hashEntry in list)
                {
                    var fullKey = redisKey.ToString() + ":" + hashEntry.Name;//最完整的finalKey（可用于LocalCache），还原完整Key，格式：[命名空间]:[Key]
                    dic[fullKey] = hashEntry.Value.ToString().DeserializeFromCache();
                }
            }
            return dic;
        }

        /// <summary>
        /// 获取所有缓存项计数（最多 99999 条）
        /// </summary>
        /// <returns></returns>
        public override long GetCount()
        {
            var keyPattern = GetFinalKey("*");//获取带XY:DefaultCache:前缀的Key（[DefaultCache]
            var count = GetServer().Keys(database: _cacheDatabase.Database, pattern: keyPattern, pageSize: 99999).Count();
            return count;
        }

        /// <summary>
        /// 设置对象。注意：过期时间对 HashSet 无效！
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="isFullKey"></param>
        /// <param name="expiry"></param>
        public override void Set(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return;
            }

            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            var json = value.SerializeToCache();
            _cacheDatabase.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, json);
        }

        public override void RemoveFromCache(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var hashKeyAndField = this.GetHashKeyAndField(key);

            XYMessageQueue.OperateQueue();//延迟缓存立即生效
            _cacheDatabase.HashDelete(hashKeyAndField.Key, hashKeyAndField.Field);//删除项
        }

        /// <summary>
        /// 更新对象。注意：过期时间对 HashSet 无效！
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="isFullKey"></param>
        /// <param name="expiry"></param>
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
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);
            return await _cacheDatabase.HashExistsAsync(hashKeyAndField.Key, hashKeyAndField.Field).ConfigureAwait(false);
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
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);
            var value = await _cacheDatabase.HashGetAsync(hashKeyAndField.Key, hashKeyAndField.Field).ConfigureAwait(false);
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
            }
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);
            var value = await _cacheDatabase.HashGetAsync(hashKeyAndField.Key, hashKeyAndField.Field).ConfigureAwait(false);
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
            var keyPrefix = GetFinalKey("");//XY:DefaultCache:前缀的Key（[DefaultCache]可配置）
            var dic = new Dictionary<string, object>();

            var hashKeys = GetServer().Keys(database: _cacheDatabase.Database, pattern: keyPrefix + "*", pageSize: 99999);
            foreach (var redisKey in hashKeys)
            {
                var list = await _cacheDatabase.HashGetAllAsync(redisKey).ConfigureAwait(false);

                foreach (var hashEntry in list)
                {
                    var fullKey = redisKey.ToString() + ":" + hashEntry.Name;//最完整的finalKey（可用于LocalCache），还原完整Key，格式：[命名空间]:[Key]
                    dic[fullKey] = hashEntry.Value.ToString().DeserializeFromCache();
                }
            }
            return dic;
        }

        public override Task<long> GetCountAsync()
        {
            return Task.Factory.StartNew(() => GetCount());
        }

        /// <summary>
        /// 设置对象。注意：过期时间对 HashSet 无效！
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="isFullKey"></param>
        /// <param name="expiry"></param>
        public override async Task SetAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return;
            }
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);
            var json = value.SerializeToCache();
            await _cacheDatabase.HashSetAsync(hashKeyAndField.Key, hashKeyAndField.Field, json).ConfigureAwait(false);
        }

        public override async Task RemoveFromCacheAsync(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            var hashKeyAndField = this.GetHashKeyAndField(key);
            XYMessageQueue.OperateQueue();//延迟缓存立即生效
            await _cacheDatabase.HashDeleteAsync(hashKeyAndField.Key, hashKeyAndField.Field).ConfigureAwait(false);//删除项
        }

        /// <summary>
        /// 更新对象。注意：过期时间对 HashSet 无效！
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="isFullKey"></param>
        /// <param name="expiry"></param>
        public override async Task UpdateAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await SetAsync(key, value, expiry, isFullKey).ConfigureAwait(false);
        }

        #endregion

        #endregion
    }
}
