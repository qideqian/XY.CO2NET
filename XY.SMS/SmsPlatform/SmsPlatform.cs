using System;
using System.Text;
using System.Threading.Tasks;

namespace XY.SMS.SmsPlatform
{
    public interface ISmsPlatform
    {
        string SmsServiceAddress { get; }

        XYSmsSetting Setting { get; set; }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="MSGContent"></param>
        /// <param name="SignName"></param>
        /// <param name="PhoneNumbers"></param>
        /// <param name="SaveToDatabaseCallback">发送回调，传入发送结果</param>
        /// <returns></returns>
        Task<SmsResult> Send(string MSGContent, string SignName, string PhoneNumbers, Action<string> SaveToDatabaseCallback = null);

        /// <summary>
        /// 获取剩余短信数量
        /// </summary>
        /// <returns></returns>
        string GetLastCount();
    }

    public abstract class SmsPlatform : ISmsPlatform
    {
        public abstract string SmsServiceAddress { get; }
        public XYSmsSetting Setting { get; set; }
        public abstract Task<SmsResult> Send(string MSGContent, string SignName, string PhoneNumbers, Action<string> SaveToDatabaseCallback = null);
        public abstract string GetLastCount();
        protected string UrlEncode(string url, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            StringBuilder sb = new StringBuilder();
            byte[] bytes = encoding.GetBytes(url);
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(bytes[i], 16));
            }
            return (sb.ToString());
        }
    }
}
