using System;
using XY.CO2NET.HttpUtility;

namespace XY.CO2NET.Exceptions
{
    /// <summary>
    /// FormFileData 的异常
    /// </summary>
    public class FileValueException : BaseException
    {
        /// <summary>
        /// FormFileData 的异常
        /// </summary>
        /// <param name="formFileData">FormFileData实体，不可为 null</param>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <param name="logged"></param>
        public FileValueException(FormFileData formFileData, string message, Exception inner = null, bool logged = false)
            : base($"FormFileData 异常：{message}，FileName：{formFileData.FileName}", inner, logged)
        {
        }
    }
}
