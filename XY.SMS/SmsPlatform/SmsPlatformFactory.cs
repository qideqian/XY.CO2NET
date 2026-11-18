using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace XY.SMS.SmsPlatform
{
    public static class SmsPlatformFactory
    {
        /// <summary>
        /// 解析短信配置并创建对应的平台实例
        /// </summary>
        /// <param name="serviceProvider">用于解析 IOptions&lt;XYSmsSetting&gt; 的服务提供者</param>
        /// <param name="constraintSend">是否在选择平台时忽略配置的 Enabled 开关</param>
        /// <returns></returns>
        public static ISmsPlatform GetSmsPlatform(IServiceProvider serviceProvider, bool constraintSend = false)
        {
            var xYSmsSetting = serviceProvider.GetService<IOptions<XYSmsSetting>>();
            return GetSmsPlatform(xYSmsSetting!.Value, constraintSend);
        }

        /// <summary>
        /// 根据配置选择并创建短信平台；关闭且未强制选择时返回 SmsPlatform_Off
        /// </summary>
        /// <param name="xYSmsSetting">用于选择平台和传递构造参数的短信配置</param>
        /// <param name="constraintSend">是否忽略 Enabled 开关，按 SmsPlatformType 选择平台</param>
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
                    return new SmsPlatform_AlibabaCloud(smsAccountName, smsAccountPassword);
                default:
                    return new SmsPlatform_Welink(smsAccountName, smsAccountPassword, smsAccountSubNumber);
            }
        }
    }
}
