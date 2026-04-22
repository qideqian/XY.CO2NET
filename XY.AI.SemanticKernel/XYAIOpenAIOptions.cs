namespace XY.AI.SemanticKernel
{
    /// <summary>
    /// OpenAI 聊天服务配置。
    /// </summary>
    public class XYAIOpenAIOptions
    {
        /// <summary>
        /// 默认配置节名称。
        /// </summary>
        public const string DefaultSectionName = "XYAIOpenAI";

        /// <summary>
        /// OpenAI 模型名称。
        /// </summary>
        /// <remarks>
        /// 例如：gpt-4o-mini。
        /// </remarks>
        public string ModelId { get; set; }

        /// <summary>
        /// OpenAI API Key。
        /// </summary>
        /// <remarks>
        /// 建议通过环境变量或安全配置中心注入，不建议硬编码。
        /// </remarks>
        public string ApiKey { get; set; }
    }
}
