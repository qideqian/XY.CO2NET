# XY.AI.SemanticKernel

基于 `Microsoft.SemanticKernel` 的结构化数据提取帮助类库。

## 功能

- 提供与参考类相似的 `ExtractDataAsync<T>` 接口（自定义提示词 / 默认提示词）
- 抽离 AI 连接参数（模型、Key、Endpoint）到底层 `Kernel` 注册过程
- 通过扩展方法直接注册到 .NET DI 容器

## 适用场景

- 需要从自然语言中提取结构化 DTO
- 希望统一通过配置切换 Azure OpenAI / 千问 / DeepSeek
- 希望在 ASP.NET Core 中通过 DI 直接注入使用

## 快速使用

```csharp
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using XY.AI.SemanticKernel;

var services = new ServiceCollection();

var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddOpenAIChatCompletion("gpt-4o-mini", "your-api-key");
services.AddSingleton(kernelBuilder.Build());

services.AddXYAIStructuredDataExtractor(options =>
{
    options.SystemPrompt = "你是一个严格输出 JSON 的助手";
});

var sp = services.BuildServiceProvider();
var extractor = sp.GetRequiredService<IAIStructuredDataExtractor>();

var dto = await extractor.ExtractDataAsync<MyDto>("文本内容");
```

> 说明：`ExtractDataAsync<T>` 会要求模型只输出 JSON，并自动清理 ```json 代码块标记后再反序列化。

## 配置文件说明（推荐）

你可以在 `appsettings.json` 中同时配置：

- `AIServices`：多模型服务配置
- `XYAIStructuredData`：结构化提取行为配置
- `XYAIOpenAI`：仅使用 OpenAI 一站式注册时的简化配置

完整示例：

```json
{
  "AIServices": {
    "DefaultUseServiceId": "AzureOpenAI",
    "AzureOpenAI": {
      "ModelId": "gpt-4o",
      "Endpoint": "https://weilan.openai.azure.com/",
      "ApiKey": "*****",
      "ServiceId": "AzureOpenAI"
    },
    "QWenPlus": {
      "ModelId": "qwen-plus",
      "Endpoint": "https://dashscope.aliyuncs.com/compatible-mode/v1",
      "ApiKey": "sk-*****",
      "ServiceId": "QWenPlusAI"
    },
    "DeepSeek": {
      "ModelId": "deepseek-chat",
      "Endpoint": "https://api.deepseek.com/v1",
      "ApiKey": "sk-*****",
      "ServiceId": "DeepSeekChatAI"
    }
  },
  "XYAIStructuredData": {
    "SystemPrompt": "你是一个结构化信息提取助手，只返回 JSON",
    "PropertyNameCaseInsensitive": true
  },
  "XYAIOpenAI": {
    "ModelId": "gpt-4o-mini",
    "ApiKey": "your-api-key"
  }
}
```

字段说明：

- `DefaultUseServiceId`：多服务同时注册时的默认服务标识
- `ModelId`：模型名称
- `Endpoint`：服务地址（Azure/QWen/DeepSeek 各自 endpoint）
- `ApiKey`：对应服务密钥
- `ServiceId`：Semantic Kernel 内部服务名，用于多服务区分
- `SystemPrompt`：提取器的系统提示词
- `PropertyNameCaseInsensitive`：JSON 反序列化忽略大小写

## 一站式 OpenAI 注册

```csharp
using Microsoft.Extensions.DependencyInjection;
using XY.AI.SemanticKernel;

var services = new ServiceCollection();

services.AddXYAIStructuredDataExtractorWithOpenAI(
    modelId: "gpt-4o-mini",
    apiKey: "your-api-key",
    configure: options =>
    {
        options.SystemPrompt = "你是一个严格输出 JSON 的助手";
    });
```

也支持从配置读取：

```csharp
services.AddXYAIStructuredDataExtractorWithOpenAI(configuration);
```

适用：只接入 OpenAI 或 Azure OpenAI 兼容服务，不需要多服务切换。

## 按单个服务注册（可选）

当你只希望启用一个服务时，可使用单服务重载：

```csharp
services.AddXYAIKernelWithAIServices(
    configuration,
    XYAIServiceType.QWenPlus);
