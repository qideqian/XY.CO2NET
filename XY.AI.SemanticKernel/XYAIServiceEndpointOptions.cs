namespace XY.AI.SemanticKernel
{
    /// <summary>
    /// AI 服务终结点配置项。
    /// </summary>
    public class XYAIServiceEndpointOptions
    {
        /// <summary>
        /// 模型名称。
        /// </summary>
        public string ModelId { get; set; } = string.Empty;

        /// <summary>
        /// 服务地址。
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// API Key。
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Semantic Kernel 服务标识。
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;
    }
}
