namespace RemoveBg.Net.Tests.Helpers;

internal sealed class FakeHttpHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _respond;

    internal FakeHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        : this(req => Task.FromResult(respond(req))) { }

    internal FakeHttpHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
        => _respond = respond;

    public List<HttpRequestMessage> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return _respond(request);
    }
}
