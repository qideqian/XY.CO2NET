using System;

namespace XY.CO2NET.Exceptions
{
    /// <summary>
    /// HttpClient 等网络请求异常
    /// </summary>
    public class HttpException : BaseException
    {
        /// <summary>
        /// 网络请求异常构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <param name="logged"></param>
        public HttpException(string message, Exception inner = null, bool logged = false) : base(message, inner, logged)
        {
        }
    }
}
