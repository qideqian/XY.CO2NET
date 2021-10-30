using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XY.CO2NET.Cache.Redis
{
    public class RedisCacheLock : BaseCacheLock
    {
        private Redlock.CSharp.Redlock _dlm;
        private Redlock.CSharp.Lock _lockObject;

        private BaseRedisObjectCacheStrategy _redisStrategy;

        protected RedisCacheLock(BaseRedisObjectCacheStrategy strategy, string resourceName, string key, int? retryCount, TimeSpan? retryDelay)
            : base(strategy, resourceName, key, retryCount, retryDelay)
        {
            _redisStrategy = strategy;
        }

        #region 同步方法

        /// <summary>
        /// 创建 RedisCacheLock 实例，并立即尝试获得锁
        /// </summary>
        /// <param name="strategy">BaseRedisObjectCacheStrategy</param>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <returns></returns>
        public static ICacheLock CreateAndLock(IBaseCacheStrategy strategy, string resourceName, string key, int? retryCount = null, TimeSpan? retryDelay = null)
        {
            return new RedisCacheLock(strategy as BaseRedisObjectCacheStrategy, resourceName, key, retryCount, retryDelay).Lock();
        }

        public override ICacheLock Lock()
        {
            if (_retryCount != 0)
            {
                _dlm = new Redlock.CSharp.Redlock(_retryCount, _retryDelay, _redisStrategy.RedisClient);
            }
            else if (_dlm == null)
            {
                _dlm = new Redlock.CSharp.Redlock(_redisStrategy.RedisClient);
            }

            var ttl = base.GetTotalTtl(_retryCount, _retryDelay);
            base.LockSuccessful = _dlm.Lock(_resourceName, TimeSpan.FromMilliseconds(ttl), out _lockObject);
            return this;
        }

        public override void UnLock()
        {
            if (_lockObject != null)
            {
                _dlm.Unlock(_lockObject);
            }
        }

        #endregion

        #region 异步方法

        /// <summary>
        /// 【异步方法】创建 RedisCacheLock 实例，并立即尝试获得锁
        /// </summary>
        /// <param name="strategy">BaseRedisObjectCacheStrategy</param>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <returns></returns>
        public static async Task<ICacheLock> CreateAndLockAsync(IBaseCacheStrategy strategy, string resourceName, string key, int? retryCount = null, TimeSpan? retryDelay = null)
        {
            return await new RedisCacheLock(strategy as BaseRedisObjectCacheStrategy, resourceName, key, retryCount, retryDelay).LockAsync().ConfigureAwait(false);
        }


        public override async Task<ICacheLock> LockAsync()
        {
            if (_retryCount != 0)
            {
                _dlm = new Redlock.CSharp.Redlock(_retryCount, _retryDelay, _redisStrategy.RedisClient);
            }
            else if (_dlm == null)
            {
                _dlm = new Redlock.CSharp.Redlock(_redisStrategy.RedisClient);
            }

            var ttl = base.GetTotalTtl(_retryCount, _retryDelay);

            Tuple<bool, Redlock.CSharp.Lock> result = await _dlm.LockAsync(_resourceName, TimeSpan.FromMilliseconds(ttl)).ConfigureAwait(false);
            base.LockSuccessful = result.Item1;
            _lockObject = result.Item2;
            return this;
        }

        public override async Task UnLockAsync()
        {
            if (_lockObject != null)
            {
                await _dlm.UnlockAsync(_lockObject).ConfigureAwait(false);
            }
        }

        #endregion
    }
}
