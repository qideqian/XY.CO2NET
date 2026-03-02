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
    /// 事例模式，需要保证调用一个实例下的 防抖/节流 方法
    /// </summary>
    public class Function2Helper
    {
        private Function2Helper() { }

        public static Function2Helper Instance = new Function2Helper();

        public object lock_o = new object();
        public Timer timer;

        /// <summary>
        /// 计时器方式防抖
        /// 延迟millisecondsDelay后执行
        /// 在此期间如果再次调用，则重新计时
        /// </summary>
        /// <param name="millisecondsDelay">延迟的毫秒数</param>
        /// <param name="action">要防抖的方法</param>
        public void DebounceByTimer(int millisecondsDelay, Action action)
        {
            lock (lock_o)
            {
                if (timer == null)
                {
                    timer = new Timer(millisecondsDelay);
                    timer.AutoReset = false;
                    timer.Elapsed += (o, e) =>
                    {
                        action();// 如果action执行耗时较久是否提到timer清空之前？
                        timer.Stop();
                        timer.Close();
                        timer = null;
                        //action();// 如果action执行耗时较久是否提到timer清空之前？
                    };
                }
                timer.Stop();
                timer.Start();
            }
        }

#if NET48
        /// <summary>
        /// 防抖相关数据存储
        /// </summary>
        public WebCache.Cache cache;

        /// <summary>
        /// 缓存方式防抖（实际执行的方法会是最后一次发起时的委托方法，而不是第一次的调用方法，委托方法并不会被缓存）
        /// 延迟millisecondsDelay后执行
        /// 在此期间如果再次调用，则重新计时
        /// </summary>
        /// <param name="millisecondsDelay">延迟的毫秒数</param>
        /// <param name="action">要防抖的方法</param>
        public void Debounce(int millisecondsDelay, Action action)
        {
            lock (lock_o)
            {
                if (cache == null)
                {
                    cache = new WebCache.Cache();
                    cache.Add(string.Empty, string.Empty, null,
                        WebCache.Cache.NoAbsoluteExpiration,
                        TimeSpan.FromMilliseconds(millisecondsDelay),
                        WebCache.CacheItemPriority.Default,
                        new WebCache.CacheItemRemovedCallback((s, o, r) =>
                        {
                            cache = null;
                            action();
                        }));
                }
                cache.Get(string.Empty);
            }
        }
#endif

        /// <summary>
        /// 节流相关数据存储
        /// </summary>
        public Task task;

        /// <summary>
        /// 节流
        /// 指定millisecondsDelay的时间内不会重复执行action
        /// 忽略指定时间内的其余调用
        /// </summary>
        /// <param name="millisecondsDelay">节流周期的毫秒数</param>
        /// <param name="action">要节流的方法</param>
        public void Throttle(int millisecondsDelay, Action action)
        {
            lock (lock_o)
            {
                if (task == null)
                {
                    action();
                    var newTask = Task.Delay(millisecondsDelay);
                    newTask.ContinueWith(_ => task = null);
                    task = newTask;
                }
            }
        }
    }
}
