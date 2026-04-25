namespace XY.OCR.AlibabaCloud
{
    /// <summary>
    /// 阿里云 OCR 配置选项。
    /// </summary>
    public class XYAlibabaCloudOCROptions
    {
        /// <summary>
        /// 默认配置节名称。
        /// </summary>
        public const string DefaultSectionName = "XYAlibabaCloudOCR";

        /// <summary>
        /// 阿里云 AccessKeyId。
        /// </summary>
        public string AccessKeyId { get; set; } = string.Empty;

        /// <summary>
        /// 阿里云 AccessKeySecret。
        /// </summary>
        public string AccessKeySecret { get; set; } = string.Empty;

        /// <summary>
        /// OCR API Endpoint。
        /// </summary>
        public string Endpoint { get; set; } = "ocr-api.cn-hangzhou.aliyuncs.com";
    }
}
