namespace XY.SMS.SmsPlatform
{
    /// <summary>
    /// 短信配置
    /// </summary>
    public class XYSmsSetting
    {
        /// <summary>
        /// 短信平台是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// 短信平台账号ID
        /// </summary>
        public string SmsAccountCORPID { get; set; }
        /// <summary>
        /// 账户名
        /// </summary>
        public string SmsAccountName { get; set; }
        /// <summary>
        /// 子帐号
        /// </summary>
        public string SmsAccountSubNumber { get; set; }
        /// <summary>
        /// 账号密码
        /// </summary>
        public string SmsAccountPassword { get; set; }
        /// <summary>
        /// 短信平台类型
        /// </summary>
        public SmsPlatformType SmsPlatformType { get; set; }
        /// <summary>
        /// 短信签名
        /// </summary>
        public string SignName { get; set; }
    }
}
