#if NET45
using System.Net;
#else
using System.Net.Http;
#endif

namespace XY.CO2NET.HttpUtility
{
    /// <summary>
    /// 统一封装HttpResonse请求，提供Http请求过程中的调试、跟踪等扩展能力
    /// </summary>
    public class XYHttpResponse
    {
#if NET45
        public HttpWebResponse Result { get; set; }

        public XYHttpResponse(HttpWebResponse httpWebResponse)
        {
            Result = httpWebResponse;
        }
#else
        public HttpResponseMessage Result { get; set; }

        public XYHttpResponse(HttpResponseMessage httpWebResponse)
        {
            Result = httpWebResponse;
        }
#endif
    }
}