```

或一站式注册（内核 + 结构化提取）：

```csharp
services.AddXYAIStructuredDataExtractorWithAIServices(
    configuration,
    XYAIServiceType.DeepSeek);
```

适用：配置里有多个服务，但当前应用只想启用其中一个。

## 多服务同时注册

```csharp
// 注册 AzureOpenAI + QWen + DeepSeek
builder.Services.AddXYAIKernelWithAIServices(builder.Configuration);

// 再注册结构化提取器
builder.Services.AddXYAIStructuredDataExtractor(builder.Configuration);
```

或一行完成：

```csharp
builder.Services.AddXYAIStructuredDataExtractorWithAIServices(builder.Configuration);
```

## 接口注册方法总览

- `AddXYAIStructuredDataExtractor(Action<XYAIStructuredDataOptions>?)`
- `AddXYAIStructuredDataExtractor(IConfiguration, string sectionName = "XYAIStructuredData")`
- `AddXYAIStructuredDataExtractorWithOpenAI(string modelId, string apiKey, Action<XYAIStructuredDataOptions>?)`
- `AddXYAIStructuredDataExtractorWithOpenAI(IConfiguration, string openAISectionName = "XYAIOpenAI", string extractorSectionName = "XYAIStructuredData")`
- `AddXYAIKernelWithAIServices(IConfiguration, string aiServicesSectionName = "AIServices")`
- `AddXYAIKernelWithAIServices(IConfiguration, XYAIServiceType, string aiServicesSectionName = "AIServices")`
- `AddXYAIStructuredDataExtractorWithAIServices(IConfiguration, string aiServicesSectionName = "AIServices", string extractorSectionName = "XYAIStructuredData")`
- `AddXYAIStructuredDataExtractorWithAIServices(IConfiguration, XYAIServiceType, string aiServicesSectionName = "AIServices", string extractorSectionName = "XYAIStructuredData")`

## 使用示例（业务代码）

```csharp
public sealed class ResumeAppService
{
    private readonly IAIStructuredDataExtractor _extractor;

    public ResumeAppService(IAIStructuredDataExtractor extractor)
    {
        _extractor = extractor;
    }

    public Task<CandidateDto?> ParseAsync(string resumeText, CancellationToken ct)
    {
        return _extractor.ExtractDataAsync<CandidateDto>(resumeText, cancellationToken: ct);
    }
}
```

自定义 prompt：

```csharp
var result = await extractor.ExtractDataAsync<OrderDto>(
    text: input,
    prompt: "请提取订单号、金额、下单时间，严格返回 JSON",
    cancellationToken: ct);
```

## Program.cs 推荐注册模板

```csharp
// 场景 A：多服务同时注册 + 提取器
builder.Services.AddXYAIStructuredDataExtractorWithAIServices(builder.Configuration);

// 场景 B：只启用某一个服务 + 提取器
// builder.Services.AddXYAIStructuredDataExtractorWithAIServices(builder.Configuration, XYAIServiceType.QWenPlus);

// 场景 C：只用 OpenAI 简化注册
// builder.Services.AddXYAIStructuredDataExtractorWithOpenAI(builder.Configuration);
```

## 安全建议

- 不要把 `ApiKey` 提交到仓库
- 建议使用环境变量、用户机密、Key Vault 等方式注入密钥

## 包版本管理（CPM）

本仓库已对 AI 相关项目启用 `Directory.Packages.props` 中央包版本管理。

- 版本集中在仓库根目录：`Directory.Packages.props`
- 项目文件中的 `PackageReference` 不再写 `Version`

如需升级 SK / Extensions，只需修改 `Directory.Packages.props`。

## 发布前一键检查

在 `XY.AI.SemanticKernel` 目录提供了两个脚本：

- PowerShell：`build.ps1`
- CMD：`build.cmd`

执行内容：`restore -> build -> test -> pack`

示例：

```powershell
pwsh ./build.ps1 -Configuration Release
```

产物输出目录：`artifacts/packages`
