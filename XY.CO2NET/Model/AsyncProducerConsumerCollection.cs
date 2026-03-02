using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XY.CO2NET.Model
{
    /// <summary>
    /// 异步生产者消费者集合-构建协调异步活动的数据结构
    /// https://learn.microsoft.com/zh-cn/dotnet/standard/asynchronous-programming-patterns/consuming-the-task-based-asynchronous-pattern#using-the-built-in-task-based-combinators
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncProducerConsumerCollection<T>
    {
        private readonly Queue<T> m_collection = new Queue<T>();//数据队列，数据冗余
        private readonly Queue<TaskCompletionSource<T>> m_waiting = new Queue<TaskCompletionSource<T>>();//等待队列，等待数据

        /// <summary>
        /// 生产任务
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            TaskCompletionSource<T> tcs = null;
            lock (m_collection)
            {
                if (m_waiting.Count > 0) tcs = m_waiting.Dequeue();
                else m_collection.Enqueue(item);
            }
            if (tcs != null) tcs.TrySetResult(item);
        }

        /// <summary>
        /// 获取任务数据
        /// </summary>
        /// <returns></returns>
        public Task<T> Take()
        {
            lock (m_collection)
            {
                if (m_collection.Count > 0)
                {
                    return Task.FromResult(m_collection.Dequeue());
                }
                else
                {
                    var tcs = new TaskCompletionSource<T>();
                    m_waiting.Enqueue(tcs);
                    return tcs.Task;
                }
            }
        }
    }

    /// <summary>
    /// 使用示例
    /// </summary>
    public class AsyncProducerConsumerCollectionTest
    {
        private static AsyncProducerConsumerCollection<int> m_data = new AsyncProducerConsumerCollection<int>();
        private static async Task ConsumerAsync()
        {
            while (true)
            {
                int nextItem = await m_data.Take();
                //ProcessNextItem(nextItem);
            }
        }
        private static void Produce(int data)
        {
            m_data.Add(data);
        }
    }
    public class AsyncProducerConsumerCollectionTest2
    {
        //System.Threading.Tasks.Dataflow 命名空间作为 NuGet 包提供
        //private static BufferBlock<int> m_data =
        //private static async Task ConsumerAsync()
        //{
        //    while (true)
        //    {
        //        int nextItem = await m_data.ReceiveAsync();
        //        //ProcessNextItem(nextItem);
        //    }
        //}
        //private static void Produce(int data)
        //{
        //    m_data.Post(data);
        //}
    }
}
