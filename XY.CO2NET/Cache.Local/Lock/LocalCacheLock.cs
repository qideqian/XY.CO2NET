using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XY.CO2NET.Trace;

namespace XY.CO2NET.Cache
{
    /// <summary>
    /// 本地锁
    /// </summary>
    public class LocalCacheLock : BaseCacheLock
    {
        private LocalObjectCacheStrategy _localStrategy;

        //这里必须为非公开的构造函数，使用 Create() 方法创建
        protected LocalCacheLock(LocalObjectCacheStrategy strategy, string resourceName, string key,
            int? retryCount = null, TimeSpan? retryDelay = null)
            : base(strategy, resourceName, key, retryCount ?? 0, retryDelay ?? TimeSpan.FromMilliseconds(10))
        {
            _localStrategy = strategy;
        }

        /// <summary>
        /// 锁存放容器   TODO：考虑分布式情况
        /// </summary>
        private static Dictionary<string, object> LockPool = new Dictionary<string, object>();
        /// <summary>
        /// 随机数
        /// </summary>
        private static Random _rnd = new Random();
        /// <summary>
        /// 读取LockPool时的锁
        /// </summary>
        private static object lookPoolLock = new object();

        #region 同步方法

        /// <summary>
        /// 创建 LocalCacheLock 实例，并立即尝试获得锁
        /// </summary>
        /// <param name="strategy">LocalObjectCacheStrategy</param>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <returns></returns>
        public static ICacheLock CreateAndLock(IBaseCacheStrategy strategy, string resourceName, string key, int? retryCount = null, TimeSpan? retryDelay = null)
        {
            return new LocalCacheLock(strategy as LocalObjectCacheStrategy, resourceName, key, retryCount, retryDelay).Lock();
        }

        /// <summary>
        /// 立即等待并抢夺锁
        /// </summary>
        /// <returns></returns>
        public override ICacheLock Lock()
        {
            int currentRetry = 0;
            int maxRetryDelay = (int)_retryDelay.TotalMilliseconds;
            while (currentRetry++ < _retryCount)
            {
                #region 尝试获得锁
                var getLock = false;
                try
                {
                    lock (lookPoolLock)
                    {
                        if (LockPool.ContainsKey(_resourceName))
                        {
                            getLock = false;//已被别人锁住，没有取得锁
                        }
                        else
                        {
                            LockPool.Add(_resourceName, new object());//创建锁
                            getLock = true;//取得锁
                        }
                    }
                }
                catch (Exception ex)
                {
                    XYTrace.Log("本地同步锁发生异常：" + ex.Message);
                    getLock = false;
                }
                #endregion

                if (getLock)
                {
                    LockSuccessful = true;
                    return this;//取得锁
                }
                Thread.Sleep(_rnd.Next(maxRetryDelay));
            }
            LockSuccessful = false;
            return this;
        }

        public override void UnLock()
        {
            if (LockSuccessful)
            {
                lock (lookPoolLock)
                {
                    LockPool.Remove(_resourceName);
                }
            }
        }

        #endregion

        #region 异步方法

        /// <summary>
        /// 【异步方法】创建 LocalCacheLock 实例，并立即尝试获得锁
        /// </summary>
        /// <param name="strategy">LocalObjectCacheStrategy</param>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <returns></returns>
        public static async Task<ICacheLock> CreateAndLockAsync(IBaseCacheStrategy strategy, string resourceName, string key, int? retryCount = null, TimeSpan? retryDelay = null)
        {
            return await new LocalCacheLock(strategy as LocalObjectCacheStrategy, resourceName, key, retryCount, retryDelay).LockAsync().ConfigureAwait(false);
        }

        public override async Task<ICacheLock> LockAsync()
        {
            return Lock();//此处使用同步方法，完成锁定
        }

        public override async Task UnLockAsync()
        {
            UnLock();
            return;
        }
        #endregion
    }
}
