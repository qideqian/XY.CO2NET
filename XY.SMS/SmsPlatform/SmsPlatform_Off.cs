using System;
using System.Threading.Tasks;

namespace XY.SMS.SmsPlatform
{
    public class SmsPlatform_Off : SmsPlatform, ISmsPlatform
    {
        public override string SmsServiceAddress { get; } = "";

        public SmsPlatform_Off()
        {

        }

        /// <summary>
        /// 发送短信（可选日志记录，Action<Action<string>>日志回调）
        /// </summary>
        public Task<SmsResult> Send(string MSGContent, string SignName, string PhoneNumbers, bool writeLog, Action<Action<string>> logAction, Action<string> callback)
        {
            if (writeLog && logAction != null)
            {
                logAction((logExtra) =>
                {
                    var logContent = $"内容:{MSGContent} 签名:{SignName} 手机号:{PhoneNumbers} 额外:{logExtra}";
                    WriteSmsLog(logContent);
                });
            }
            if (callback != null) callback("短信发送服务已关闭");
            return Task.FromResult(new SmsResult() { SmsStatus = SmsStatus.短信发送服务已关闭 });
        }

        /// <summary>
        /// 发送短信（可选日志记录，直接传递日志内容）
        /// </summary>
        public Task<SmsResult> Send(string MSGContent, string SignName, string PhoneNumbers, bool writeLog, string logExtra, Action<string> callback)
        {
            if (writeLog)
            {
                var logContent = $"内容:{MSGContent} 签名:{SignName} 手机号:{PhoneNumbers} 额外:{logExtra}";
                WriteSmsLog(logContent);
            }
            if (callback != null) callback("短信发送服务已关闭");
            return Task.FromResult(new SmsResult() { SmsStatus = SmsStatus.短信发送服务已关闭 });
        }

        public override Task<SmsResult> Send(string MSGContent, string SignName, string PhoneNumbers, Action<string> callback)
        {
            if (callback != null) callback("短信发送服务已关闭");
            return Task.FromResult(new SmsResult() { SmsStatus = SmsStatus.短信发送服务已关闭 });
        }

        public override string GetLastCount()
        {
            throw new NotImplementedException();
        }
    }
}
