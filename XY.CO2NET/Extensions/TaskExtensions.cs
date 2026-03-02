using System.Collections.Generic;
using System.Linq;

namespace System.Threading.Tasks
{
    /// <summary>
    /// 任务扩展
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// 如果失败，重试指定次数
        /// string pageContents = await RetryOnFault(() => DownloadStringTaskAsync(url), 3);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function">执行的方法</param>
        /// <param name="maxTries">重试次数</param>
        /// <returns></returns>
        public static T RetryOnFault<T>(Func<T> function, int maxTries)
        {
            for (int i = 0; i < maxTries; i++)
            {
                try { return function(); }
                catch { if (i == maxTries - 1) throw; }
            }
            return default(T);
        }

        /// <summary>
        /// 如果失败，重试指定次数，返回任务
        /// string pageContents = await RetryOnFault(() => DownloadStringTaskAsync(url), 3);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function">执行的方法</param>
        /// <param name="maxTries">重试次数</param>
        /// <returns></returns>
        public static async Task<T> RetryOnFault<T>(Func<Task<T>> function, int maxTries)
        {
            for (int i = 0; i < maxTries; i++)
            {
                try { return await function().ConfigureAwait(false); }
                catch { if (i == maxTries - 1) throw; }
            }
            return default(T);
        }

        /// <summary>
        /// 如果失败，重试指定次数，返回任务
        /// string pageContents = await RetryOnFault(() => DownloadStringTaskAsync(url), 3, () => Task.Delay(1000));
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function">执行的方法</param>
        /// <param name="maxTries">重试次数</param>
        /// <param name="retryWhen">在重试间隔期间调用以确定何时重试该操作</param>
        /// <returns></returns>
        public static async Task<T> RetryOnFault<T>(Func<Task<T>> function, int maxTries, Func<Task> retryWhen)
        {
            for (int i = 0; i < maxTries; i++)
            {
                try { return await function().ConfigureAwait(false); }
                catch { if (i == maxTries - 1) throw; }
                await retryWhen().ConfigureAwait(false);
            }
            return default(T);
        }

        /// <summary>
        /// 利用冗余改进操作延迟和提高成功的可能性，多个任务只要有一个成功即返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functions"></param>
        /// <returns></returns>
        public static async Task<T> NeedOnlyOne<T>(params Func<CancellationToken, Task<T>>[] functions)
        {
            var cts = new CancellationTokenSource();
            var tasks = (from function in functions
                         select function(cts.Token)).ToArray();
            var completed = await Task.WhenAny(tasks).ConfigureAwait(false);
            cts.Cancel();
            foreach (var task in tasks)
            {
                var ignored = task.ContinueWith(t => { }, TaskContinuationOptions.OnlyOnFaulted);
            }
            return await completed;
        }

        /// <summary>
        /// 交错操作
        /// 处理大型任务集时，如果使用 WhenAny 方法支持交错方案，可能存在潜在性能问题。
        /// 每次调用 WhenAny 都会向每个任务注册延续。 对于 N 个任务，这将导致在交错操作的操作期间创建 O(N2) 次延续。
        /// 如果处理大型任务集，则可以使用连结符（以下示例中的 Interleaved）来解决性能问题
        /// 
        /// IEnumerable<Task<int>> tasks = ...;
        /// foreach(var task in Interleaved(tasks))
        /// {
        ///     int result = await task;
        ///     …
        /// }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tasks"></param>
        /// <returns></returns>
        static IEnumerable<Task<T>> Interleaved<T>(IEnumerable<Task<T>> tasks)
        {
            var inputTasks = tasks.ToList();
            var sources = (from _ in Enumerable.Range(0, inputTasks.Count)
                           select new TaskCompletionSource<T>()).ToList();
            int nextTaskIndex = -1;
            foreach (var inputTask in inputTasks)
            {
                inputTask.ContinueWith(completed =>
                {
                    var source = sources[Interlocked.Increment(ref nextTaskIndex)];
                    if (completed.IsFaulted)
                        source.TrySetException(completed.Exception.InnerExceptions);
                    else if (completed.IsCanceled)
                        source.TrySetCanceled();
                    else
                        source.TrySetResult(completed.Result);
                }, CancellationToken.None,
                   TaskContinuationOptions.ExecuteSynchronously,
                   TaskScheduler.Default);
            }
            return from source in sources
                   select source.Task;
        }

        /// <summary>
        /// 等待集中的所有任务，除非某个任务发生错误
        /// 异常发生时停止等待
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task<T[]> WhenAllOrFirstException<T>(IEnumerable<Task<T>> tasks)
        {
            var inputs = tasks.ToList();
            var ce = new CountdownEvent(inputs.Count);
            var tcs = new TaskCompletionSource<T[]>();

            Action<Task> onCompleted = (Task completed) =>
            {
                if (completed.IsFaulted)
                    tcs.TrySetException(completed.Exception.InnerExceptions);
                if (ce.Signal() && !tcs.Task.IsCompleted)
                    tcs.TrySetResult(inputs.Select(t => t.Result).ToArray());
            };

            foreach (var t in inputs) t.ContinueWith(onCompleted);
            return tcs.Task;
        }
    }
}
