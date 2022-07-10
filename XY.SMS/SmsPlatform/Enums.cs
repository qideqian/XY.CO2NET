﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XY.SMS.SmsPlatform
{
    /// <summary>
    /// 短信平台类型
    /// </summary>
    public enum SmsPlatformType
    {
        Unknow = -1,
        Aliyun = 0,
        Welink = 1
    }

    /// <summary>
    /// 短信发送状态
    /// </summary>
    public enum SmsStatus
    {
        未知错误 = 0,
        成功 = 1,
        访问数据库写入数据错误 = -1,
        一次发送的手机号码过多 = -3,
        内容包含不合法文字 = -4,
        登录账户错误 = -5,
        手机号码不合法黑名单 = -9,
        号码太长不能超过100条一次提交 = -10,
        内容太长 = -11,
        余额不足 = -13,
        子号码不正确 = -14,
        短信发送服务已关闭 = -1000,
        参数不全 = -999,
        超过当天请求限制 = -998,
        请在60秒后重试 = -997
    }
}
