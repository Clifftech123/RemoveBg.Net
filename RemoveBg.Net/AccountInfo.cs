using System.Text.Json;

namespace RemoveBg;

/// <summary>Credit balance and free-call allowance for a remove.bg account.</summary>
public sealed class AccountInfo
{
    internal AccountInfo(
        double totalCredits,
        double subscriptionCredits,
        double payAsYouGoCredits,
        double enterpriseCredits,
        int freeApiCalls,
        string? apiSizes)
    {
        TotalCredits = totalCredits;
        SubscriptionCredits = subscriptionCredits;
        PayAsYouGoCredits = payAsYouGoCredits;
        EnterpriseCredits = enterpriseCredits;
        FreeApiCalls = freeApiCalls;
        ApiSizes = apiSizes;
    }

    /// <summary>Total credits available across all buckets.</summary>
    public double TotalCredits { get; }

    /// <summary>Credits from an active subscription.</summary>
    public double SubscriptionCredits { get; }

    /// <summary>Pay-as-you-go credits.</summary>
    public double PayAsYouGoCredits { get; }

    /// <summary>Enterprise credits.</summary>
    public double EnterpriseCredits { get; }

    /// <summary>Remaining free preview API calls for the current period.</summary>
    public int FreeApiCalls { get; }

    /// <summary>Sizes permitted by the free tier (e.g. <c>"all"</c> or <c>"preview"</c>).</summary>
    public string? ApiSizes { get; }

    internal static AccountInfo Parse(string json)
    {
        using (var doc = JsonDocument.Parse(json))
        {
            var attributes = doc.RootElement.GetProperty("data").GetProperty("attributes");
            var credits = attributes.GetProperty("credits");
            var api = attributes.GetProperty("api");

            return new AccountInfo(
                GetDouble(credits, "total"),
                GetDouble(credits, "subscription"),
                GetDouble(credits, "payg"),
                GetDouble(credits, "enterprise"),
                api.TryGetProperty("free_calls", out var fc) && fc.TryGetInt32(out var fcv) ? fcv : 0,
                api.TryGetProperty("sizes", out var s) ? s.GetString() : null);
        }
    }

    private static double GetDouble(JsonElement element, string name)
        => element.TryGetProperty(name, out var v) && v.TryGetDouble(out var d) ? d : 0d;
}
