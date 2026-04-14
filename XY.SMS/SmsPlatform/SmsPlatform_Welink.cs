using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XY.CO2NET.Helpers;
using XY.CO2NET.Trace;
using XY.Encrypt;

namespace XY.SMS.SmsPlatform
{
    public class SmsPlatform_Welink : SmsPlatform, ISmsPlatform
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private string AccountId { get; set; }
        private string Password { get; set; }
        private string ProductId { get; set; }
        public SmsPlatform_Welink(string SmsAccountName, string SmsAccountPassword, string SmsAccountSubNumber)
        {
            AccountId = SmsAccountName;
            Password = SmsAccountPassword;
            ProductId = SmsAccountSubNumber;
        }

        public override string SmsServiceAddress => "https://api.51welink.com/EncryptionSubmit/SendSms.ashx";

        public override string GetLastCount()
        {
            throw new NotImplementedException();
        }

        public override async Task<SmsResult> Send(string MSGContent, string SignName, string PhoneNumbers, Action<string> SaveToDatabaseCallback = null)
        {
            var RegexStr = @"【[\S]+】";
            if (!Regex.IsMatch(MSGContent, RegexStr)) MSGContent += $"【{SignName}】";

            var Timestamp = GetTimeStamp();
            var Random = new Random().Next(int.MaxValue);
            var PWD = EncryptHelper.GetMD5(Password + "SMmsEncryptKey", Encoding.UTF8);
            var AccessKey = SHA1Encrypt.SHA256($"AccountId={AccountId}&PhoneNos={PhoneNumbers}&Password={PWD}&Random={Random}&Timestamp={Timestamp}");

            var requestData = new Dictionary<string, string>
            {
                ["AccountId"] = AccountId,
                ["AccessKey"] = AccessKey.ToLower(),
                ["Timestamp"] = Timestamp,
                ["Random"] = Random.ToString(),
                ["ExtendNo"] = string.Empty,
                ["ProductId"] = ProductId,
                ["PhoneNos"] = PhoneNumbers,
                ["Content"] = MSGContent
            };

            using (var response = await _httpClient.PostAsync(SmsServiceAddress, new FormUrlEncodedContent(requestData)).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject<SelinkResult>(json);
                    if (result.Result == "succ")
                    {
                        XYTrace.SendCustomLog("【短信发送】", $"发送短信成功：{MSGContent}，号码：{PhoneNumbers}。状态：{result.Result}（{result.Reason}）。发送通道：51welink");
                        if (SaveToDatabaseCallback != null) SaveToDatabaseCallback($"{result.Result}|{result.Reason}");
                        return new SmsResult() { SmsStatus = SmsStatus.成功, MSG = result.Reason };
                    }
                    else
                    {
                        XYTrace.SendCustomLog("【发送短信失败】", $"内容：{MSGContent}，号码：{PhoneNumbers}。状态：{result.Result}（{result.Reason}）。发送通道：51welink");
                        if (SaveToDatabaseCallback != null) SaveToDatabaseCallback(result.Reason);
                        return new SmsResult() { SmsStatus = SmsStatus.未知错误, MSG = result.Reason };
                    }
                }
                else
                {
                    XYTrace.SendCustomLog("【发送短信失败】", $"内容：{MSGContent}，号码：{PhoneNumbers}。发送通道：51welink");
                    if (SaveToDatabaseCallback != null) SaveToDatabaseCallback("请求异常！");
                    return new SmsResult() { SmsStatus = SmsStatus.未知错误 };
                }
            }
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            //TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            //return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        public class SelinkResult
        {
            /// <summary>
            /// 返回状态值，返回succ标识提交成功
            /// </summary>
            public string Result { get; set; }
            /// <summary>
            /// 返回状态描述
            /// </summary>
            public string Reason { get; set; }
            /// <summary>
            /// 信息批次号
            /// </summary>
            public long MsgId { get; set; }
            /// <summary>
            /// 单条短信内容拆分条数
            /// </summary>
            public byte SplitCount { get; set; }
        }

        /// <summary>
        /// 发送短信（可选日志记录，Action<Action<string>>日志回调）
        /// </summary>
        public Task<SmsResult> Send(string MSGContent, string SignName, string PhoneNumbers, bool writeLog, Action<Action<string>> logAction, Action<string> SaveToDatabaseCallback = null)
        {
            var resultTask = Send(MSGContent, SignName, PhoneNumbers, SaveToDatabaseCallback);
            if (writeLog && logAction != null)
            {
                logAction((logExtra) =>
                {
                    var logContent = $"内容:{MSGContent} 签名:{SignName} 手机号:{PhoneNumbers} 额外:{logExtra}";
                    WriteSmsLog(logContent);
                });
            }
            return resultTask;
        }

        /// <summary>
        /// 发送短信（可选日志记录，直接传递日志内容）
        /// </summary>
        public Task<SmsResult> Send(string MSGContent, string SignName, string PhoneNumbers, bool writeLog, string logExtra, Action<string> SaveToDatabaseCallback = null)
        {
            var resultTask = Send(MSGContent, SignName, PhoneNumbers, SaveToDatabaseCallback);
            if (writeLog)
            {
                var logContent = $"内容:{MSGContent} 签名:{SignName} 手机号:{PhoneNumbers} 额外:{logExtra}";
                WriteSmsLog(logContent);
            }
            return resultTask;
        }
    }
}
