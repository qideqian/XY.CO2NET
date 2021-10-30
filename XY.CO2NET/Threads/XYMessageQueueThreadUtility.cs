using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XY.CO2NET.MessageQueue;

namespace XY.CO2NET.Threads
{
    /// <summary>
    /// XYMessageQueue线程自动处理
    /// </summary>
    public class XYMessageQueueThreadUtility
    {
        private readonly int _sleepMilliSeconds;

        public XYMessageQueueThreadUtility(int sleepMilliSeconds = 500)
        {
            _sleepMilliSeconds = sleepMilliSeconds;
        }

        /// <summary>
        /// 析构函数，将未处理的队列处理掉
        /// </summary>
        ~XYMessageQueueThreadUtility()
        {
            try
            {
                var mq = new XYMessageQueue();
#if NET45
                System.Diagnostics.Trace.WriteLine(string.Format("XYMessageQueueThreadUtility执行析构函数"));
                System.Diagnostics.Trace.WriteLine(string.Format("当前队列数量：{0}", mq.GetCount()));
#endif
                XYMessageQueue.OperateQueue();//处理队列
            }
            catch (Exception ex)
            {
                //此处可以添加日志
#if NET45
                System.Diagnostics.Trace.WriteLine(string.Format("XYMessageQueueThreadUtility执行析构函数错误：{0}", ex.Message));
#endif
            }
        }

        /// <summary>
        /// 启动线程轮询
        /// </summary>
        public void Run()
        {
            do
            {
                XYMessageQueue.OperateQueue();
                Thread.Sleep(_sleepMilliSeconds);
            } while (true);
        }
    }
}
