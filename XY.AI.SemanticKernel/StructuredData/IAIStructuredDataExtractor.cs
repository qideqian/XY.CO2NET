using Microsoft.SemanticKernel;
using System.Threading;
using System.Threading.Tasks;

namespace XY.AI.SemanticKernel
{
    /// <summary>
    /// AI 结构化数据提取服务接口。
    /// </summary>
    public interface IAIStructuredDataExtractor
    {
        /// <summary>
        /// 使用自定义提示词提取结构化数据。
        /// </summary>
        /// <typeparam name="T">目标结构化数据类型。</typeparam>
        /// <param name="text">待分析的原始文本。</param>
        /// <param name="prompt">提取任务提示词。</param>
        /// <param name="executionSettings">模型执行参数，可用于控制温度、最大 Token 等。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>提取并反序列化后的对象；当模型返回空对象或无有效内容时可能为 null。</returns>
        Task<T?> ExtractDataAsync<T>(
            string text,
            string prompt,
            PromptExecutionSettings executionSettings = null,
            CancellationToken cancellationToken = default) where T : class, new();

        /// <summary>
        /// 使用默认提示词提取结构化数据。
        /// </summary>
        /// <typeparam name="T">目标结构化数据类型。</typeparam>
        /// <param name="text">待分析的原始文本。</param>
        /// <param name="executionSettings">模型执行参数，可用于控制温度、最大 Token 等。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>提取并反序列化后的对象；当模型返回空对象或无有效内容时可能为 null。</returns>
        Task<T?> ExtractDataAsync<T>(
            string text,
            PromptExecutionSettings executionSettings = null,
            CancellationToken cancellationToken = default) where T : class, new();
    }
}
