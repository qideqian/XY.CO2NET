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
        /// 关闭状态处理：不发送短信；按条件调用日志委托和回调，正常结束时返回已完成的关闭状态任务。
        /// </summary>
        /// <remarks>writeLog 为 true 且 logAction 非 null 时，同步调用日志委托；委托可选择是否调用传入的写日志动作。随后同步调用非 null 的 callback。同步抛出的异常会阻止正常返回任务。</remarks>
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
        /// 关闭状态处理：不发送短信；writeLog 为 true 时调用日志方法，随后调用非 null 的回调，正常结束时返回已完成的关闭状态任务。
        /// </summary>
        /// <remarks>日志方法和回调在返回任务前同步调用；同步抛出的异常会阻止正常返回任务。</remarks>
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

        /// <summary>不发送短信；同步调用非 null 的回调，正常结束时返回已完成的关闭状态任务。</summary>
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
