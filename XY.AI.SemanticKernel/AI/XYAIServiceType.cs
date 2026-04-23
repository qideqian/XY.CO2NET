namespace XY.AI.SemanticKernel;

/// <summary>
/// 可选的 AI 服务类型。
/// </summary>
public enum XYAIServiceType
{
    /// <summary>
    /// Azure OpenAI。
    /// </summary>
    AzureOpenAI = 0,

    /// <summary>
    /// 千问（QWen Plus，OpenAI Compatible）。
    /// </summary>
    QWenPlus = 1,

    /// <summary>
    /// DeepSeek（OpenAI Compatible）。
    /// </summary>
    DeepSeek = 2
}
