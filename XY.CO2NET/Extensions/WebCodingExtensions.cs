using System.Net;
using System.Text;

namespace System
{
    /// <summary>
    /// 用于网页的HTML、Url编码/解码
    /// </summary>
    public static class WebCodingExtensions
    {
        /// <summary>
        /// 封装System.Web.HttpUtility.HtmlEncode
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HtmlEncode(this string html)
        {
#if NET48
            return System.Web.HttpUtility.HtmlEncode(html);
#else
            return WebUtility.HtmlEncode(html);
#endif
        }
        /// <summary>
        /// 封装System.Web.HttpUtility.HtmlDecode
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HtmlDecode(this string html)
        {
#if NET48
            return System.Web.HttpUtility.HtmlDecode(html);
#else
            return WebUtility.HtmlDecode(html);
#endif
        }

#if NET48
        /// <summary>
        /// 封装 System.Web.HttpUtility.UrlEncode
        /// <para>注意：.NET Core 转义后字母为大写</para>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding">编码，默认为 UTF8</param>
        /// <returns></returns>
        public static string UrlEncode(this string url, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return System.Web.HttpUtility.UrlEncode(url, encoding);
        }
#else
        /// <summary>
        /// 封装 WebUtility.UrlEncode
        /// <para>注意：.NET Core 转义后字母为大写</para>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string UrlEncode(this string url)
        {
            return WebUtility.UrlEncode(url);//转义后字母为大写
        }
#endif

#if NET48
        /// <summary>
        /// 封装System.Web.HttpUtility.UrlDecode
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding">编码，默认为 UTF8</param>
        /// <returns></returns>
        public static string UrlDecode(this string url, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return System.Web.HttpUtility.UrlDecode(url, encoding);
        }
#else
        /// <summary>
        /// 封装 WebUtility.UrlDecode
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string UrlDecode(this string url)
        {
            return WebUtility.UrlDecode(url);
        }
#endif

        /// <summary>
        /// <para>将 URL 中的参数名称/值编码为合法的格式。</para>
        /// <para>可以解决类似这样的问题：假设参数名为 tvshow, 参数值为 Tom&amp;Jerry，如果不编码，可能得到的网址： http://a.com/?tvshow=Tom&amp;Jerry&amp;year=1965 编码后则为：http://a.com/?tvshow=Tom%26Jerry&amp;year=1965 </para>
        /// <para>实践中经常导致问题的字符有：'&amp;', '?', '=' 等</para>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string AsUrlData(this string data)
        {
            if (data == null)
            {
                return null;
            }
            return Uri.EscapeDataString(data);
        }
    }
}
