using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XY.CO2NET.Trace;
using XY.Encrypt;

namespace XY.SMS.SmsPlatform
{
    public class SmsPlatform_Welink : SmsPlatform, ISmsPlatform
    {
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

        public override Task<SmsResult> Send(string MSGContent, string SignName, string PhoneNumbers, Action<string> SaveToDatabaseCallback = null)
        {
            var RegexStr = @"【[\S]+】";
            if (!Regex.IsMatch(MSGContent, RegexStr)) MSGContent += $"【{SignName}】";

            var Timestamp = GetTimeStamp();
            var Random = new Random().Next(int.MaxValue);
            var PWD = MD5Encrypt.Encrypt(Password + "SMmsEncryptKey").ToUpper();
            var AccessKey = SHA1Encrypt.sha256($"AccountId={AccountId}&PhoneNos={PhoneNumbers}&Password={PWD}&Random={Random}&Timestamp={Timestamp}");

            UTF8Encoding encoding = new UTF8Encoding();
            byte[] postData = encoding.GetBytes($"AccountId={AccountId}&AccessKey={AccessKey.ToLower()}&Timestamp={Timestamp}&Random={Random}&ExtendNo=&ProductId={ProductId}&PhoneNos={PhoneNumbers}&Content={MSGContent}");

            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(SmsServiceAddress);
            myRequest.Method = "POST";
            myRequest.ContentType = "application/x-www-form-urlencoded";
            myRequest.ContentLength = postData.Length;

            Stream newStream = myRequest.GetRequestStream();
            newStream.Write(postData, 0, postData.Length);
            newStream.Flush();
            newStream.Close();

            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            if (myResponse.StatusCode == HttpStatusCode.OK)
            {
                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<SelinkResult>(reader.ReadToEnd());
                if (result.Result == "succ")
                {
                    XYTrace.SendCustomLog("【短信发送】", $"发送短信成功：{MSGContent}，号码：{PhoneNumbers}。状态：{result.Result}（{result.Reason}）。发送通道：51welink");
                    if (SaveToDatabaseCallback != null) SaveToDatabaseCallback($"{result.Result}|{result.Reason}");
                    return Task.FromResult(new SmsResult() { SmsStatus = SmsStatus.成功, MSG = result.Reason });
                }
                else
                {
                    XYTrace.SendCustomLog("【发送短信失败】", $"内容：{MSGContent}，号码：{PhoneNumbers}。状态：{result.Result}（{result.Reason}）。发送通道：51welink");
                    if (SaveToDatabaseCallback != null) SaveToDatabaseCallback(result.Reason);
                    return Task.FromResult(new SmsResult() { SmsStatus = SmsStatus.未知错误, MSG = result.Reason });
                }
            }
            else
            {
                XYTrace.SendCustomLog("【发送短信失败】", $"内容：{MSGContent}，号码：{PhoneNumbers}。发送通道：51welink");
                if (SaveToDatabaseCallback != null) SaveToDatabaseCallback("请求异常！");
                return Task.FromResult(new SmsResult() { SmsStatus = SmsStatus.未知错误 });
            }
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return ((int)(DateTime.Now - startTime).TotalSeconds).ToString();
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
    }
}
