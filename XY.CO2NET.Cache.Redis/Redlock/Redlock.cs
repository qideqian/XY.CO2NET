﻿using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Redlock.CSharp
{
    public class Redlock
    {
        public Redlock(int retryCount, TimeSpan retryDelay, params ConnectionMultiplexer[] list)
        {
            DefaultRetryCount = retryCount;
            DefaultRetryDelay = retryDelay;
            foreach (var item in list)
            {
                this.redisMasterDictionary.Add(item.GetEndPoints().First().ToString(), item);
            }
        }

        public Redlock(params ConnectionMultiplexer[] list)
            : this(3, new TimeSpan(0, 0, 0, 0, 200), list)
        {

        }

        readonly int DefaultRetryCount = 3;
        readonly TimeSpan DefaultRetryDelay = new TimeSpan(0, 0, 0, 0, 200);
        const double ClockDriveFactor = 0.01;

        protected int Quorum { get { return (redisMasterDictionary.Count / 2) + 1; } }

        /// <summary>
        /// String containing the Lua unlock script.
        /// </summary>
        const String UnlockScript = @"
            if redis.call(""get"",KEYS[1]) == ARGV[1] then
                return redis.call(""del"",KEYS[1])
            else
                return 0
            end";


        protected static byte[] CreateUniqueLockId()
        {
            return Guid.NewGuid().ToByteArray();
        }


        protected Dictionary<String, ConnectionMultiplexer> redisMasterDictionary = new Dictionary<string, ConnectionMultiplexer>();

        #region 同步方法

        protected bool LockInstance(string redisServer, string resource, byte[] val, TimeSpan ttl)
        {

            bool succeeded;
            try
            {
                var redis = this.redisMasterDictionary[redisServer];
                succeeded = redis.GetDatabase().StringSet(resource, val, ttl, When.NotExists);
            }
            catch (Exception)
            {
                succeeded = false;
            }
            return succeeded;
        }

        protected void UnlockInstance(string redisServer, string resource, byte[] val)
        {
            RedisKey[] key = { resource };
            RedisValue[] values = { val };
            var redis = redisMasterDictionary[redisServer];
            redis.GetDatabase().ScriptEvaluate(
                UnlockScript,
                key,
                values
                );
        }

        public bool Lock(RedisKey resource, TimeSpan ttl, out Lock lockObject)
        {
            var val = CreateUniqueLockId();
            Lock innerLock = null;
            bool successfull = retry(DefaultRetryCount, DefaultRetryDelay, () =>
            {
                try
                {
                    int n = 0;
                    var startTime = DateTime.Now;

                    // Use keys
                    for_each_redis_registered(
                        redis =>
                        {
                            if (LockInstance(redis, resource, val, ttl)) n += 1;
                        }
                    );

                    /*
                     * Add 2 milliseconds to the drift to account for Redis expires
                     * precision, which is 1 millisecond, plus 1 millisecond min drift 
                     * for small TTLs.        
                     */
                    var drift = Convert.ToInt32((ttl.TotalMilliseconds * ClockDriveFactor) + 2);
                    var validity_time = ttl - (DateTime.Now - startTime) - new TimeSpan(0, 0, 0, 0, drift);

                    if (n >= Quorum && validity_time.TotalMilliseconds > 0)
                    {
                        innerLock = new Lock(resource, val, validity_time);
                        return true;
                    }
                    else
                    {
                        for_each_redis_registered(
                            redis =>
                            {
                                UnlockInstance(redis, resource, val);
                            }
                        );
                        return false;
                    }
                }
                catch (Exception)
                { return false; }
            });

            lockObject = innerLock;
            return successfull;
        }

        protected void for_each_redis_registered(Action<ConnectionMultiplexer> action)
        {
            foreach (var item in redisMasterDictionary)
            {
                action(item.Value);
            }
        }

        protected void for_each_redis_registered(Action<String> action)
        {
            foreach (var item in redisMasterDictionary)
            {
                action(item.Key);
            }
        }

        protected bool retry(int retryCount, TimeSpan retryDelay, Func<bool> action)
        {
            int maxRetryDelay = (int)retryDelay.TotalMilliseconds;
            Random rnd = new Random();
            int currentRetry = 0;

            while (currentRetry++ < retryCount)
            {
                if (action()) return true;
                Thread.Sleep(rnd.Next(maxRetryDelay));
            }
            return false;
        }

        public void Unlock(Lock lockObject)
        {
            for_each_redis_registered(redis =>
            {
                UnlockInstance(redis, lockObject.Resource, lockObject.Value);
            });
        }

        #endregion

        #region 异步方法

        protected async Task<bool> LockInstanceAsync(string redisServer, string resource, byte[] val, TimeSpan ttl)
        {

            bool succeeded;
            try
            {
                var redis = this.redisMasterDictionary[redisServer];
                succeeded = await redis.GetDatabase().StringSetAsync(resource, val, ttl, When.NotExists).ConfigureAwait(false);
            }
            catch (Exception)
            {
                succeeded = false;
            }
            return succeeded;
        }

        protected async Task UnlockInstanceAsync(string redisServer, string resource, byte[] val)
        {
            RedisKey[] key = { resource };
            RedisValue[] values = { val };
            var redis = redisMasterDictionary[redisServer];
            await redis.GetDatabase().ScriptEvaluateAsync(
                   UnlockScript,
                   key,
                   values
                   ).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="ttl"></param>
        /// <returns>bool：successfull，Lock：lockObject</returns>
        public async Task<Tuple<bool, Lock>> LockAsync(RedisKey resource, TimeSpan ttl)
        {
            var val = CreateUniqueLockId();
            Lock innerLock = null;
            bool successfull = await retryAsync(DefaultRetryCount, DefaultRetryDelay, async () =>
            {
                await Task.CompletedTask;
                try
                {
                    int n = 0;
                    var startTime = DateTime.Now;

                    // Use keys
                    for_each_redis_registered(
                          async redis =>
                          {
                              if (await LockInstanceAsync(redis, resource, val, ttl).ConfigureAwait(false)) n += 1;
                          }
                       );

                    /*
                     * Add 2 milliseconds to the drift to account for Redis expires
                     * precision, which is 1 millisecond, plus 1 millisecond min drift 
                     * for small TTLs.        
                     */
                    var drift = Convert.ToInt32((ttl.TotalMilliseconds * ClockDriveFactor) + 2);
                    var validity_time = ttl - (DateTime.Now - startTime) - new TimeSpan(0, 0, 0, 0, drift);

                    if (n >= Quorum && validity_time.TotalMilliseconds > 0)
                    {
                        innerLock = new Lock(resource, val, validity_time);
                        return true;
                    }
                    else
                    {
                        for_each_redis_registered(
                             async redis =>
                             {
                                 await UnlockInstanceAsync(redis, resource, val).ConfigureAwait(false);
                             }
                          );
                        return false;
                    }
                }
                catch (Exception)
                { return false; }
            }).ConfigureAwait(false);

            return Tuple.Create(successfull, innerLock);
        }

        protected async Task for_each_redis_registeredAsync(Action<ConnectionMultiplexer> action)
        {
            await Task.Factory.StartNew(() => for_each_redis_registered(action)).ConfigureAwait(false);
        }

        protected async Task for_each_redis_registeredAsync(Action<String> action)
        {
            await Task.Factory.StartNew(() => for_each_redis_registered(action)).ConfigureAwait(false);

        }

        protected async Task<bool> retryAsync(int retryCount, TimeSpan retryDelay, Func<Task<bool>> action)
        {
            int maxRetryDelay = (int)retryDelay.TotalMilliseconds;
            Random rnd = new Random();
            int currentRetry = 0;

            while (currentRetry++ < retryCount)
            {
                if (await action().ConfigureAwait(false)) return true;
                Thread.Sleep(rnd.Next(maxRetryDelay));
            }
            return false;
        }

        public async Task UnlockAsync(Lock lockObject)
        {
            await for_each_redis_registeredAsync(async redis =>
            {
                await UnlockInstanceAsync(redis, lockObject.Resource, lockObject.Value).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(this.GetType().FullName);

            sb.AppendLine("Registered Connections:");
            foreach (var item in redisMasterDictionary)
            {
                sb.AppendLine(item.Value.GetEndPoints().First().ToString());
            }

            return sb.ToString();
        }
    }
}
