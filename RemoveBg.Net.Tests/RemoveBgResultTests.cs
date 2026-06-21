namespace RemoveBg.Net.Tests;

public class RemoveBgResultTests
{
    private static readonly byte[] SampleBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A];

    private static RemoveBgResult Make(byte[]? content = null)
        => new(
            content ?? SampleBytes,
            creditsCharged: 1.5,
            detectedType: "person",
            width: 800,
            height: 600,
            rateLimit: 500,
            rateLimitRemaining: 499,
            rateLimitReset: 1700000000L,
            retryAfter: null);

    [Fact]
    public void Content_ReturnsSuppliedBytes()
    {
        Assert.Equal(SampleBytes, Make().Content);
    }

    [Fact]
    public void Base64_EncodesContentCorrectly()
    {
        Assert.Equal(Convert.ToBase64String(SampleBytes), Make().Base64);
    }

    [Fact]
    public async Task SaveAsync_WritesContentToDisk()
    {
        var path = Path.GetTempFileName();
        try
        {
            await Make().SaveAsync(path);
            Assert.Equal(SampleBytes, await File.ReadAllBytesAsync(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Save_WritesContentToDisk()
    {
        var path = Path.GetTempFileName();
        try
        {
            Make().Save(path);
            Assert.Equal(SampleBytes, File.ReadAllBytes(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void HeaderProperties_AreSetCorrectly()
    {
        var result = Make();

        Assert.Equal(1.5, result.CreditsCharged);
        Assert.Equal("person", result.DetectedType);
        Assert.Equal(800, result.Width);
        Assert.Equal(600, result.Height);
        Assert.Equal(500, result.RateLimit);
        Assert.Equal(499, result.RateLimitRemaining);
        Assert.Equal(1700000000L, result.RateLimitReset);
        Assert.Null(result.RetryAfter);
    }

    [Fact]
    public void NullableHeaderProperties_CanBeNull()
    {
        var result = new RemoveBgResult(
            SampleBytes,
            creditsCharged: null,
            detectedType: null,
            width: null,
            height: null,
            rateLimit: null,
            rateLimitRemaining: null,
            rateLimitReset: null,
            retryAfter: null);

        Assert.Null(result.CreditsCharged);
        Assert.Null(result.DetectedType);
        Assert.Null(result.Width);
        Assert.Null(result.Height);
        Assert.Null(result.RateLimit);
        Assert.Null(result.RateLimitRemaining);
        Assert.Null(result.RateLimitReset);
        Assert.Null(result.RetryAfter);
    }
}
