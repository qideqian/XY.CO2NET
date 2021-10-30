using System;
using System.IO;
using XY.CO2NET.Cache;
using XY.CO2NET.Exceptions;
using XY.CO2NET.Helpers;
using XY.CO2NET.MessageQueue;

namespace XY.CO2NET.Trace
{
    /// <summary>
    /// XY.CO2NET 日志记录
    /// </summary>
    public class XYTrace
    {
        /// <summary>
        /// 统一日志锁名称
        /// </summary>
        const string LockName = "XYTraceLock";

        /// <summary>
        /// 记录BaseException日志时需要执行的任务
        /// </summary>
        public static Action<BaseException> OnBaseExceptionFunc;

        /// <summary>
        /// 执行所有日志记录操作时执行的任务（发生在记录日志之后）
        /// </summary>
        public static Action OnLogFunc;

        /// <summary>
        /// 是否开放每次 APM 录入的记录，默认为关闭（当 XY.CO2ENT.APM 启用时有效）
        /// </summary>
        public static bool RecordAPMLog { get; set; } = false;

        /// <summary>
        /// 是否自动解锁可能发生的日志文件被占用情况，如果为 true，则启用（触发时会立即进行 GC 操作，消耗部分系统资源，一般不会影响系统整体性能），如果为 false，则在文件使用冲突发生时抛出异常，放弃日志写入。
        /// （注意：如果多个站点或应用程序使用相同的日志文件目录，请务必开启此项）
        /// </summary>
        public static bool AutoUnlockLogFile { get; set; } = true;

        #region 私有方法

        /// <summary>
        /// XY.Weixin全局统一的缓存策略
        /// </summary>
        private static IBaseObjectCacheStrategy Cache
        {
            get
            {
                //使用工厂模式或者配置进行动态加载
                return CacheStrategyFactory.GetObjectCacheStrategyInstance();
            }
        }

        /// <summary>
        /// 队列执行逻辑
        /// </summary>
        protected static Action<string> _queue = async (logStr) =>
        {
            using (await Cache.BeginCacheLockAsync(LockName, "").ConfigureAwait(false))
            {
                string logDir;
                logDir = Path.Combine(Config.RootDictionaryPath, "App_Data", "XYTraceLog");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                string logFile = Path.Combine(logDir, string.Format("XYTrace-{0}.log", SystemTime.Now.ToString("yyyyMMdd")));

                //判断文件被占用情况
                if (AutoUnlockLogFile)
                {
                    const int maxRetryTimes = 3;//最大重试次数
                    const int retryDelayTimeMillinSeconds = 100;//每次重试后等待的时间（毫秒）

                    //立即进行回收
                    for (int i = 0; i < maxRetryTimes; i++)
                    {
                        if (FileHelper.FileInUse(logFile))
                        {
                            //说明：
                            //1、回收文件占用的方法有许多方案，以下两行 GC 命令同时使用的参考见：https://stackoverflow.com/questions/4128211/system-io-ioexception-the-process-cannot-access-the-file-file-name
                            //2、针对 .NET Core 如果有更好的差异化的方法，欢迎 PR：https://github.com/JeffreySu/WeiXinMPSDK。
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            var dt = SystemTime.Now;
                            if (i < maxRetryTimes - 1)
                            {
                                while (SystemTime.NowDiff(dt).TotalMilliseconds < retryDelayTimeMillinSeconds)
                                {
                                    //如果不是最后一次尝试，则等待一段时间再进入下一步
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                try
                {
                    using (var fs = new FileStream(logFile, FileMode.OpenOrCreate))
                    {
                        using (var sw = new StreamWriter(fs))
                        {
                            fs.Seek(0, SeekOrigin.End);
                            await sw.WriteAsync(logStr);
                            await sw.FlushAsync();
                        }
                    }
                }
                catch (Exception)
                {
                    //写入失败
                    //throw;
                }

                if (OnLogFunc != null)
                {
                    try
                    {
                        OnLogFunc();
                    }
                    catch
                    {
                    }
                }
            }
        };

        /// <summary>
        /// 结束日志记录
        /// 将待写入加入到队列
        /// </summary>
        protected static Action<XYTraceItem> _logEndActon = (traceItem) =>
        {
            var logStr = traceItem.GetFullLog();
            XYMessageQueue messageQueue = new XYMessageQueue();
            var key = $"{SystemTime.Now.Ticks}{traceItem.ThreadId}{logStr.Length}";//确保全局唯一
            messageQueue.Add(key, () => _queue(logStr));
        };

        #endregion

        #region 日志记录

        /// <summary>
        /// 系统日志
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Log(string message)
        {
            SendCustomLog("系统日志", message);
        }

        /// <summary>
        /// 自定义日志
        /// </summary>
        /// <param name="typeName">日志类型</param>
        /// <param name="content">日志内容</param>
        public static void SendCustomLog(string typeName, string content)
        {
            if (!Config.IsDebug)
            {
                return;
            }

            using (var traceItem = new XYTraceItem(_logEndActon, typeName, content))
            {
                //traceItem.Log(content);
            }
        }

        /// <summary>
        /// API请求日志（接收结果）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="returnText"></param>
        public static void SendApiLog(string url, string returnText)
        {
            if (!Config.IsDebug)
            {
                return;
            }

            using (var traceItem = new XYTraceItem(_logEndActon, "接口调用"))
            {
                traceItem.Log("URL：{0}", url);
                traceItem.Log("Result：\r\n{0}", returnText);
            }
        }

        /// <summary>
        /// API请求日志（Post发送消息）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        public static void SendApiPostDataLog(string url, string data)
        {
            if (!Config.IsDebug)
            {
                return;
            }

            using (var traceItem = new XYTraceItem(_logEndActon, "接口调用"))
            {
                traceItem.Log("URL：{0}", url);
                traceItem.Log("Post Data：\r\n{0}", data);
            }
        }

        #endregion

        #region BaseException

        /// <summary>
        /// BaseException 日志
        /// </summary>
        /// <param name="ex"></param>
        public static void BaseExceptionLog(Exception ex)
        {
            BaseExceptionLog(new BaseException(ex.Message, ex));
        }

        /// <summary>
        /// BaseException 日志
        /// </summary>
        /// <param name="ex"></param>
        public static void BaseExceptionLog(BaseException ex)
        {
            if (!Config.IsDebug)
            {
                return;
            }

            using (var traceItem = new XYTraceItem(_logEndActon, "BaseException"))
            {
                traceItem.Log(ex.GetType().Name);
                traceItem.Log("Message：{0}", ex.Message);
                traceItem.Log("StackTrace：{0}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    traceItem.Log("InnerException：{0}", ex.InnerException.Message);
                    traceItem.Log("InnerException.StackTrace：{0}", ex.InnerException.StackTrace);
                }

                if (OnBaseExceptionFunc != null)
                {
                    try
                    {
                        OnBaseExceptionFunc(ex);
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion
    }
}
