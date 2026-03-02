using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
#if NET48
using WebCache = System.Web.Caching;
#endif

namespace XY.CO2NET.Helpers
{
    /// <summary>
    /// 防抖、节流相关延迟的封装
    /// </summary>
    public class FunctionHelper
    {
        //public static object lock_lock = new object();//获取锁的锁
        public static ConcurrentDictionary<string, object> lockDic = new ConcurrentDictionary<string, object>();
        public static ConcurrentDictionary<string, Timer> timerDic = new ConcurrentDictionary<string, Timer>();

        /// <summary>
        /// 计时器方式防抖
        /// 延迟millisecondsDelay后执行
        /// 在此期间如果再次调用，则重新计时
        /// </summary>
        /// <param name="millisecondsDelay">延迟的毫秒数</param>
        /// <param name="action">要防抖的方法</param>
        public static void DebounceByTimer(int millisecondsDelay, Action action)
        {
            DebounceByTimer(string.Empty, millisecondsDelay, action);
        }

        /// <summary>
        /// 计时器方式防抖
        /// 延迟millisecondsDelay后执行
        /// 在此期间如果再次调用，则重新计时
        /// </summary>
        /// <param name="methodType">要防抖的方法类型 多方法使用时区分执行方法</param>
        /// <param name="millisecondsDelay">延迟的毫秒数</param>
        /// <param name="action">要防抖的方法</param>
        public static void DebounceByTimer(string methodType, int millisecondsDelay, Action action)
        {
            if (timerDic.TryGetValue(methodType, out Timer timer))
            {
                timer.Stop();
                timer.Start();
            }
            else
            {
                lock (lockDic.GetOrAdd(methodType, new object()))
                {
                    timerDic.GetOrAdd(methodType, key =>
                    {
                        var timer = new Timer(millisecondsDelay);
                        timer.AutoReset = false;
                        timer.Elapsed += (o, e) =>
                        {
                            action();
                            timer.Dispose();
                            timerDic.TryRemove(methodType, out Timer _);
                        };
                        return timer;
                    });
                }
            }
        }

#if NET48
        /// <summary>
        /// 防抖相关数据存储
        /// </summary>
        public static ConcurrentDictionary<string, WebCache.Cache> cacheDic = new ConcurrentDictionary<string, WebCache.Cache>();

        /// <summary>
        /// 缓存方式防抖（实际执行的方法会是最后一次发起时的委托方法，而不是第一次的调用方法，委托方法并不会被缓存）
        /// 延迟millisecondsDelay后执行
        /// 在此期间如果再次调用，则重新计时
        /// </summary>
        /// <param name="millisecondsDelay">延迟的毫秒数</param>
        /// <param name="action">要防抖的方法</param>
        public static void Debounce(int millisecondsDelay, Action action)
        {
            Debounce(string.Empty, millisecondsDelay, action);
        }

        /// <summary>
        /// 缓存方式防抖（实际执行的方法会是最后一次发起时的委托方法，而不是第一次的调用方法，委托方法并不会被缓存）
        /// 延迟millisecondsDelay后执行
        /// 在此期间如果再次调用，则重新计时
        /// </summary>
        /// <param name="methodType">要防抖的方法类型 多方法使用时区分执行方法</param>
        /// <param name="millisecondsDelay">延迟的毫秒数</param>
        /// <param name="action">要防抖的方法</param>
        public static void Debounce(string methodType, int millisecondsDelay, Action action)
        {
            if (cacheDic.TryGetValue(methodType, out WebCache.Cache cache))
            {
                cache.Get(methodType);
            }
            else
            {
                lock (lockDic.GetOrAdd(methodType, new object()))
                {
                    cacheDic.GetOrAdd(methodType, key =>
                    {
                        var cache = new WebCache.Cache();
                        cache.Add(methodType, string.Empty, null,
                            WebCache.Cache.NoAbsoluteExpiration,
                            TimeSpan.FromMilliseconds(millisecondsDelay),
                            WebCache.CacheItemPriority.Default,
                            new WebCache.CacheItemRemovedCallback((s, o, r) =>
                            {
                                action();
                            }));
                        return cache;
                    });
                }
            }
        }
#endif

        /// <summary>
        /// 节流相关数据存储
        /// </summary>
        public static ConcurrentDictionary<string, Task> taskDic = new ConcurrentDictionary<string, Task>();

        /// <summary>
        /// 节流
        /// 指定millisecondsDelay的时间内不会重复执行action
        /// 忽略指定时间内的其余调用
        /// </summary>
        /// <param name="millisecondsDelay">节流周期的毫秒数</param>
        /// <param name="action">要节流的方法</param>
        public static void Throttle(int millisecondsDelay, Action action)
        {
            Throttle(string.Empty, millisecondsDelay, action);
        }
        /// <summary>
        /// 节流
        /// 指定millisecondsDelay的时间内不会重复执行action
        /// 忽略指定时间内的其余调用
        /// </summary>
        /// <param name="methodType">要节流的方法类型 多方法使用时区分执行方法</param>
        /// <param name="millisecondsDelay">节流周期的毫秒数</param>
        /// <param name="action">要节流的方法</param>
        public static void Throttle(string methodType, int millisecondsDelay, Action action)
        {
            if (!taskDic.TryGetValue(methodType, out Task _))
            {
                action();
                var newTask = Task.Delay(millisecondsDelay);
                newTask.ContinueWith(_ => taskDic.TryRemove(methodType, out Task _));
                taskDic.TryAdd(methodType, newTask);
            }
        }
    }

}
