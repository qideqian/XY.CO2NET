namespace XY.AI.SemanticKernel
{
    /// <summary>
    /// AI 结构化数据提取选项。
    /// </summary>
    public class XYAIStructuredDataOptions
    {
        /// <summary>
        /// 默认配置节名称。
        /// </summary>
        public const string DefaultSectionName = "XYAIStructuredData";

        /// <summary>
        /// 系统提示词。
        /// </summary>
        /// <remarks>
        /// 会作为 System Message 注入到对话上下文，建议保持稳定且约束明确。
        /// </remarks>
        public string SystemPrompt { get; set; } = "你是一个结构化信息提取助手，只能返回纯 JSON，不得返回任何额外说明。";

        /// <summary>
        /// 反序列化时是否忽略属性大小写。
        /// </summary>
        /// <remarks>
        /// 建议保持为 true，以提升模型输出字段大小写不一致时的兼容性。
        /// </remarks>
        public bool PropertyNameCaseInsensitive { get; set; } = true;
    }
}
