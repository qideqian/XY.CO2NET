using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XY.CO2NET.Exceptions
{
    /// <summary>
    /// 缓存异常
    /// </summary>
    public class CacheException : BaseException
    {
        /// <summary>
        /// 缓存异常构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <param name="logged"></param>
        public CacheException(string message, Exception inner = null, bool logged = false) : base(message, inner, logged)
        {
        }
    }
}
