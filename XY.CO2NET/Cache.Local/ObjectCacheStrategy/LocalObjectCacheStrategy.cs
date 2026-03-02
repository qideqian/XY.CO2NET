using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
#if !NET48
using Microsoft.Extensions.Caching.Memory;
#endif

namespace XY.CO2NET.Cache
{
    /// <summary>
    /// 本地容器缓存策略
    /// </summary>
    public class LocalObjectCacheStrategy : BaseCacheStrategy, IBaseObjectCacheStrategy
    {
        #region 数据源

#if NET48
        private System.Web.Caching.Cache _cache = LocalObjectCacheHelper.LocalObjectCache;
#else
        private IMemoryCache _cache = LocalObjectCacheHelper.LocalObjectCache;
#endif

        #endregion

        #region 单例

        ///<summary>
        /// LocalCacheStrategy的构造函数
        ///</summary>
        LocalObjectCacheStrategy()
        {
        }

        //静态LocalCacheStrategy
        public static LocalObjectCacheStrategy Instance
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
            //将instance设为一个初始化的LocalCacheStrategy新实例
            internal static readonly LocalObjectCacheStrategy instance = new LocalObjectCacheStrategy();
        }

        #endregion

        #region IObjectCacheStrategy 成员

        //public IContainerCacheStrategy ContainerCacheStrategy
        //{
        //    get { return LocalContainerCacheStrategy.Instance; }
        //}

        #region 同步方法
        public void Set(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            if (key == null || value == null)
            {
                return;
            }

            var finalKey = base.GetFinalKey(key, isFullKey);

#if NET48
            _cache[finalKey] = value;
#else
            var newKey = !CheckExisted(finalKey, true);

            if (expiry.HasValue)
            {
                _cache.Set(finalKey, value, expiry.Value);
            }
            else
            {
                _cache.Set(finalKey, value);
            }

            //由于MemoryCache不支持遍历Keys，所以需要单独储存
            if (newKey)
            {
                var keyStoreFinalKey = LocalObjectCacheHelper.GetKeyStoreKey(this);
                List<string> keys;
                if (!CheckExisted(keyStoreFinalKey, true))
                {
                    keys = new List<string>();
                }
                else
                {
                    keys = _cache.Get<List<string>>(keyStoreFinalKey);
                }
                keys.Add(finalKey);
                _cache.Set(keyStoreFinalKey, keys);
            }
#endif
        }

        public void RemoveFromCache(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            _cache.Remove(cacheKey);

#if !NET48
            //移除key
            var keyStoreFinalKey = LocalObjectCacheHelper.GetKeyStoreKey(this);
            if (CheckExisted(keyStoreFinalKey, true))
            {
                var keys = _cache.Get<List<string>>(keyStoreFinalKey);
                keys.Remove(cacheKey);
                _cache.Set(keyStoreFinalKey, keys);
            }
#endif

        }

        public object Get(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (!CheckExisted(key, isFullKey))
            {
                return null;
                //InsertToCache(key, new ContainerItemCollection());
            }

            var cacheKey = GetFinalKey(key, isFullKey);

#if NET48
            return _cache[cacheKey];
#else
            return _cache.Get(cacheKey);
#endif
        }

        /// <summary>
        /// 返回指定缓存键的对象，并强制指定类型
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <returns></returns>
        public T Get<T>(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }

            if (!CheckExisted(key, isFullKey))
            {
                return default(T);
                //InsertToCache(key, new ContainerItemCollection());
            }
            var cacheKey = GetFinalKey(key, isFullKey);
#if NET48
            return (T)_cache[cacheKey];
#else
            return _cache.Get<T>(cacheKey);
#endif
        }

        public IDictionary<string, object> GetAll()
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
#if NET48
            IDictionaryEnumerator cacheEnum = System.Web.HttpRuntime.Cache.GetEnumerator();

            while (cacheEnum.MoveNext())
            {
                data[cacheEnum.Key as string] = cacheEnum.Value;
            }
#else
            //获取所有Key
            var keyStoreFinalKey = LocalObjectCacheHelper.GetKeyStoreKey(this);
            if (CheckExisted(keyStoreFinalKey, true))
            {
                var keys = _cache.Get<List<string>>(keyStoreFinalKey);
                foreach (var key in keys)
                {
                    data[key] = Get(key, true);
                }
            }
#endif
            return data;

        }

        public bool CheckExisted(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);

#if NET48
            return _cache.Get(cacheKey) != null;
#else
            return _cache.Get(cacheKey) != null;
#endif
        }

        public long GetCount()
        {
#if NET48
            return _cache.Count;
#else
            var keyStoreFinalKey = LocalObjectCacheHelper.GetKeyStoreKey(this);
            if (CheckExisted(keyStoreFinalKey, true))
            {
                var keys = _cache.Get<List<string>>(keyStoreFinalKey);
                return keys.Count;
            }
            else
            {
                return 0;
            }
#endif
        }

        public void Update(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            Set(key, value, expiry, isFullKey);
        }

        #endregion

        #region 异步方法
        public async Task SetAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await Task.Factory.StartNew(() => Set(key, value, expiry, isFullKey)).ConfigureAwait(false);
        }

        public async Task RemoveFromCacheAsync(string key, bool isFullKey = false)
        {
            await Task.Factory.StartNew(() => RemoveFromCache(key, isFullKey)).ConfigureAwait(false);
        }

        public async Task<object> GetAsync(string key, bool isFullKey = false)
        {
            return await Task.Factory.StartNew(() => Get(key, isFullKey)).ConfigureAwait(false);
        }

        /// <summary>
        /// 返回指定缓存键的对象，并强制指定类型
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string key, bool isFullKey = false)
        {
            return await Task.Factory.StartNew(() => Get<T>(key, isFullKey)).ConfigureAwait(false);
        }

        public async Task<IDictionary<string, object>> GetAllAsync()
        {
            return await Task.Factory.StartNew(() => GetAll()).ConfigureAwait(false);
        }

        public async Task<bool> CheckExistedAsync(string key, bool isFullKey = false)
        {
            return await Task.Factory.StartNew(() => CheckExisted(key, isFullKey)).ConfigureAwait(false);
        }

        public async Task<long> GetCountAsync()
        {
            return await Task.Factory.StartNew(() => GetCount()).ConfigureAwait(false);
        }

        public async Task UpdateAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await Task.Factory.StartNew(() => Update(key, value, expiry, isFullKey)).ConfigureAwait(false);
        }
        #endregion
        #endregion

        #region ICacheLock

        public override ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return LocalCacheLock.CreateAndLock(this, resourceName, key, retryCount, retryDelay);
        }

        public override async Task<ICacheLock> BeginCacheLockAsync(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return await LocalCacheLock.CreateAndLockAsync(this, resourceName, key, retryCount, retryDelay).ConfigureAwait(false);
        }
        #endregion
    }
}
