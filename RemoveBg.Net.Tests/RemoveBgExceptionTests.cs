using System.Net;

namespace RemoveBg.Net.Tests;

public class RemoveBgExceptionTests
{
    [Fact]
    public void Message_IncludesStatusCodeAndFirstErrorDetail()
    {
        var errors = new List<RemoveBgError> { new("Invalid image", "File is corrupted", "invalid_image") };
        var ex = new RemoveBgException(HttpStatusCode.UnprocessableEntity, errors, retryAfter: null);

        Assert.Contains("422", ex.Message);
        Assert.Contains("Invalid image", ex.Message);
    }

    [Fact]
    public void Message_NoErrors_ShowsUnknownError()
    {
        var ex = new RemoveBgException(HttpStatusCode.InternalServerError, [], retryAfter: null);

        Assert.Contains("500", ex.Message);
        Assert.Contains("Unknown error", ex.Message);
    }

    [Fact]
    public void IsRateLimited_Status429_ReturnsTrue()
    {
        var ex = new RemoveBgException(HttpStatusCode.TooManyRequests, [], retryAfter: 30);
        Assert.True(ex.IsRateLimited);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.UnprocessableEntity)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public void IsRateLimited_NonRateLimitStatus_ReturnsFalse(HttpStatusCode status)
    {
        var ex = new RemoveBgException(status, [], retryAfter: null);
        Assert.False(ex.IsRateLimited);
    }

    [Fact]
    public void RetryAfter_IsSetWhenProvided()
    {
        var ex = new RemoveBgException(HttpStatusCode.TooManyRequests, [], retryAfter: 45);
        Assert.Equal(45, ex.RetryAfter);
    }

    [Fact]
    public void RetryAfter_IsNullWhenNotProvided()
    {
        var ex = new RemoveBgException(HttpStatusCode.InternalServerError, [], retryAfter: null);
        Assert.Null(ex.RetryAfter);
    }

    [Fact]
    public void Errors_AreAccessible()
    {
        var errors = new List<RemoveBgError>
        {
            new("Title A", "Detail A", "code_a"),
            new("Title B", "Detail B", "code_b")
        };
        var ex = new RemoveBgException(HttpStatusCode.UnprocessableEntity, errors, retryAfter: null);

        Assert.Equal(2, ex.Errors.Count);
        Assert.Equal("Title A", ex.Errors[0].Title);
        Assert.Equal("code_b", ex.Errors[1].Code);
    }

}
