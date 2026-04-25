namespace XY.OCR.UmiOCR
{
    /// <summary>
    /// Umi-OCR 配置选项。
    /// </summary>
    public class XYUmiOCROptions
    {
        /// <summary>
        /// 默认配置节名称。
        /// </summary>
        public const string DefaultSectionName = "XYUmiOCR";

        /// <summary>
        /// 图片 OCR 接口地址。
        /// </summary>
        public string OcrUrl { get; set; } = "http://127.0.0.1:1224/api/ocr";

        /// <summary>
        /// 文档 OCR 接口基础地址。
        /// </summary>
        public string DocUrl { get; set; } = "http://127.0.0.1:1224/api/doc";

        /// <summary>
        /// PDF 任务轮询间隔（毫秒）。
        /// </summary>
        public int PollingIntervalMilliseconds { get; set; } = 1000;

        /// <summary>
        /// PDF 任务最大轮询次数。
        /// </summary>
        public int MaxPollingAttempts { get; set; } = 300;

        /// <summary>
        /// 文本合并时按 Y 轴换行的阈值。
        /// </summary>
        public double LineMergeThreshold { get; set; } = 10d;
    }
}
