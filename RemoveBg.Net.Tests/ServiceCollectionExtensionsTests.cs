using Microsoft.Extensions.DependencyInjection;

namespace RemoveBg.Net.Tests;

public class ServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddRemoveBg_InvalidApiKey_Throws(string? apiKey)
    {
        Assert.Throws<ArgumentException>(() =>
            new ServiceCollection().AddRemoveBg(apiKey!));
    }

    [Fact]
    public void AddRemoveBg_RegistersIRemoveBgClient()
    {
        var sp = new ServiceCollection()
            .AddRemoveBg("test-api-key")
            .BuildServiceProvider();

        Assert.NotNull(sp.GetService<IRemoveBgClient>());
    }

    [Fact]
    public void AddRemoveBg_ResolvedClientIsRemoveBgClient()
    {
        var sp = new ServiceCollection()
            .AddRemoveBg("test-api-key")
            .BuildServiceProvider();

        Assert.IsType<RemoveBgClient>(sp.GetRequiredService<IRemoveBgClient>());
    }

    [Fact]
    public void AddRemoveBg_ReturnsTheSameServiceCollection()
    {
        var services = new ServiceCollection();
        var returned = services.AddRemoveBg("test-api-key");
        Assert.Same(services, returned);
    }

    [Fact]
    public void AddRemoveBg_IsSingleton()
    {
        var sp = new ServiceCollection()
            .AddRemoveBg("test-api-key")
            .BuildServiceProvider();

        var a = sp.GetRequiredService<IRemoveBgClient>();
        var b = sp.GetRequiredService<IRemoveBgClient>();
        Assert.Same(a, b);
    }
}
