using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace XY.OCR.UmiOCR
{
    /// <summary>
    /// 基于 Umi-OCR HTTP API 的 OCR 服务实现。
    /// </summary>
    public class UmiOCRService : IUmiOCRService
    {
        private readonly HttpClient _httpClient;
        private readonly XYUmiOCROptions _options;

        /// <summary>
        /// 初始化 <see cref="UmiOCRService" /> 实例。
        /// </summary>
        /// <param name="httpClient">HTTP 客户端。</param>
        /// <param name="options">OCR 配置选项。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="httpClient" /> 或 <paramref name="options" /> 为 null 时抛出。</exception>
        public UmiOCRService(HttpClient httpClient, IOptions<XYUmiOCROptions> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// 对图片内容执行 OCR 并返回合并后的文本结果。
        /// </summary>
        /// <param name="imageBytes">图片二进制内容。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>识别并合并后的文本。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="imageBytes" /> 为空时抛出。</exception>
        /// <exception cref="InvalidOperationException">当 OCR 接口地址未配置时抛出。</exception>
        /// <exception cref="HttpRequestException">当调用 Umi-OCR 接口失败时抛出。</exception>
        public async Task<string> RecognizeImageAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                throw new ArgumentException("图片内容不能为空。", nameof(imageBytes));
            }

            if (string.IsNullOrWhiteSpace(_options.OcrUrl))
            {
                throw new InvalidOperationException("Umi-OCR 图片接口地址未配置。请设置 XYUmiOCR:OcrUrl。 ");
            }

            var base64 = Convert.ToBase64String(imageBytes);
            var requestData = new
            {
                base64,
                options = new
                {
                    data_format = "json"
                }
            };

            var json = JsonSerializer.Serialize(requestData);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(_options.OcrUrl, content, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

            if (!result.TryGetProperty("data", out var ocrData))
            {
                return string.Empty;
            }

            if (ocrData.ValueKind == JsonValueKind.Array)
            {
                return MergeOcrTextFromBlocks(ocrData);
            }

            if (ocrData.ValueKind == JsonValueKind.Object)
            {
                return MergeOcrTextForSinglePage(ocrData, _options.LineMergeThreshold);
            }

            return string.Empty;
        }

        /// <summary>
        /// 对 PDF 内容执行 OCR 并返回合并后的文本结果。
        /// </summary>
        /// <param name="pdfBytes">PDF 二进制内容。</param>
        /// <param name="fileName">文件名，默认 <c>document.pdf</c>。</param>
        /// <param name="contentType">文件 Content-Type，默认 <c>application/pdf</c>。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>识别并合并后的文本。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="pdfBytes" /> 为空时抛出。</exception>
        /// <exception cref="InvalidOperationException">当文档接口配置缺失或返回数据无效时抛出。</exception>
        /// <exception cref="TimeoutException">当轮询超过最大次数仍未成功时抛出。</exception>
        /// <exception cref="HttpRequestException">当调用 Umi-OCR 接口失败时抛出。</exception>
        public async Task<string> RecognizePdfAsync(
            byte[] pdfBytes,
            string fileName = "document.pdf",
            string contentType = "application/pdf",
            CancellationToken cancellationToken = default)
        {
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                throw new ArgumentException("PDF 内容不能为空。", nameof(pdfBytes));
            }

            if (string.IsNullOrWhiteSpace(_options.DocUrl))
            {
                throw new InvalidOperationException("Umi-OCR 文档接口地址未配置。请设置 XYUmiOCR:DocUrl。 ");
            }

            string taskId = await UploadPdfAndGetTaskIdAsync(pdfBytes, fileName, contentType, cancellationToken).ConfigureAwait(false);

            try
            {
                await WaitUntilPdfTaskSuccessAsync(taskId, cancellationToken).ConfigureAwait(false);

                var downloadUrl = await GetDownloadUrlAsync(taskId, cancellationToken).ConfigureAwait(false);
                using var downloadResponse = await _httpClient.GetAsync(downloadUrl, cancellationToken).ConfigureAwait(false);
                downloadResponse.EnsureSuccessStatusCode();

                var resultContent = await downloadResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                var ocrResult = JsonSerializer.Deserialize<JsonElement>(resultContent);
                return MergeOcrText(ocrResult, _options.LineMergeThreshold);
            }
            finally
            {
                _ = _httpClient.DeleteAsync($"{_options.DocUrl.TrimEnd('/')}/task/{taskId}", cancellationToken);
            }
        }

        private async Task<string> UploadPdfAndGetTaskIdAsync(
            byte[] pdfBytes,
            string fileName,
            string contentType,
            CancellationToken cancellationToken)
        {
            using var formData = new MultipartFormDataContent();
            using var fileStream = new MemoryStream(pdfBytes);
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            formData.Add(fileContent, "file", fileName);

            using var uploadResponse = await _httpClient
                .PostAsync($"{_options.DocUrl.TrimEnd('/')}/upload", formData, cancellationToken)
                .ConfigureAwait(false);
            uploadResponse.EnsureSuccessStatusCode();

            var uploadResult = JsonSerializer.Deserialize<JsonElement>(await uploadResponse.Content.ReadAsStringAsync().ConfigureAwait(false));

            if (!uploadResult.TryGetProperty("data", out var data)
                || !data.TryGetProperty("task_id", out var taskIdElement)
                || string.IsNullOrWhiteSpace(taskIdElement.GetString()))
            {
                throw new InvalidOperationException("Umi-OCR 返回的 task_id 无效。 ");
            }

            return taskIdElement.GetString()!;
        }

        private async Task WaitUntilPdfTaskSuccessAsync(string taskId, CancellationToken cancellationToken)
        {
            var attempt = 0;
            while (attempt < _options.MaxPollingAttempts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                attempt++;

                await Task.Delay(_options.PollingIntervalMilliseconds, cancellationToken).ConfigureAwait(false);
                using var statusResponse = await _httpClient
                    .GetAsync($"{_options.DocUrl.TrimEnd('/')}/result/{taskId}", cancellationToken)
                    .ConfigureAwait(false);

                if (!statusResponse.IsSuccessStatusCode)
                {
                    continue;
                }

                var statusJson = await statusResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                var statusResult = JsonSerializer.Deserialize<JsonElement>(statusJson);

                if (!statusResult.TryGetProperty("data", out var data)
                    || !data.TryGetProperty("status", out var statusElement))
                {
                    continue;
                }

                var status = statusElement.GetString() ?? string.Empty;
                if (string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            throw new TimeoutException($"Umi-OCR 文档识别超时，taskId: {taskId}");
        }

        private async Task<string> GetDownloadUrlAsync(string taskId, CancellationToken cancellationToken)
        {
            using var downloadLinkResponse = await _httpClient
                .GetAsync($"{_options.DocUrl.TrimEnd('/')}/download/{taskId}", cancellationToken)
                .ConfigureAwait(false);
            downloadLinkResponse.EnsureSuccessStatusCode();

            var downloadResult = JsonSerializer.Deserialize<JsonElement>(await downloadLinkResponse.Content.ReadAsStringAsync().ConfigureAwait(false));

            if (!downloadResult.TryGetProperty("data", out var data)
                || !data.TryGetProperty("download_url", out var downloadUrlElement)
                || string.IsNullOrWhiteSpace(downloadUrlElement.GetString()))
            {
                throw new InvalidOperationException("Umi-OCR 返回的 download_url 无效。 ");
            }

            return downloadUrlElement.GetString()!;
        }

        private static string MergeOcrText(JsonElement ocrResult, double lineMergeThreshold)
        {
            if (!ocrResult.TryGetProperty("pages", out var pages) || pages.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            var allBlocks = new List<(double y, string text)>();

            foreach (var page in pages.EnumerateArray())
            {
                if (!page.TryGetProperty("blocks", out var blocks) || blocks.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var block in blocks.EnumerateArray())
                {
                    if (!TryGetYFromBbox(block, out var y))
                    {
                        continue;
                    }

                    var text = block.TryGetProperty("text", out var textElement)
                        ? textElement.GetString() ?? string.Empty
                        : string.Empty;
                    allBlocks.Add((y, text));
                }
            }

            return MergeByY(allBlocks, lineMergeThreshold);
        }

        private static string MergeOcrTextForSinglePage(JsonElement page, double lineMergeThreshold)
        {
            var allBlocks = new List<(double y, string text)>();

            if (page.TryGetProperty("blocks", out var blocks) && blocks.ValueKind == JsonValueKind.Array)
            {
                foreach (var block in blocks.EnumerateArray())
                {
                    if (!TryGetYFromBbox(block, out var y))
                    {
                        continue;
                    }

                    var text = block.TryGetProperty("text", out var textElement)
                        ? textElement.GetString() ?? string.Empty
                        : string.Empty;
                    allBlocks.Add((y, text));
                }
            }

            return MergeByY(allBlocks, lineMergeThreshold);
        }

        private static string MergeOcrTextFromBlocks(JsonElement blocksArray)
        {
            var orderedBlocks = blocksArray.EnumerateArray()
                .Select(block =>
                {
                    var y = GetYFromBox(block);
                    var text = block.TryGetProperty("text", out var textElement) ? textElement.GetString() ?? string.Empty : string.Empty;
                    var end = block.TryGetProperty("end", out var endElement) ? endElement.GetString() ?? string.Empty : string.Empty;
                    return (y, text + end);
                })
                .OrderBy(b => b.y)
                .Select(b => b.Item2);

            return string.Join(string.Empty, orderedBlocks);
        }

        private static string MergeByY(List<(double y, string text)> allBlocks, double lineMergeThreshold)
        {
            if (allBlocks.Count == 0)
            {
                return string.Empty;
            }

            allBlocks.Sort((a, b) => a.y.CompareTo(b.y));
            var lines = new List<string>();
            var currentY = double.NaN;
            var currentLine = new StringBuilder();

            foreach (var (y, text) in allBlocks)
            {
                if (double.IsNaN(currentY) || Math.Abs(y - currentY) > lineMergeThreshold)
                {
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine.ToString().Trim());
                    }

                    currentLine.Clear();
                    currentY = y;
                }

                currentLine.Append(text);
                currentLine.Append(' ');
            }

            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString().Trim());
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static bool TryGetYFromBbox(JsonElement block, out double y)
        {
            y = 0d;
            if (!block.TryGetProperty("bbox", out var bbox)
                || bbox.ValueKind != JsonValueKind.Array
                || bbox.GetArrayLength() < 2)
            {
                return false;
            }

            var yElement = bbox[1];
            if (yElement.ValueKind != JsonValueKind.Number)
            {
                return false;
            }

            return yElement.TryGetDouble(out y);
        }

        private static double GetYFromBox(JsonElement block)
        {
            if (!block.TryGetProperty("box", out var box)
                || box.ValueKind != JsonValueKind.Array
                || box.GetArrayLength() == 0)
            {
                return 0d;
            }

            var firstPoint = box[0];
            if (firstPoint.ValueKind != JsonValueKind.Array || firstPoint.GetArrayLength() < 2)
            {
                return 0d;
            }

            var yElement = firstPoint[1];
            if (yElement.ValueKind != JsonValueKind.Number || !yElement.TryGetDouble(out var y))
            {
                return 0d;
            }

            return y;
        }
    }
}
