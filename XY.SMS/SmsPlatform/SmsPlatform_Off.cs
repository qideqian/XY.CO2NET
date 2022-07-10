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
