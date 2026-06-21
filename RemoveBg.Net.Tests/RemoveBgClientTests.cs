using System.Net;
using System.Net.Http.Headers;
using RemoveBg.Net.Tests.Helpers;

namespace RemoveBg.Net.Tests;

public class RemoveBgClientTests
{
    private const string ApiKey = "test-api-key";
    private static readonly byte[] FakeImage = [0x89, 0x50, 0x4E, 0x47];

    #region Constructor

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidApiKey_Throws(string? apiKey)
    {
        Assert.Throws<ArgumentException>(() => new RemoveBgClient(apiKey!));
    }

    [Fact]
    public void Constructor_NullHttpClient_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new RemoveBgClient(ApiKey, null!));
    }

    #endregion

    #region RemoveFromUrlAsync

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RemoveFromUrlAsync_InvalidUrl_Throws(string? url)
    {
        using var client = MakeClient(_ => OkImage(FakeImage));
        await Assert.ThrowsAsync<ArgumentException>(() => client.RemoveFromUrlAsync(url!));
    }

    [Fact]
    public async Task RemoveFromUrlAsync_PostsToRemoveBgEndpoint()
    {
        var handler = new FakeHttpHandler(_ => OkImage(FakeImage));
        using var client = MakeClient(handler);

        await client.RemoveFromUrlAsync("https://example.com/photo.jpg");

        var req = handler.Requests.Single();
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("removebg", req.RequestUri!.ToString());
    }

    [Fact]
    public async Task RemoveFromUrlAsync_SendsApiKeyAndAcceptHeaders()
    {
        var handler = new FakeHttpHandler(_ => OkImage(FakeImage));
        using var client = MakeClient(handler);

        await client.RemoveFromUrlAsync("https://example.com/photo.jpg");

        var req = handler.Requests.Single();
        Assert.Equal(ApiKey, req.Headers.GetValues("X-Api-Key").Single());
        Assert.Contains("image/*", req.Headers.Accept.ToString());
    }

    [Fact]
    public async Task RemoveFromUrlAsync_IncludesImageUrlInFormData()
    {
        const string imageUrl = "https://example.com/photo.jpg";
        string? sentUrl = null;
        using var client = MakeClient(async req =>
        {
            sentUrl = await ReadFieldAsync(req, "image_url");
            return OkImage(FakeImage);
        });

        await client.RemoveFromUrlAsync(imageUrl);

        Assert.Equal(imageUrl, sentUrl);
    }

    [Fact]
    public async Task RemoveFromUrlAsync_WithOptions_IncludesOptionFields()
    {
        var captured = new Dictionary<string, string>();
        using var client = MakeClient(async req =>
        {
            await ReadAllFieldsAsync(req, captured);
            return OkImage(FakeImage);
        });

        await client.RemoveFromUrlAsync(
            "https://example.com/photo.jpg",
            new RemoveBgOptions { Size = ImageSize.Hd, Crop = true, BackgroundColor = "ff0000" });

        Assert.Equal("hd", captured["size"]);
        Assert.Equal("true", captured["crop"]);
        Assert.Equal("ff0000", captured["bg_color"]);
    }

    [Fact]
    public async Task RemoveFromUrlAsync_ReturnsImageBytes()
    {
        using var client = MakeClient(_ => OkImage(FakeImage));
        var result = await client.RemoveFromUrlAsync("https://example.com/photo.jpg");
        Assert.Equal(FakeImage, result.Content);
    }

    [Fact]
    public async Task RemoveFromUrlAsync_MapsAllResponseHeaders()
    {
        using var client = MakeClient(_ => OkImage(FakeImage, new()
        {
            ["X-Credits-Charged"] = "1.5",
            ["X-Type"] = "person",
            ["X-Width"] = "800",
            ["X-Height"] = "600",
            ["X-RateLimit-Limit"] = "500",
            ["X-RateLimit-Remaining"] = "499",
            ["X-RateLimit-Reset"] = "1700000000"
        }));

        var result = await client.RemoveFromUrlAsync("https://example.com/photo.jpg");

        Assert.Equal(1.5, result.CreditsCharged);
        Assert.Equal("person", result.DetectedType);
        Assert.Equal(800, result.Width);
        Assert.Equal(600, result.Height);
        Assert.Equal(500, result.RateLimit);
        Assert.Equal(499, result.RateLimitRemaining);
        Assert.Equal(1700000000L, result.RateLimitReset);
    }

    [Fact]
    public async Task RemoveFromUrlAsync_ApiError_ThrowsRemoveBgException()
    {
        using var client = MakeClient(_ => ErrorJson(HttpStatusCode.UnprocessableEntity,
            """{"errors":[{"title":"Invalid image","detail":"Could not process","code":"invalid_image"}]}"""));

        var ex = await Assert.ThrowsAsync<RemoveBgException>(() =>
            client.RemoveFromUrlAsync("https://example.com/photo.jpg"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, ex.StatusCode);
        Assert.Single(ex.Errors);
        Assert.Equal("Invalid image", ex.Errors[0].Title);
        Assert.Equal("Could not process", ex.Errors[0].Detail);
        Assert.Equal("invalid_image", ex.Errors[0].Code);
    }

    [Fact]
    public async Task RemoveFromUrlAsync_ApiError_NonJsonBody_WrapsBodyAsDetail()
    {
        using var client = MakeClient(_ => ErrorJson(HttpStatusCode.InternalServerError, "Internal server error"));

        var ex = await Assert.ThrowsAsync<RemoveBgException>(() =>
            client.RemoveFromUrlAsync("https://example.com/photo.jpg"));

        Assert.Single(ex.Errors);
        Assert.Equal("Internal server error", ex.Errors[0].Detail);
    }

    [Fact]
    public async Task RemoveFromUrlAsync_RateLimited_ExceptionCarriesRetryAfter()
    {
        var response = ErrorJson(HttpStatusCode.TooManyRequests,
            """{"errors":[{"title":"Rate Limit Exceeded","detail":"Too many requests","code":"rate_limit"}]}""");
        response.Headers.TryAddWithoutValidation("Retry-After", "60");
        using var client = MakeClient(_ => response);

        var ex = await Assert.ThrowsAsync<RemoveBgException>(() =>
            client.RemoveFromUrlAsync("https://example.com/photo.jpg"));

        Assert.True(ex.IsRateLimited);
        Assert.Equal(60, ex.RetryAfter);
    }

    #endregion

    #region RemoveFromBytesAsync

    [Fact]
    public async Task RemoveFromBytesAsync_NullBytes_Throws()
    {
        using var client = MakeClient(_ => OkImage(FakeImage));
        await Assert.ThrowsAsync<ArgumentException>(() => client.RemoveFromBytesAsync(null!));
    }

    [Fact]
    public async Task RemoveFromBytesAsync_EmptyBytes_Throws()
    {
        using var client = MakeClient(_ => OkImage(FakeImage));
        await Assert.ThrowsAsync<ArgumentException>(() => client.RemoveFromBytesAsync([]));
    }

    [Fact]
    public async Task RemoveFromBytesAsync_SendsBytesWithFileName()
    {
        byte[]? receivedBytes = null;
        string? receivedFileName = null;
        using var client = MakeClient(async req =>
        {
            var multipart = (MultipartFormDataContent)req.Content!;
            foreach (var part in multipart)
            {
                if (part.Headers.ContentDisposition?.Name?.Trim('"') == "image_file")
                {
                    receivedBytes = await part.ReadAsByteArrayAsync();
                    receivedFileName = part.Headers.ContentDisposition?.FileName?.Trim('"');
                }
            }
            return OkImage(FakeImage);
        });

        await client.RemoveFromBytesAsync(FakeImage, "photo.png");

        Assert.Equal(FakeImage, receivedBytes);
        Assert.Equal("photo.png", receivedFileName);
    }

    [Fact]
    public async Task RemoveFromBytesAsync_UsesDefaultFileName()
    {
        string? receivedFileName = null;
        using var client = MakeClient(async req =>
        {
            var multipart = (MultipartFormDataContent)req.Content!;
            foreach (var part in multipart)
                if (part.Headers.ContentDisposition?.Name?.Trim('"') == "image_file")
                    receivedFileName = part.Headers.ContentDisposition?.FileName?.Trim('"');
            return OkImage(FakeImage);
        });

        await client.RemoveFromBytesAsync(FakeImage);

        Assert.Equal("image.png", receivedFileName);
    }

    #endregion

    #region RemoveFromFileAsync

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RemoveFromFileAsync_InvalidPath_Throws(string? path)
    {
        using var client = MakeClient(_ => OkImage(FakeImage));
        await Assert.ThrowsAsync<ArgumentException>(() => client.RemoveFromFileAsync(path!));
    }

    [Fact]
    public async Task RemoveFromFileAsync_SendsFileContentWithCorrectFileName()
    {
        var tmpFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tmpFile, FakeImage);
            byte[]? receivedBytes = null;
            string? receivedFileName = null;
            using var client = MakeClient(async req =>
            {
                var multipart = (MultipartFormDataContent)req.Content!;
                foreach (var part in multipart)
                {
                    if (part.Headers.ContentDisposition?.Name?.Trim('"') == "image_file")
                    {
                        receivedBytes = await part.ReadAsByteArrayAsync();
                        receivedFileName = part.Headers.ContentDisposition?.FileName?.Trim('"');
                    }
                }
                return OkImage(FakeImage);
            });

            await client.RemoveFromFileAsync(tmpFile);

            Assert.Equal(FakeImage, receivedBytes);
            Assert.Equal(Path.GetFileName(tmpFile), receivedFileName);
        }
        finally
        {
            File.Delete(tmpFile);
        }
    }

    #endregion

    #region GetAccountInfoAsync

    [Fact]
    public async Task GetAccountInfoAsync_SendsGetToAccountEndpoint()
    {
        var handler = new FakeHttpHandler(_ => OkJson(AccountJson()));
        using var client = MakeClient(handler);

        await client.GetAccountInfoAsync();

        var req = handler.Requests.Single();
        Assert.Equal(HttpMethod.Get, req.Method);
        Assert.EndsWith("account", req.RequestUri!.ToString());
        Assert.Equal(ApiKey, req.Headers.GetValues("X-Api-Key").Single());
    }

    [Fact]
    public async Task GetAccountInfoAsync_ParsesAllFields()
    {
        using var client = MakeClient(_ => OkJson(AccountJson(
            total: 200, subscription: 150, payg: 50, enterprise: 5,
            freeCalls: 40, sizes: "all")));

        var info = await client.GetAccountInfoAsync();

        Assert.Equal(200, info.TotalCredits);
        Assert.Equal(150, info.SubscriptionCredits);
        Assert.Equal(50, info.PayAsYouGoCredits);
        Assert.Equal(5, info.EnterpriseCredits);
        Assert.Equal(40, info.FreeApiCalls);
        Assert.Equal("all", info.ApiSizes);
    }

    [Fact]
    public async Task GetAccountInfoAsync_NonSuccessResponse_Throws()
    {
        using var client = MakeClient(_ => ErrorJson(HttpStatusCode.Unauthorized,
            """{"errors":[{"title":"Unauthorized","detail":"Invalid API key","code":"unauthorized"}]}"""));

        var ex = await Assert.ThrowsAsync<RemoveBgException>(() => client.GetAccountInfoAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    #endregion

    #region Helpers

    private static RemoveBgClient MakeClient(Func<HttpRequestMessage, HttpResponseMessage> respond)
        => new(ApiKey, new HttpClient(new FakeHttpHandler(respond)));

    private static RemoveBgClient MakeClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
        => new(ApiKey, new HttpClient(new FakeHttpHandler(respond)));

    private static RemoveBgClient MakeClient(FakeHttpHandler handler)
        => new(ApiKey, new HttpClient(handler));

    private static HttpResponseMessage OkImage(
        byte[] bytes, Dictionary<string, string>? headers = null)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        if (headers != null)
            foreach (var (k, v) in headers)
                response.Headers.TryAddWithoutValidation(k, v);
        return response;
    }

    private static HttpResponseMessage OkJson(string json)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };

    private static HttpResponseMessage ErrorJson(HttpStatusCode status, string json)
        => new(status)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };

    private static string AccountJson(
        double total = 100, double subscription = 80, double payg = 20,
        double enterprise = 0, int freeCalls = 50, string sizes = "all")
        => $$"""
             {
               "data": {
                 "attributes": {
                   "credits": {
                     "total": {{total}},
                     "subscription": {{subscription}},
                     "payg": {{payg}},
                     "enterprise": {{enterprise}}
                   },
                   "api": {
                     "free_calls": {{freeCalls}},
                     "sizes": "{{sizes}}"
                   }
                 }
               }
             }
             """;

    private static async Task<string?> ReadFieldAsync(HttpRequestMessage request, string name)
    {
        foreach (var part in (MultipartFormDataContent)request.Content!)
            if (part.Headers.ContentDisposition?.Name?.Trim('"') == name)
                return await part.ReadAsStringAsync();
        return null;
    }

    private static async Task ReadAllFieldsAsync(
        HttpRequestMessage request, Dictionary<string, string> fields)
    {
        foreach (var part in (MultipartFormDataContent)request.Content!)
        {
            var name = part.Headers.ContentDisposition?.Name?.Trim('"');
            if (name != null)
                fields[name] = await part.ReadAsStringAsync();
        }
    }

    #endregion
}
