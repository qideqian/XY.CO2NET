using System.Threading;
using System.Threading.Tasks;

namespace XY.OCR.UmiOCR
{
    /// <summary>
    /// Umi-OCR 服务接口。
    /// </summary>
    public interface IUmiOCRService
    {
        /// <summary>
        /// 对图片二进制内容执行 OCR 并返回合并后的文本。
        /// </summary>
        /// <param name="imageBytes">图片二进制内容。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>合并后的文本。</returns>
        Task<string> RecognizeImageAsync(byte[] imageBytes, CancellationToken cancellationToken = default);

        /// <summary>
        /// 对 PDF 二进制内容执行 OCR 并返回合并后的文本。
        /// </summary>
        /// <param name="pdfBytes">PDF 二进制内容。</param>
        /// <param name="fileName">文件名。</param>
        /// <param name="contentType">内容类型。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>合并后的文本。</returns>
        Task<string> RecognizePdfAsync(
            byte[] pdfBytes,
            string fileName = "document.pdf",
            string contentType = "application/pdf",
            CancellationToken cancellationToken = default);
    }
}
