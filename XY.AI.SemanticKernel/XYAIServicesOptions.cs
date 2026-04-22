namespace XY.AI.SemanticKernel
{
    /// <summary>
    /// 多 AI 服务配置。
    /// </summary>
    public class XYAIServicesOptions
    {
        /// <summary>
        /// 默认配置节名称。
        /// </summary>
        public const string DefaultSectionName = "AIServices";

        /// <summary>
        /// 默认使用的服务标识。
        /// </summary>
        public string DefaultUseServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Azure OpenAI 配置。
        /// </summary>
        public XYAIServiceEndpointOptions AzureOpenAI { get; set; } = new XYAIServiceEndpointOptions();

        /// <summary>
        /// 千问（OpenAI Compatible）配置。
        /// </summary>
        public XYAIServiceEndpointOptions QWenPlus { get; set; } = new XYAIServiceEndpointOptions();

        /// <summary>
        /// DeepSeek（OpenAI Compatible）配置。
        /// </summary>
        public XYAIServiceEndpointOptions DeepSeek { get; set; } = new XYAIServiceEndpointOptions();
    }
}
