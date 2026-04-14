using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tea;

namespace XY.SMS.SmsPlatform
{
    public class SmsPlatform_AlibabaCloud : SmsPlatform, ISmsPlatform
    {
        static String _accessKeyId;//你的accessKeyId，参考本文档步骤2
        static String _accessKeySecret;//你的accessKeySecret，参考本文档步骤2

        public SmsPlatform_AlibabaCloud(string accessKeyId, string accessKeySecret)
        {
            _accessKeyId = accessKeyId;
            _accessKeySecret = accessKeySecret;
        }

        public override string SmsServiceAddress => throw new NotImplementedException();

        public override string GetLastCount()
        {
            throw new NotImplementedException();
        }

        public Task<SmsResult> Send(string TemplateCode, string TemplateParam, string SignName, string PhoneNumbers, Action<string> SaveToDatabaseCallback = null)
        {
            AlibabaCloud.SDK.Dysmsapi20170525.Client client = CreateClient();
            AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest sendSmsRequest = new AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest
            {
                PhoneNumbers = PhoneNumbers,
                SignName = SignName,
                //必填:短信模板-可在短信控制台中找到，发送国际/港澳台消息时，请使用国际/港澳台短信模版
                TemplateCode = TemplateCode,
                //可选:模板中的变量替换JSON串,如模板内容为"亲爱的${name},您的验证码为${code}"时,此处的值为{"name":"用户名", "code":"验证码数字"}
                TemplateParam = TemplateParam
            };
            try
            {
                // 复制代码运行请自行打印 API 的返回值
                var sendSmsResponse = client.SendSmsWithOptions(sendSmsRequest, new AlibabaCloud.TeaUtil.Models.RuntimeOptions());
                return Task.FromResult(new SmsResult() { SmsStatus = SmsStatus.成功, MSG = sendSmsResponse.Body.Message });
            }
            catch (TeaException error)
            {
                // 此处仅做打印展示，请谨慎对待异常处理，在工程项目中切勿直接忽略异常。
                // 错误 message
                Console.WriteLine(error.Message);
                // 诊断地址
                Console.WriteLine(error.Data["Recommend"]);
                AlibabaCloud.TeaUtil.Common.AssertAsString(error.Message);
                return Task.FromResult(new SmsResult() { SmsStatus = SmsStatus.未知错误, MSG = $"{error.Message}\r\n\r\n诊断地址：{error.Data["Recommend"]}" });
            }
            catch (Exception _error)
            {
                TeaException error = new TeaException(new Dictionary<string, object>
                {
                    { "message", _error.Message }
                });
                // 此处仅做打印展示，请谨慎对待异常处理，在工程项目中切勿直接忽略异常。
                // 错误 message
                Console.WriteLine(error.Message);
                // 诊断地址
                Console.WriteLine(error.Data["Recommend"]);
                AlibabaCloud.TeaUtil.Common.AssertAsString(error.Message);
                return Task.FromResult(new SmsResult() { SmsStatus = SmsStatus.未知错误, MSG = $"{error.Message}\r\n\r\n诊断地址：{error.Data["Recommend"]}" });
            }
        }

        public override Task<SmsResult> Send(string MSGContent, string SignName, string PhoneNumbers, Action<string> SaveToDatabaseCallback = null)
        {
            var _split = MSGContent.Split('#');
            if (_split.Length != 2) throw new Exception("参数错误，阿里云模版请使用“短信编码Code#短信参数”");
            return Send(MSGContent.Split('#')[0], MSGContent.Split('#')[1], SignName, PhoneNumbers, SaveToDatabaseCallback);
        }

        /// <term><b>Description:</b></term>
        /// <description>
        /// <para>使用凭据初始化账号Client</para>
        /// </description>
        /// 
        /// <returns>
        /// Client
        /// </returns>
        /// 
        /// <term><b>Exception:</b></term>
        /// Exception
        public static AlibabaCloud.SDK.Dysmsapi20170525.Client CreateClient()
        {
            // 工程代码建议使用更安全的无AK方式，凭据配置方式请参见：https://help.aliyun.com/document_detail/378671.html。
            Aliyun.Credentials.Client credential = new Aliyun.Credentials.Client();
            AlibabaCloud.OpenApiClient.Models.Config config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                AccessKeyId = _accessKeyId,
                AccessKeySecret = _accessKeySecret,
                Credential = credential
            };
            // Endpoint 请参考 https://api.aliyun.com/product/Dysmsapi
            config.Endpoint = "dysmsapi.aliyuncs.com";
            return new AlibabaCloud.SDK.Dysmsapi20170525.Client(config);
        }

        /// <summary>
        /// 发送短信（可选日志记录，Action<Action<string>>日志回调）
        /// </summary>
        public Task<SmsResult> Send(string TemplateCode, string TemplateParam, string SignName, string PhoneNumbers, bool writeLog, Action<Action<string>> logAction, Action<string> SaveToDatabaseCallback = null)
        {
            var resultTask = Send(TemplateCode, TemplateParam, SignName, PhoneNumbers, SaveToDatabaseCallback);
            if (writeLog && logAction != null)
            {
                logAction((logExtra) =>
                {
                    var logContent = $"模板Code:{TemplateCode} 参数:{TemplateParam} 签名:{SignName} 手机号:{PhoneNumbers} 额外:{logExtra}";
                    WriteSmsLog(logContent);
                });
            }
            return resultTask;
        }

        /// <summary>
        /// 发送短信（可选日志记录，直接传递日志内容）
        /// </summary>
        public Task<SmsResult> Send(string TemplateCode, string TemplateParam, string SignName, string PhoneNumbers, bool writeLog, string logExtra, Action<string> SaveToDatabaseCallback = null)
        {
            var resultTask = Send(TemplateCode, TemplateParam, SignName, PhoneNumbers, SaveToDatabaseCallback);
            if (writeLog)
            {
                var logContent = $"模板Code:{TemplateCode} 参数:{TemplateParam} 签名:{SignName} 手机号:{PhoneNumbers} 额外:{logExtra}";
                WriteSmsLog(logContent);
            }
            return resultTask;
        }
    }
}
