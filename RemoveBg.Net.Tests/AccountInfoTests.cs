namespace RemoveBg.Net.Tests;

public class AccountInfoTests
{
    [Fact]
    public void Parse_FullJson_ExtractsAllFields()
    {
        var json = """
            {
              "data": {
                "attributes": {
                  "credits": {
                    "total": 200.5,
                    "subscription": 150.0,
                    "payg": 50.5,
                    "enterprise": 2.0
                  },
                  "api": {
                    "free_calls": 42,
                    "sizes": "preview"
                  }
                }
              }
            }
            """;

        var info = AccountInfo.Parse(json);

        Assert.Equal(200.5, info.TotalCredits);
        Assert.Equal(150.0, info.SubscriptionCredits);
        Assert.Equal(50.5, info.PayAsYouGoCredits);
        Assert.Equal(2.0, info.EnterpriseCredits);
        Assert.Equal(42, info.FreeApiCalls);
        Assert.Equal("preview", info.ApiSizes);
    }

    [Fact]
    public void Parse_MissingCreditFields_DefaultsToZero()
    {
        var json = """
            {
              "data": {
                "attributes": {
                  "credits": {},
                  "api": {}
                }
              }
            }
            """;

        var info = AccountInfo.Parse(json);

        Assert.Equal(0.0, info.TotalCredits);
        Assert.Equal(0.0, info.SubscriptionCredits);
        Assert.Equal(0.0, info.PayAsYouGoCredits);
        Assert.Equal(0.0, info.EnterpriseCredits);
        Assert.Equal(0, info.FreeApiCalls);
        Assert.Null(info.ApiSizes);
    }

    [Fact]
    public void Parse_MissingSizesField_ApiSizesIsNull()
    {
        var json = """
            {
              "data": {
                "attributes": {
                  "credits": { "total": 100 },
                  "api": { "free_calls": 5 }
                }
              }
            }
            """;

        var info = AccountInfo.Parse(json);

        Assert.Null(info.ApiSizes);
        Assert.Equal(5, info.FreeApiCalls);
    }

}
