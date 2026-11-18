using System;
using XY.CO2NET.Trace;

namespace XY.CO2NET.Exceptions
{
    /// <summary>
    /// 异常基类
    /// </summary>
#if net48
    public class BaseException : ApplicationException
#else
    public class BaseException : Exception
#endif
    {
        /// <summary>
        /// BaseException 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logged">是否跳过构造时的 XYTrace 日志调用；为 false 时调用日志接口，实际记录受日志设置影响。</param>
        public BaseException(string message, bool logged = false)
            : this(message, null, logged)
        {
        }

        /// <summary>
        /// BaseException
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="inner">内部异常信息</param>
        /// <param name="logged">是否跳过构造时的 XYTrace.Log 和 XYTrace.BaseExceptionLog 调用；为 false 时调用，实际记录受日志设置影响</param>
        public BaseException(string message, Exception inner, bool logged = false)
            : base(message, inner)
        {
            if (!logged)
            {
                XYTrace.Log(string.Format("BaseException（{0}）：{1}", this.GetType().Name, message));
                XYTrace.BaseExceptionLog(this);
            }
        }
    }
}
