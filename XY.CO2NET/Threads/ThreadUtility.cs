using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XY.CO2NET.Threads
{
    /// <summary>
    /// 线程处理类
    /// </summary>
    public static class ThreadUtility
    {
        /// <summary>
        /// 异步线程容器
        /// </summary>
        public static Dictionary<string, Thread> AsynThreadCollection = new Dictionary<string, Thread>();//后台运行线程

        private static object AsynThreadCollectionLock = new object();

        /// <summary>
        /// 注册线程
        /// </summary>
        public static void Register()
        {
            lock (AsynThreadCollectionLock)
            {
                if (AsynThreadCollection.Count == 0)
                {
                    //队列线程
                    {
                        XYMessageQueueThreadUtility xyMessageQueue = new XYMessageQueueThreadUtility();
                        Thread xyMessageQueueThread = new Thread(xyMessageQueue.Run) { Name = "XYMessageQueue" };
                        AsynThreadCollection.Add(xyMessageQueueThread.Name, xyMessageQueueThread);
                    }
                    //其它后台线程

                    AsynThreadCollection.Values.ToList().ForEach(z =>
                    {
                        z.IsBackground = true;
                        z.Start();
                    });//全部运行
                }
            }
        }
    }
}
