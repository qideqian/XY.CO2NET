using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Dysmsapi.Model.V20170525;
using System;
using System.Threading.Tasks;
using XY.CO2NET.Trace;

namespace XY.SMS.SmsPlatform
{
    public class SmsPlatform_Aliyun : SmsPlatform, ISmsPlatform
    {
        static String product = "Dysmsapi";//短信API产品名称（短信产品名固定，无需修改）
        static String domain = "dysmsapi.aliyuncs.com";//短信API产品域名（接口地址固定，无需修改）
        static String _accessKeyId;//你的accessKeyId，参考本文档步骤2
        static String _accessKeySecret;//你的accessKeySecret，参考本文档步骤2
        static IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", _accessKeyId, _accessKeySecret);
        //IAcsClient client = new DefaultAcsClient(profile);
        // SingleSendSmsRequest request = new SingleSendSmsRequest();
        //初始化ascClient,暂时不支持多region（请勿修改）
        static IAcsClient acsClient = new DefaultAcsClient(profile);
        static SendSmsRequest request = new SendSmsRequest();

        public SmsPlatform_Aliyun(string accessKeyId, string accessKeySecret)
        {
            _accessKeyId = accessKeyId;
            _accessKeySecret = accessKeySecret;
        }

        public override string SmsServiceAddress => "dysmsapi.aliyuncs.com";

        public override string GetLastCount()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TemplateCode">必填:短信模板-可在短信控制台中找到，发送国际/港澳台消息时，请使用国际/港澳台短信模版</param>
        /// <param name="TemplateParam">可选:模板中的变量替换JSON串,如模板内容为"亲爱的${name},您的验证码为${code}"时,此处的值为</param>
        /// <param name="SignName">短信签名</param>
        /// <param name="PhoneNumbers">必填:待发送手机号。支持以逗号分隔的形式进行批量调用，批量上限为1000个手机号码,批量调用相对于单条调用及时性稍有延迟,验证码类型的短信推荐使用单条调用的方式，发送国际/港澳台消息时，接收号码格式为00+国际区号+号码，如“0085200000000”</param>
        /// <returns></returns>
        public Task<SmsResult> Send(string TemplateCode, string TemplateParam, string SignName, string PhoneNumbers, Action<string> SaveToDatabaseCallback)
        {
            try
            {
                request.SetEndpoint("cn-hangzhou");
                request.RegionId = "cn-hangzhou";
                request.Product = product;
                request.SetProductDomain(domain);

                //必填:待发送手机号。支持以逗号分隔的形式进行批量调用，批量上限为1000个手机号码,批量调用相对于单条调用及时性稍有延迟,验证码类型的短信推荐使用单条调用的方式，发送国际/港澳台消息时，接收号码格式为00+国际区号+号码，如“0085200000000”
                request.PhoneNumbers = PhoneNumbers;
                //必填:短信签名-可在短信控制台中找到
                request.SignName = SignName;
                //必填:短信模板-可在短信控制台中找到，发送国际/港澳台消息时，请使用国际/港澳台短信模版
                request.TemplateCode = TemplateCode;
                //可选:模板中的变量替换JSON串,如模板内容为"亲爱的${name},您的验证码为${code}"时,此处的值为
                request.TemplateParam = TemplateParam;
                //可选:outId为提供给业务方扩展字段,最终在短信回执消息中将此值带回给调用者
                request.OutId = "yourOutId";
                //请求失败这里会抛ClientException异常
                SendSmsResponse sendSmsResponse = acsClient.GetAcsResponse(request);
                XYTrace.SendCustomLog("【短信发送】", $"编号：{TemplateCode}，模版：{TemplateParam}，签名：{SignName}，号码：{PhoneNumbers}。状态：{sendSmsResponse.Code}（{sendSmsResponse.Message}）。发送通道：Aliyun");
                if (SaveToDatabaseCallback != null) SaveToDatabaseCallback(sendSmsResponse.Message);
                return Task.FromResult(new SmsResult() { SmsStatus = SmsStatus.成功, MSG = sendSmsResponse.Message });
            }
            catch (ServerException e)
            {
                XYTrace.SendCustomLog("【发送短信失败】", $"编号：{TemplateCode}，模版：{TemplateParam}，签名：{SignName}，号码：{PhoneNumbers}。发送通道：Aliyun");
                if (SaveToDatabaseCallback != null) SaveToDatabaseCallback(e.Message);
                return Task.FromResult(new SmsResult() { SmsStatus = SmsStatus.未知错误, MSG = e.Message });
            }
            catch (ClientException e)
            {
                XYTrace.SendCustomLog("【发送短信失败】", $"编号：{TemplateCode}，模版：{TemplateParam}，签名：{SignName}，号码：{PhoneNumbers}。发送通道：Aliyun");
                if (SaveToDatabaseCallback != null) SaveToDatabaseCallback(e.Message);
                return Task.FromResult(new SmsResult() { SmsStatus = SmsStatus.未知错误, MSG = e.Message });
            }
            catch (Exception e)
            {
                XYTrace.SendCustomLog("【发送短信失败】", $"编号：{TemplateCode}，模版：{TemplateParam}，签名：{SignName}，号码：{PhoneNumbers}。发送通道：Aliyun");
                if (SaveToDatabaseCallback != null) SaveToDatabaseCallback(e.Message);
                return Task.FromResult(new SmsResult() { SmsStatus = SmsStatus.未知错误, MSG = e.Message });
            }
        }

        /// <summary>
        /// 请优先使用重载方法Send(string TemplateCode, string TemplateParam, string SignName, string PhoneNumbers, Action<string> SaveToDatabaseCallback)
        /// 或将TemplateCode、TemplateParam通过#拼接到MSGContent
        /// </summary>
        /// <param name="MSGContent"></param>
        /// <param name="SignName"></param>
        /// <param name="PhoneNumbers"></param>
        /// <param name="SaveToDatabaseCallback"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Task<SmsResult> Send(string MSGContent, string SignName, string PhoneNumbers, Action<string> SaveToDatabaseCallback = null)
        {
            var _split = MSGContent.Split('#');
            if (_split.Length != 2) throw new Exception("参数错误");
            return Send(MSGContent.Split('#')[0], MSGContent.Split('#')[1], SignName, PhoneNumbers, SaveToDatabaseCallback);
        }
    }
}
