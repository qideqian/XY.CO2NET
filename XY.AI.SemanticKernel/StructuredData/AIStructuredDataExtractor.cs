using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using XY.AI.SemanticKernel.StructuredData;

namespace XY.AI.SemanticKernel
{
    /// <summary>
    /// 基于 Semantic Kernel 的结构化数据提取实现。
    /// </summary>
    public class AIStructuredDataExtractor : IAIStructuredDataExtractor
    {
        /// <summary>
        /// Semantic Kernel 实例。
        /// </summary>
        private readonly Kernel _kernel;

        /// <summary>
        /// 结构化提取配置。
        /// </summary>
        private readonly XYAIStructuredDataOptions _options;

        /// <summary>
        /// 初始化 <see cref="AIStructuredDataExtractor" /> 实例。
        /// </summary>
        /// <param name="kernel">已注册聊天服务的 Semantic Kernel 实例。</param>
        /// <param name="options">结构化提取配置选项。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="kernel" /> 或 <paramref name="options" /> 为 null 时抛出。</exception>
        public AIStructuredDataExtractor(Kernel kernel, IOptions<XYAIStructuredDataOptions> options)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// 使用自定义提示词从输入文本中提取结构化数据。
        /// </summary>
        /// <typeparam name="T">目标结构化类型，必须为引用类型且具有无参构造函数。</typeparam>
        /// <param name="text">待分析的原始文本。</param>
        /// <param name="prompt">用于指导模型提取结构化信息的提示词。</param>
        /// <param name="executionSettings">模型执行设置，例如温度、最大 Token 等。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>反序列化后的目标对象；当模型返回空内容或无法映射时可能为 null。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="text" /> 或 <paramref name="prompt" /> 为空时抛出。</exception>
        /// <exception cref="OperationCanceledException">当请求被取消时抛出。</exception>
        /// <exception cref="InvalidOperationException">当未注册聊天服务或模型返回内容无法反序列化时抛出。</exception>
        /// <remarks>
        /// 当模型返回 JSON 数组且目标类型为对象时，会自动尝试提取数组首个对象并反序列化。
        /// </remarks>
        public async Task<T?> ExtractDataAsync<T>(
            string text,
            string prompt,
            PromptExecutionSettings executionSettings = null,
            CancellationToken cancellationToken = default) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("文本内容不能为空。", nameof(text));
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException("提示词不能为空。", nameof(prompt));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var chatService = _kernel.Services.GetService(typeof(IChatCompletionService)) as IChatCompletionService;
            if (chatService == null)
            {
                throw new InvalidOperationException("未找到 IChatCompletionService。请先在 Kernel 中注册聊天模型服务。\n例如：builder.AddOpenAIChatCompletion(...) 或 builder.AddAzureOpenAIChatCompletion(...)");
            }

            var chatHistory = new ChatHistory();
            if (!string.IsNullOrWhiteSpace(_options.SystemPrompt))
            {
                chatHistory.AddSystemMessage(_options.SystemPrompt);
            }

            chatHistory.AddUserMessage($"{prompt}{Environment.NewLine}{Environment.NewLine}{text}");

            var message = await chatService.GetChatMessageContentAsync(chatHistory, executionSettings, _kernel, cancellationToken).ConfigureAwait(false);
            var cleanResponse = CleanAiResponse(message?.Content);
            var serializerOptions = CreateSerializerOptions();

            try
            {
                return JsonSerializer.Deserialize<T>(cleanResponse, serializerOptions);
            }
            catch (JsonException ex)
            {
                try
                {
                    using var jsonDocument = JsonDocument.Parse(cleanResponse);
                    if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array
                        && jsonDocument.RootElement.GetArrayLength() > 0)
                    {
                        var firstItem = jsonDocument.RootElement[0];
                        if (firstItem.ValueKind == JsonValueKind.Object)
                        {
                            return JsonSerializer.Deserialize<T>(firstItem.GetRawText(), serializerOptions);
                        }
                    }
                }
                catch (JsonException)
                {
                }

                throw new InvalidOperationException($"无法将 AI 响应反序列化为 {typeof(T).Name}。响应内容：{cleanResponse}", ex);
            }
        }

        private JsonSerializerOptions CreateSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = _options.PropertyNameCaseInsensitive
            };

            options.Converters.Add(new NullableDateTimeEmptyStringJsonConverter());
            return options;
        }

        /// <summary>
        /// 使用默认提示词从输入文本中提取结构化数据。
        /// </summary>
        /// <typeparam name="T">目标结构化类型，必须为引用类型且具有无参构造函数。</typeparam>
        /// <param name="text">待分析的原始文本。</param>
        /// <param name="executionSettings">模型执行设置，例如温度、最大 Token 等。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>反序列化后的目标对象；当模型返回空内容或无法映射时可能为 null。</returns>
        public Task<T?> ExtractDataAsync<T>(
            string text,
            PromptExecutionSettings executionSettings = null,
            CancellationToken cancellationToken = default) where T : class, new()
        {
            var defaultPrompt = GenerateDefaultPrompt<T>();
            return ExtractDataAsync<T>(text, defaultPrompt, executionSettings, cancellationToken);
        }

        /// <summary>
        /// 清理模型返回文本中的 Markdown 代码块标记，仅保留 JSON 内容。
        /// </summary>
        /// <param name="responseText">模型原始响应文本。</param>
        /// <returns>清理后的纯文本 JSON 字符串。</returns>
        /// <remarks>
        /// 支持移除 <c>```json</c> 与 <c>```</c> 包裹标记。
        /// </remarks>
        private static string CleanAiResponse(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return responseText;
            }

            responseText = responseText.Trim();

            if (responseText.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                responseText = responseText.Substring(7).Trim();
            }
            else if (responseText.StartsWith("```", StringComparison.OrdinalIgnoreCase))
            {
                responseText = responseText.Substring(3).Trim();
            }

            if (responseText.EndsWith("```", StringComparison.OrdinalIgnoreCase))
            {
                responseText = responseText.Substring(0, responseText.Length - 3).Trim();
            }

            return responseText;
        }

        /// <summary>
        /// 根据目标类型自动生成默认提取提示词。
        /// </summary>
        /// <typeparam name="T">目标结构化数据类型。</typeparam>
        /// <returns>包含类型属性说明的提示词。</returns>
        /// <remarks>
        /// 该提示词会要求模型仅返回 JSON，并严格匹配目标类型属性结构。
        /// </remarks>
        private static string GenerateDefaultPrompt<T>() where T : class, new()
        {
            var properties = typeof(T).GetProperties();
            var propertyDescriptions = string.Join(", ", properties.Select(p => $"{p.Name} ({p.PropertyType.Name})"));

            return $"请根据输入文本提取关键信息，并仅返回纯 JSON。JSON 结构必须严格匹配类型 {typeof(T).Name} 的属性：{propertyDescriptions}。如果无法提取，请返回空对象 {{}}。";
        }

        private sealed class NullableDateTimeEmptyStringJsonConverter : JsonConverter<DateTime?>
        {
            public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return null;
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    var value = reader.GetString();
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return null;
                    }

                    if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime))
                    {
                        return dateTime;
                    }
                }

                throw new JsonException("DateTime 字段格式无效，期望 ISO 8601 日期字符串或 null。");
            }

            public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
            {
                if (value.HasValue)
                {
                    writer.WriteStringValue(value.Value);
                }
                else
                {
                    writer.WriteNullValue();
                }
            }
        }
    }
}
