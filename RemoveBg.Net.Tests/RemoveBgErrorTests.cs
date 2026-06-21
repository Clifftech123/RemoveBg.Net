namespace RemoveBg.Net.Tests;

public class RemoveBgErrorTests
{
    [Fact]
    public void ToString_TitleAndDetail_ReturnsTitleColonDetail()
    {
        var error = new RemoveBgError("Bad Request", "Image is corrupted", "invalid_image");
        Assert.Equal("Bad Request: Image is corrupted", error.ToString());
    }

    [Fact]
    public void ToString_NoTitle_ReturnsDetail()
    {
        var error = new RemoveBgError(null, "Something went wrong", null);
        Assert.Equal("Something went wrong", error.ToString());
    }

    [Fact]
    public void ToString_EmptyTitle_ReturnsDetail()
    {
        var error = new RemoveBgError("", "Something went wrong", null);
        Assert.Equal("Something went wrong", error.ToString());
    }

    [Fact]
    public void ToString_NoTitleNoDetail_ReturnsUnknownError()
    {
        var error = new RemoveBgError(null, null, null);
        Assert.Equal("Unknown error", error.ToString());
    }

}
