using System.Collections.Concurrent;

namespace System.Threading.Tasks
{
    /// <summary>
    /// 按键缓存延迟创建的异步任务。
    /// </summary>
    /// <remarks>
    /// <para>首次访问某个键时调用工厂，后续访问复用该键的任务；不提供过期、移除或自动重试。</para>
    /// <para>任务失败或取消后仍保留在缓存中，再次访问同一键不会重新调用工厂。</para>
    /// <para>用法示例：</para>
    /// <code>private AsyncCache&lt;string, string&gt; m_webPages = new AsyncCache&lt;string, string&gt;(DownloadStringTaskAsync);</code>
    /// https://learn.microsoft.com/zh-cn/dotnet/standard/asynchronous-programming-patterns/consuming-the-task-based-asynchronous-pattern#using-the-built-in-task-based-combinators
    /// </remarks>
    /// <typeparam name="TKey">用于区分缓存项的键类型。</typeparam>
    /// <typeparam name="TValue">异步任务完成后的结果类型。</typeparam>
    public class AsyncCache<TKey, TValue>
    {
        private readonly Func<TKey, Task<TValue>> _valueFactory;
        private readonly ConcurrentDictionary<TKey, Lazy<Task<TValue>>> _map;

        /// <summary>创建缓存，并保存按键生成任务的工厂。</summary>
        /// <param name="valueFactory">接收缓存键并返回 <see cref="Task{TValue}"/> 的工厂；创建缓存时不调用它。</param>
        /// <exception cref="ArgumentNullException"><paramref name="valueFactory"/> 为 <see langword="null"/>。</exception>
        public AsyncCache(Func<TKey, Task<TValue>> valueFactory)
        {
            if (valueFactory == null) throw new ArgumentNullException("valueFactory");
            _valueFactory = valueFactory;
            _map = new ConcurrentDictionary<TKey, Lazy<Task<TValue>>>();
        }

        /// <summary>获取指定键缓存的任务，尚未缓存时通过工厂创建。</summary>
        /// <param name="key">缓存键，不能为 <see langword="null"/>。</param>
        /// <value>工厂返回的缓存任务；任务失败或取消时也会复用。</value>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> 为 <see langword="null"/>。</exception>
        public Task<TValue> this[TKey key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException("key");
                return _map.GetOrAdd(key, toAdd =>
                    new Lazy<Task<TValue>>(() => _valueFactory(toAdd))).Value;
            }
        }
    }
}
