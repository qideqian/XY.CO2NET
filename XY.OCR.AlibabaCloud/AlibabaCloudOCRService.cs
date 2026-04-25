using AlibabaCloud.OpenApiClient.Models;
using AlibabaCloud.SDK.Ocr_api20210707;
using AlibabaCloud.SDK.Ocr_api20210707.Models;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace XY.OCR.AlibabaCloud
{
    /// <summary>
    /// 基于阿里云 OCR SDK 的文本识别服务实现。
    /// </summary>
    public class AlibabaCloudOCRService : IAlibabaCloudOCRService
    {
        private readonly XYAlibabaCloudOCROptions _options;
        private readonly Client _client;

        /// <summary>
        /// 初始化 <see cref="AlibabaCloudOCRService" /> 实例。
        /// </summary>
        /// <param name="options">阿里云 OCR 配置选项。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="options" /> 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当配置缺少必要参数时抛出。</exception>
        public AlibabaCloudOCRService(IOptions<XYAlibabaCloudOCROptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            ValidateOptions(_options);
            _client = CreateClient(_options);
        }

        /// <summary>
        /// 调用阿里云 OCR 通用文字识别接口识别图片（二进制）并返回提取文本。
        /// </summary>
        /// <param name="imageBytes">图片二进制内容。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>提取出的文本内容。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="imageBytes" /> 为空时抛出。</exception>
        public async Task<string> RecognizeGeneralAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                throw new ArgumentException("图片内容不能为空。", nameof(imageBytes));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var request = new RecognizeGeneralRequest
            {
                Body = new MemoryStream(imageBytes)
            };

            var response = await _client.RecognizeGeneralAsync(request).ConfigureAwait(false);
            return ExtractText(response?.Body?.Data);
        }

        /// <summary>
        /// 调用阿里云 OCR 通用文字识别接口识别图片（URL）并返回提取文本。
        /// </summary>
        /// <param name="imageUrl">公网可访问的图片 URL。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>提取出的文本内容。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="imageUrl" /> 为空时抛出。</exception>
        public async Task<string> RecognizeGeneralByUrlAsync(string imageUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                throw new ArgumentException("图片 URL 不能为空。", nameof(imageUrl));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var request = new RecognizeGeneralRequest
            {
                Url = imageUrl
            };

            var response = await _client.RecognizeGeneralAsync(request).ConfigureAwait(false);
            return ExtractText(response?.Body?.Data);
        }

        private static Client CreateClient(XYAlibabaCloudOCROptions options)
        {
            var config = new Config
            {
                AccessKeyId = options.AccessKeyId,
                AccessKeySecret = options.AccessKeySecret,
                Endpoint = options.Endpoint
            };

            return new Client(config);
        }

        private static void ValidateOptions(XYAlibabaCloudOCROptions options)
        {
            if (string.IsNullOrWhiteSpace(options.AccessKeyId))
            {
                throw new InvalidOperationException("AccessKeyId 未配置。请设置 XYAlibabaCloudOCR:AccessKeyId。 ");
            }

            if (string.IsNullOrWhiteSpace(options.AccessKeySecret))
            {
                throw new InvalidOperationException("AccessKeySecret 未配置。请设置 XYAlibabaCloudOCR:AccessKeySecret。 ");
            }

            if (string.IsNullOrWhiteSpace(options.Endpoint))
            {
                throw new InvalidOperationException("Endpoint 未配置。请设置 XYAlibabaCloudOCR:Endpoint。 ");
            }
        }

        private static string ExtractText(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            try
            {
                using var document = JsonDocument.Parse(data);
                var root = document.RootElement;

                if (root.TryGetProperty("content", out var contentElement))
                {
                    return contentElement.GetString() ?? string.Empty;
                }

                if (root.TryGetProperty("prism_wordsInfo", out var wordsInfo)
                    && wordsInfo.ValueKind == JsonValueKind.Array)
                {
                    var lines = string.Empty;
                    foreach (var item in wordsInfo.EnumerateArray())
                    {
                        if (item.TryGetProperty("word", out var wordElement))
                        {
                            lines += (wordElement.GetString() ?? string.Empty) + Environment.NewLine;
                        }
                    }

                    return lines.Trim();
                }
            }
            catch (JsonException)
            {
            }

            return data;
        }
    }
}
