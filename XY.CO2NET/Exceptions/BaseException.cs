using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XY.CO2NET.Trace;

namespace XY.CO2NET.Exceptions
{
    /// <summary>
    /// 异常基类
    /// </summary>
#if NET48
    public class BaseException : ApplicationException
#else
    public class BaseException : Exception
#endif
    {
        /// <summary>
        /// BaseException 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logged"></param>
        public BaseException(string message, bool logged = false)
            : this(message, null, logged)
        {
        }

        /// <summary>
        /// BaseException
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="inner">内部异常信息</param>
        /// <param name="logged">是否已经使用WeixinTrace记录日志，如果没有，BaseException会进行概要记录</param>
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
