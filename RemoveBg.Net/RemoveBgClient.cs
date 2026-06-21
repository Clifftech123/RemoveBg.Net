using System.Globalization;
using System.Text.Json;

namespace RemoveBg.Net
{
    public sealed class RemoveBgClient : IRemoveBgClient, IDisposable
    {
        private const string DefaultBaseUrl = "https://api.remove.bg/v1.0/";
        private const string ApiKeyHeader = "X-Api-Key";

        private readonly HttpClient _http;
        private readonly bool _ownsHttpClient;
        private readonly string _apiKey;
        private readonly Uri _baseUrl;

        /// <summary>
        /// Creates a client that owns its own <see cref="HttpClient"/>.
        /// Remember to <see cref="Dispose"/> it (or wrap it in a <c>using</c>).
        /// </summary>
        public RemoveBgClient(string apiKey)
            : this(apiKey, new HttpClient(), ownsHttpClient: true) { }

        /// <summary>
        /// Creates a client using a caller-supplied <see cref="HttpClient"/>.
        /// The client will not be disposed — use this overload with <c>IHttpClientFactory</c>.
        /// </summary>
        public RemoveBgClient(string apiKey, HttpClient httpClient)
            : this(apiKey, httpClient, ownsHttpClient: false) { }

        private RemoveBgClient(string apiKey, HttpClient httpClient, bool ownsHttpClient)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("An API key is required.", nameof(apiKey));

            _apiKey = apiKey;
            _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _ownsHttpClient = ownsHttpClient;
            _baseUrl = new Uri(DefaultBaseUrl);
        }

        /// <summary>Removes the background from an image at a public URL.</summary>
        public Task<RemoveBgResult> RemoveFromUrlAsync(
            string imageUrl,
            RemoveBgOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("An image URL is required.", nameof(imageUrl));

            var content = BuildContent(options);
            content.Add(new StringContent(imageUrl), "image_url");
            return SendAsync(content, cancellationToken);
        }

        /// <summary>Removes the background from a local image file.</summary>
        public async Task<RemoveBgResult> RemoveFromFileAsync(
            string filePath,
            RemoveBgOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A file path is required.", nameof(filePath));

            var bytes = await File.ReadAllBytesAsync(filePath, cancellationToken).ConfigureAwait(false);
            return await RemoveFromBytesAsync(bytes, Path.GetFileName(filePath), options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Removes the background from raw image bytes.</summary>
        public Task<RemoveBgResult> RemoveFromBytesAsync(
            byte[] imageBytes,
            string fileName = "image.png",
            RemoveBgOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (imageBytes is null || imageBytes.Length == 0)
                throw new ArgumentException("Image bytes are required.", nameof(imageBytes));

            var content = BuildContent(options);
            content.Add(new ByteArrayContent(imageBytes), "image_file", fileName);
            return SendAsync(content, cancellationToken);
        }

        /// <summary>Retrieves account credit balance and API quota information.</summary>
        public async Task<AccountInfo> GetAccountInfoAsync(CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseUrl, "account"));
            request.Headers.Add(ApiKeyHeader, _apiKey);
            request.Headers.Accept.ParseAdd("application/json");

            using var response = await _http
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                await ThrowApiExceptionAsync(response, cancellationToken).ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return AccountInfo.Parse(body);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_ownsHttpClient)
                _http.Dispose();
        }

        #region Private helpers

        private static MultipartFormDataContent BuildContent(RemoveBgOptions? options)
        {
            var content = new MultipartFormDataContent();
            if (options != null)
            {
                foreach (var field in options.ToFormFields())
                    content.Add(new StringContent(field.Value), field.Key);
            }
            return content;
        }

        private async Task<RemoveBgResult> SendAsync(
            MultipartFormDataContent content,
            CancellationToken cancellationToken)
        {
            using var _ = content;
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_baseUrl, "removebg"));
            request.Headers.Add(ApiKeyHeader, _apiKey);
            request.Headers.Accept.ParseAdd("image/*");
            request.Content = content;

            using var response = await _http
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                await ThrowApiExceptionAsync(response, cancellationToken).ConfigureAwait(false);

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            return BuildResult(bytes, response);
        }

        private static async Task ThrowApiExceptionAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var errors = ParseErrors(body);
            var retryAfter = GetIntHeader(response, "Retry-After");
            throw new RemoveBgException(response.StatusCode, errors, retryAfter);
        }

        private static IReadOnlyList<RemoveBgError> ParseErrors(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return Array.Empty<RemoveBgError>();

            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("errors", out var errorsEl)
                    && errorsEl.ValueKind == JsonValueKind.Array)
                {
                    return errorsEl.EnumerateArray()
                        .Select(e => new RemoveBgError(
                            e.TryGetProperty("title", out var t) ? t.GetString() : null,
                            e.TryGetProperty("detail", out var d) ? d.GetString() : null,
                            e.TryGetProperty("code", out var c) ? c.GetString() : null))
                        .ToList();
                }
            }
            catch (JsonException) { }

            return [new RemoveBgError(null, body, null)];
        }

        private static RemoveBgResult BuildResult(byte[] content, HttpResponseMessage response)
            => new RemoveBgResult(
                content,
                creditsCharged: GetDoubleHeader(response, "X-Credits-Charged"),
                detectedType: GetStringHeader(response, "X-Type"),
                width: GetIntHeader(response, "X-Width"),
                height: GetIntHeader(response, "X-Height"),
                rateLimit: GetIntHeader(response, "X-RateLimit-Limit"),
                rateLimitRemaining: GetIntHeader(response, "X-RateLimit-Remaining"),
                rateLimitReset: GetLongHeader(response, "X-RateLimit-Reset"),
                retryAfter: GetIntHeader(response, "Retry-After"));

        private static string? GetStringHeader(HttpResponseMessage response, string name)
            => response.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;

        private static int? GetIntHeader(HttpResponseMessage response, string name)
            => int.TryParse(GetStringHeader(response, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
                ? v : null;

        private static long? GetLongHeader(HttpResponseMessage response, string name)
            => long.TryParse(GetStringHeader(response, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
                ? v : null;

        private static double? GetDoubleHeader(HttpResponseMessage response, string name)
            => double.TryParse(GetStringHeader(response, name), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
                ? v : null;

        #endregion
    }
}
