namespace RemoveBg.Net.Tests;

public class RemoveBgOptionsTests
{
    [Fact]
    public void ToFormFields_NoPropertiesSet_ReturnsEmpty()
    {
        Assert.Empty(new RemoveBgOptions().ToFormFields());
    }

    [Theory]
    [InlineData(ImageSize.Auto, "auto")]
    [InlineData(ImageSize.Preview, "preview")]
    [InlineData(ImageSize.Small, "small")]
    [InlineData(ImageSize.Regular, "regular")]
    [InlineData(ImageSize.Medium, "medium")]
    [InlineData(ImageSize.Hd, "hd")]
    [InlineData(ImageSize.Full, "full")]
    [InlineData(ImageSize.FourK, "4k")]
    public void ToFormFields_Size_MapsToApiValue(ImageSize size, string expected)
    {
        var fields = Fields(new RemoveBgOptions { Size = size });
        Assert.Equal(expected, fields["size"]);
    }

    [Theory]
    [InlineData(ImageType.Auto, "auto")]
    [InlineData(ImageType.Person, "person")]
    [InlineData(ImageType.Product, "product")]
    [InlineData(ImageType.Car, "car")]
    [InlineData(ImageType.Animal, "animal")]
    [InlineData(ImageType.Graphic, "graphic")]
    [InlineData(ImageType.Transportation, "transportation")]
    public void ToFormFields_Type_MapsToLowercase(ImageType type, string expected)
    {
        var fields = Fields(new RemoveBgOptions { Type = type });
        Assert.Equal(expected, fields["type"]);
    }

    [Theory]
    [InlineData(OutputFormat.Auto, "auto")]
    [InlineData(OutputFormat.Png, "png")]
    [InlineData(OutputFormat.Jpg, "jpg")]
    [InlineData(OutputFormat.Zip, "zip")]
    public void ToFormFields_Format_MapsToLowercase(OutputFormat format, string expected)
    {
        var fields = Fields(new RemoveBgOptions { Format = format });
        Assert.Equal(expected, fields["format"]);
    }

    [Theory]
    [InlineData(ChannelsType.Rgba, "rgba")]
    [InlineData(ChannelsType.Alpha, "alpha")]
    public void ToFormFields_Channels_MapsToLowercase(ChannelsType channels, string expected)
    {
        var fields = Fields(new RemoveBgOptions { Channels = channels });
        Assert.Equal(expected, fields["channels"]);
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void ToFormFields_BoolFields_SerializeAsTrueFalseString(bool value, string expected)
    {
        var fields = Fields(new RemoveBgOptions
        {
            Crop = value,
            AddShadow = value,
            Semitransparency = value
        });

        Assert.Equal(expected, fields["crop"]);
        Assert.Equal(expected, fields["add_shadow"]);
        Assert.Equal(expected, fields["semitransparency"]);
    }

    [Fact]
    public void ToFormFields_StringProperties_MapToCorrectApiKeys()
    {
        var fields = Fields(new RemoveBgOptions
        {
            Scale = "50%",
            Position = "center",
            RegionOfInterest = "0% 0% 100% 100%",
            CropMargin = "10px",
            BackgroundColor = "81d4fa",
            BackgroundImageUrl = "https://example.com/bg.jpg"
        });

        Assert.Equal("50%", fields["scale"]);
        Assert.Equal("center", fields["position"]);
        Assert.Equal("0% 0% 100% 100%", fields["roi"]);
        Assert.Equal("10px", fields["crop_margin"]);
        Assert.Equal("81d4fa", fields["bg_color"]);
        Assert.Equal("https://example.com/bg.jpg", fields["bg_image_url"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ToFormFields_NullOrEmptyStringProperties_NotIncluded(string? value)
    {
        var fields = Fields(new RemoveBgOptions
        {
            Scale = value,
            Position = value,
            RegionOfInterest = value,
            CropMargin = value,
            BackgroundColor = value,
            BackgroundImageUrl = value
        });

        Assert.DoesNotContain("scale", fields.Keys);
        Assert.DoesNotContain("position", fields.Keys);
        Assert.DoesNotContain("roi", fields.Keys);
        Assert.DoesNotContain("crop_margin", fields.Keys);
        Assert.DoesNotContain("bg_color", fields.Keys);
        Assert.DoesNotContain("bg_image_url", fields.Keys);
    }

    [Fact]
    public void ToFormFields_OnlySetPropertiesAreIncluded()
    {
        var fields = Fields(new RemoveBgOptions { Size = ImageSize.Hd });

        Assert.Single(fields);
        Assert.True(fields.ContainsKey("size"));
    }

    private static Dictionary<string, string> Fields(RemoveBgOptions options)
        => options.ToFormFields().ToDictionary(f => f.Key, f => f.Value);
}
