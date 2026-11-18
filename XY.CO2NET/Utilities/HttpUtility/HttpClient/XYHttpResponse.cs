#if net48
using System.Net;
#else
using System.Net.Http;
#endif

namespace XY.CO2NET.HttpUtility
{
    /// <summary>
    /// 保存 HTTP 响应对象的容器；定义 net48 编译符号时使用 HttpWebResponse，否则使用 HttpResponseMessage。
    /// 构造函数仅保存传入对象，本类不执行请求、调试或跟踪操作。
    /// </summary>
    public class XYHttpResponse
    {
#if net48
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
