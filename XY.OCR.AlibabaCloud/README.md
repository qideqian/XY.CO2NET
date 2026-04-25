# XY.OCR.AlibabaCloud

基于阿里云 OCR（通用文字识别）的文本提取类库，支持 DI 直接注入使用。

## 参考文档

- 通用文字识别产品概述：
  <https://help.aliyun.com/zh/ocr/product-overview/common-character-recognition-1>

## 配置项

默认配置节：`XYAlibabaCloudOCR`

```json
{
  "XYAlibabaCloudOCR": {
    "AccessKeyId": "你的AccessKeyId",
    "AccessKeySecret": "你的AccessKeySecret",
    "Endpoint": "ocr-api.cn-hangzhou.aliyuncs.com"
  }
}
```

## DI 注册方式

### 1) 从配置文件注册

```csharp
services.AddXYAlibabaCloudOCR(configuration);
// 或自定义配置节
services.AddXYAlibabaCloudOCR(configuration, "AliyunOcr");
```

### 2) 代码委托注册

```csharp
services.AddXYAlibabaCloudOCR(options =>
{
    options.AccessKeyId = "你的AccessKeyId";
    options.AccessKeySecret = "你的AccessKeySecret";
    options.Endpoint = "ocr-api.cn-hangzhou.aliyuncs.com";
});
```

### 3) 直接参数注册

```csharp
services.AddXYAlibabaCloudOCR(
    accessKeyId: "你的AccessKeyId",
    accessKeySecret: "你的AccessKeySecret",
    endpoint: "ocr-api.cn-hangzhou.aliyuncs.com");
```

## 使用示例

```csharp
public class OcrAppService
{
    private readonly IAlibabaCloudOCRService _ocrService;

    public OcrAppService(IAlibabaCloudOCRService ocrService)
    {
        _ocrService = ocrService;
    }

    public async Task<string> RecognizeByBytesAsync(byte[] imageBytes)
    {
        return await _ocrService.RecognizeGeneralAsync(imageBytes);
    }

    public async Task<string> RecognizeByUrlAsync(string imageUrl)
    {
        return await _ocrService.RecognizeGeneralByUrlAsync(imageUrl);
    }
}
```
