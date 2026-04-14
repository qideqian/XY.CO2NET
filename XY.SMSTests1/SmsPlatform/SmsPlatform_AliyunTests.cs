using Microsoft.VisualStudio.TestTools.UnitTesting;
using XY.SMS.SmsPlatform;
using System;
using System.Collections.Generic;
using System.Text;

namespace XY.SMS.SmsPlatform.Tests
{
    [TestClass()]
    public class SmsPlatform_AliyunTests
    {
        [TestMethod()]
        public void Send_WithActionActionString_LogIsWritten()
        {
            var sms = new SmsPlatform_Aliyun("testKeyId", "testKeySecret");
            bool logCalled = false;
            sms.Send(
                "TEMPLATE_CODE",
                "{\"code\":\"9999\"}",
                "签名",
                "13800000000",
                true,
                cb => {
                    logCalled = true;
                    cb("127.0.0.1");
                }
            );
            Assert.IsTrue(logCalled);
        }

        [TestMethod()]
        public void Send_WithActionString_LogIsWritten()
        {
            var sms = new SmsPlatform_Aliyun("testKeyId", "testKeySecret");
            bool logCalled = false;
            sms.Send(
                "TEMPLATE_CODE",
                "{\"code\":\"8888\"}",
                "签名",
                "13800000001",
                true,
                ip => {
                    logCalled = true;
                    var logIp = "192.168.1.1";
                    XY.SMS.SmsLogHelper.WriteLog($"测试日志IP:{logIp}");
                }
            );
            Assert.IsTrue(logCalled);
        }
    }
}