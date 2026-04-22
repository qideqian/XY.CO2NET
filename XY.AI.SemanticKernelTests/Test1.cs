using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using XY.AI.SemanticKernel;

namespace XY.AI.SemanticKernelTests;

[TestClass]
public sealed class AIStructuredDataExtractorTests
{
    private sealed class DemoDto
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    private sealed class FakeChatService : IChatCompletionService
    {
        private readonly string _response;
        public ChatHistory? LastChatHistory { get; private set; }

        public FakeChatService(string response)
        {
            _response = response;
        }

        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            LastChatHistory = chatHistory;

            IReadOnlyList<ChatMessageContent> result = new List<ChatMessageContent>
            {
                new ChatMessageContent(AuthorRole.Assistant, _response)
            };

            return Task.FromResult(result);
        }

        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            return AsyncEnumerable.Empty<StreamingChatMessageContent>();
        }
    }

    [TestMethod]
    public async Task ExtractDataAsync_UsesDefaultPrompt_AndParsesJson()
    {
        var fakeChatService = new FakeChatService("{\"Name\":\"Tom\",\"Age\":30}");
        var kernel = BuildKernel(fakeChatService);
        var extractor = new AIStructuredDataExtractor(kernel, Options.Create(new XYAIStructuredDataOptions()));

        var result = await extractor.ExtractDataAsync<DemoDto>("用户：Tom，年龄：30");

        Assert.IsNotNull(result);
        Assert.AreEqual("Tom", result.Name);
        Assert.AreEqual(30, result.Age);

        Assert.IsNotNull(fakeChatService.LastChatHistory);
        Assert.IsGreaterThanOrEqualTo(fakeChatService.LastChatHistory.Count, 2);
        Assert.AreEqual(AuthorRole.System, fakeChatService.LastChatHistory[0].Role);
        Assert.AreEqual(AuthorRole.User, fakeChatService.LastChatHistory[1].Role);
        StringAssert.Contains(fakeChatService.LastChatHistory[1].Content, "DemoDto");
        StringAssert.Contains(fakeChatService.LastChatHistory[1].Content, "Name (String)");
        StringAssert.Contains(fakeChatService.LastChatHistory[1].Content, "Age (Int32)");
    }

    [TestMethod]
    public async Task ExtractDataAsync_CleansMarkdownJsonFence()
    {
        var fakeChatService = new FakeChatService("```json\n{\"Name\":\"Alice\",\"Age\":18}\n```");
        var kernel = BuildKernel(fakeChatService);
        var extractor = new AIStructuredDataExtractor(kernel, Options.Create(new XYAIStructuredDataOptions()));

        var result = await extractor.ExtractDataAsync<DemoDto>("文本", "请提取用户信息");

        Assert.IsNotNull(result);
        Assert.AreEqual("Alice", result.Name);
        Assert.AreEqual(18, result.Age);
    }

    [TestMethod]
    public async Task ExtractDataAsync_ThrowsWhenJsonInvalid()
    {
        var fakeChatService = new FakeChatService("not-a-json");
        var kernel = BuildKernel(fakeChatService);
        var extractor = new AIStructuredDataExtractor(kernel, Options.Create(new XYAIStructuredDataOptions()));

        InvalidOperationException? ex = null;
        try
        {
            await extractor.ExtractDataAsync<DemoDto>("文本", "请提取用户信息");
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException caught)
        {
            ex = caught;
        }

        Assert.IsNotNull(ex);
        StringAssert.Contains(ex.Message, "无法将 AI 响应反序列化为 DemoDto");
    }

    [TestMethod]
    public void AddXYAIStructuredDataExtractorWithOpenAI_RegistersServices_WhenValidParams()
    {
        var services = new ServiceCollection();

        services.AddXYAIStructuredDataExtractorWithOpenAI("gpt-4o-mini", "test-api-key");

        var provider = services.BuildServiceProvider();
        var kernel = provider.GetService<Kernel>();
        var extractor = provider.GetService<IAIStructuredDataExtractor>();

        Assert.IsNotNull(kernel);
        Assert.IsNotNull(extractor);
    }

    [TestMethod]
    public void AddXYAIStructuredDataExtractorWithOpenAI_Config_ThrowsWhenModelMissing()
    {
        var data = new Dictionary<string, string?>
        {
            ["XYAIOpenAI:ApiKey"] = "test-api-key"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();

        var services = new ServiceCollection();

        try
        {
            services.AddXYAIStructuredDataExtractorWithOpenAI(configuration);
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static Kernel BuildKernel(IChatCompletionService chatService)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(chatService);
        return new Kernel(serviceCollection.BuildServiceProvider());
    }
}
