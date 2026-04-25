using Microsoft.Extensions.Options;
using System.Reflection;
using XY.OCR.AlibabaCloud;

namespace XY.OCR.AlibabaCloudTests;

[TestClass]
public class AlibabaCloudOCRServiceTests
{
    [TestMethod]
    public void Ctor_ShouldThrow_WhenAccessKeyIdMissing()
    {
        var options = new XYAlibabaCloudOCROptions
        {
            AccessKeyId = string.Empty,
            AccessKeySecret = "secret",
            Endpoint = "ocr-api.cn-hangzhou.aliyuncs.com"
        };

        try
        {
            _ = new AlibabaCloudOCRService(Options.Create(options));
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException)
        {
        }
    }

    [TestMethod]
    public void Ctor_ShouldThrow_WhenAccessKeySecretMissing()
    {
        var options = new XYAlibabaCloudOCROptions
        {
            AccessKeyId = "ak",
            AccessKeySecret = string.Empty,
            Endpoint = "ocr-api.cn-hangzhou.aliyuncs.com"
        };

        try
        {
            _ = new AlibabaCloudOCRService(Options.Create(options));
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException)
        {
        }
    }

    [TestMethod]
    public void Ctor_ShouldThrow_WhenEndpointMissing()
    {
        var options = new XYAlibabaCloudOCROptions
        {
            AccessKeyId = "ak",
            AccessKeySecret = "secret",
            Endpoint = string.Empty
        };

        try
        {
            _ = new AlibabaCloudOCRService(Options.Create(options));
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException)
        {
        }
    }

    [TestMethod]
    public async Task RecognizeGeneralAsync_ShouldThrow_WhenImageBytesEmpty()
    {
        var service = CreateService();

        try
        {
            await service.RecognizeGeneralAsync(Array.Empty<byte>());
            Assert.Fail("Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException)
        {
        }
    }

    [TestMethod]
    public async Task RecognizeGeneralByUrlAsync_ShouldThrow_WhenImageUrlEmpty()
    {
        var service = CreateService();

        try
        {
            await service.RecognizeGeneralByUrlAsync(string.Empty);
            Assert.Fail("Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException)
        {
        }
    }

    [TestMethod]
    public void ExtractText_ShouldReturnContent_WhenContentExists()
    {
        const string json = "{\"content\":\"hello world\"}";

        var result = InvokeExtractText(json);

        Assert.AreEqual("hello world", result);
    }

    [TestMethod]
    public void ExtractText_ShouldMergePrismWordsInfo_WhenContentMissing()
    {
        const string json = "{\"prism_wordsInfo\":[{\"word\":\"第一行\"},{\"word\":\"第二行\"}]}";

        var result = InvokeExtractText(json);

        Assert.AreEqual($"第一行{Environment.NewLine}第二行", result);
    }

    [TestMethod]
    public void ExtractText_ShouldReturnRawData_WhenJsonInvalid()
    {
        const string raw = "not-json";

        var result = InvokeExtractText(raw);

        Assert.AreEqual(raw, result);
    }

    private static AlibabaCloudOCRService CreateService()
    {
        var options = new XYAlibabaCloudOCROptions
        {
            AccessKeyId = "ak",
            AccessKeySecret = "secret",
            Endpoint = "ocr-api.cn-hangzhou.aliyuncs.com"
        };

        return new AlibabaCloudOCRService(Options.Create(options));
    }

    private static string InvokeExtractText(string data)
    {
        var method = typeof(AlibabaCloudOCRService).GetMethod(
            "ExtractText",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.IsNotNull(method);

        var result = method.Invoke(null, new object[] { data }) as string;
        return result ?? string.Empty;
    }
}
