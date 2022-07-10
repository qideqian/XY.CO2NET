using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XY.SMS.SmsPlatform
{
    public static class SmsPlatformFactory
    {
        /// <summary>
        /// 获取短信发送服务
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="constraintSend"></param>
        /// <returns></returns>
        public static ISmsPlatform GetSmsPlatform(IServiceProvider serviceProvider, bool constraintSend = false)
        {
            var xYSmsSetting = serviceProvider.GetService<IOptions<XYSmsSetting>>();
            return GetSmsPlatform(xYSmsSetting.Value, constraintSend);
        }

        /// <summary>
        /// 获取短信发送服务
        /// </summary>
        /// <param name="smsPlatformType"></param>
        /// <param name="constraintSend">是否强制发送，忽略短信发送开关</param>
        /// <returns></returns>
        public static ISmsPlatform GetSmsPlatform(XYSmsSetting xYSmsSetting, bool constraintSend = false)
        {
            if (xYSmsSetting == null) throw new Exception("请在Startup利用Register.AddSMSServices(..)注册短信配置");
            if (constraintSend)
                return GetSmsPlatform(xYSmsSetting.SmsAccountCORPID, xYSmsSetting.SmsAccountName, xYSmsSetting.SmsAccountPassword, xYSmsSetting.SmsAccountSubNumber, xYSmsSetting.SmsPlatformType);
            if (!xYSmsSetting.Enabled)
                return new SmsPlatform_Off();
            else
                return GetSmsPlatform(xYSmsSetting.SmsAccountCORPID, xYSmsSetting.SmsAccountName, xYSmsSetting.SmsAccountPassword, xYSmsSetting.SmsAccountSubNumber, xYSmsSetting.SmsPlatformType);
        }
        private static ISmsPlatform GetSmsPlatform(string smsAccountCorpid, string smsAccountName,
            string smsAccountPassword, string smsAccountSubNumber, SmsPlatformType smsPlatformType = SmsPlatformType.Welink)
        {
            switch (smsPlatformType)
            {
                case SmsPlatformType.Aliyun:
                    return new SmsPlatform_Aliyun(smsAccountName, smsAccountPassword);
                default:
                    return new SmsPlatform_Welink(smsAccountName, smsAccountPassword, smsAccountSubNumber);
            }
        }
    }
}
