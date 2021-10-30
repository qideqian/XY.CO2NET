using Newtonsoft.Json;

namespace XY
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// 把数据转换为Json格式（使用Newtonsoft.Json.dll）
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="indented">是否使用缩进格式</param>
        /// <param name="jsonSerializerSettings">序列化设置（默认为null）</param>
        /// <returns></returns>
        public static string ToJson(this object data, bool indented = false, JsonSerializerSettings jsonSerializerSettings = null)
        {
            var formatting = indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;
            return Newtonsoft.Json.JsonConvert.SerializeObject(data, formatting, jsonSerializerSettings);
        }

        /// <summary>
        /// string.IsNullOrWhiteSpace()的扩展方法
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// string.IsNullOrEmpty()的扩展方法
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// string.Format()的扩展方法
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
    }
}
