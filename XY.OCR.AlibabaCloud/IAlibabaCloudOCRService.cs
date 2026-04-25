using System.Threading;
using System.Threading.Tasks;

namespace XY.OCR.AlibabaCloud
{
    /// <summary>
    /// 阿里云 OCR 文本识别服务接口。
    /// </summary>
    public interface IAlibabaCloudOCRService
    {
        /// <summary>
        /// 调用阿里云 OCR 通用文字识别接口识别图片（二进制）并返回提取文本。
        /// </summary>
        /// <param name="imageBytes">图片二进制内容。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>提取出的文本内容。</returns>
        Task<string> RecognizeGeneralAsync(byte[] imageBytes, CancellationToken cancellationToken = default);

        /// <summary>
        /// 调用阿里云 OCR 通用文字识别接口识别图片（URL）并返回提取文本。
        /// </summary>
        /// <param name="imageUrl">公网可访问的图片 URL。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>提取出的文本内容。</returns>
        Task<string> RecognizeGeneralByUrlAsync(string imageUrl, CancellationToken cancellationToken = default);
    }
}
