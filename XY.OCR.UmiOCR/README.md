# XY.OCR.UmiOCR

基于 Umi-OCR HTTP API 的 OCR 文本提取类库，支持图片识别与 PDF 识别，开箱可用于 DI 注入。

## 发布Nuget 包
- 包名称：`XY.OCR.UmiOCR`
- 描述：基于 Umi-OCR HTTP API 的 OCR 文本提取类库，支持图片识别与 PDF 识别，开箱可用于 DI 注入。
- PowerShell（pwsh）发布命令模板

```
cd F:\GitHub\XY.CO2NET;
dotnet pack .\XY.OCR.UmiOCR\XY.OCR.UmiOCR.csproj -c Release -o .\artifacts\nuget;
dotnet nuget push .\artifacts\nuget\XY.OCR.UmiOCR.*.nupkg --source https://api.nuget.org/v3/index.json --api-key "<YOUR_NUGET_API_KEY>" --skip-duplicate
```

## 安装与引用

将项目或 NuGet 包 `XY.OCR.UmiOCR` 引用到你的应用中。

## Umi-OCR 下载与 HTTP 服务开启

### 1) 下载 Umi-OCR

- 官方仓库（Releases）：<https://github.com/hiroi-sora/Umi-OCR/releases>
- 下载后解压到本地任意目录并运行 `Umi-OCR.exe`。

### 2) 开启 HTTP API 服务

- 在 Umi-OCR 客户端中进入 **设置 / 接口 / HTTP API**（不同版本菜单名称可能略有差异）。
- 启用 HTTP 服务，并确认监听地址与端口（默认常见为 `127.0.0.1:1224`）。
- 启用后可在本库配置中使用：
  - `OcrUrl`: `http://127.0.0.1:1224/api/ocr`
  - `DocUrl`: `http://127.0.0.1:1224/api/doc`

## 配置项

默认配置节：`XYUmiOCR`

```json
{
  "XYUmiOCR": {
    "OcrUrl": "http://127.0.0.1:1224/api/ocr",
    "DocUrl": "http://127.0.0.1:1224/api/doc",
    "PollingIntervalMilliseconds": 1000,
    "MaxPollingAttempts": 300,
    "LineMergeThreshold": 10
  }
}
```

## DI 注册方式

### 1) 从配置文件注册

```csharp
services.AddXYUmiOCR(configuration);
// 或自定义配置节
services.AddXYUmiOCR(configuration, "OcrService");
```

### 2) 代码委托注册

```csharp
services.AddXYUmiOCR(options =>
{
    options.OcrUrl = "http://127.0.0.1:1224/api/ocr";
    options.DocUrl = "http://127.0.0.1:1224/api/doc";
    options.PollingIntervalMilliseconds = 1000;
    options.MaxPollingAttempts = 300;
    options.LineMergeThreshold = 10d;
});
```

### 3) 直接参数注册

```csharp
services.AddXYUmiOCR(
    ocrUrl: "http://127.0.0.1:1224/api/ocr",
    docUrl: "http://127.0.0.1:1224/api/doc",
    pollingIntervalMilliseconds: 1000,
    maxPollingAttempts: 300,
    lineMergeThreshold: 10d);
```

## 使用示例

```csharp
public class OcrAppService
{
    private readonly IUmiOCRService _umiOcrService;

    public OcrAppService(IUmiOCRService umiOcrService)
    {
        _umiOcrService = umiOcrService;
    }

    public async Task<string> RecognizeImageAsync(byte[] imageBytes)
    {
        return await _umiOcrService.RecognizeImageAsync(imageBytes);
    }

    public async Task<string> RecognizePdfAsync(byte[] pdfBytes)
    {
        return await _umiOcrService.RecognizePdfAsync(pdfBytes, "sample.pdf", "application/pdf");
    }
}
```

## 接口说明

- `RecognizeImageAsync`：发送图片到 Umi-OCR 图片接口，返回合并文本。
- `RecognizePdfAsync`：上传 PDF、轮询任务、下载结果并合并文本，最后执行任务清理。

## 注意事项

- 使用前请确认 Umi-OCR 服务可访问。
- `MaxPollingAttempts * PollingIntervalMilliseconds` 决定 PDF 任务最长等待时间。
- 若 Umi-OCR 返回格式与默认解析结构不一致，请按实际接口格式调整解析逻辑。
