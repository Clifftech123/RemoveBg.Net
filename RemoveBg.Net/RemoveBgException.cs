using System.Net;

namespace RemoveBg.Net
{

    /// <summary>A single error entry returned by the remove.bg API.</summary>
    public sealed class RemoveBgError
    {
        public RemoveBgError(string? title, string? detail, string? code)
        {
            Title = title;
            Detail = detail;
            Code = code;
        }

        /// <summary>Short error title.</summary>
        public string? Title { get; }

        /// <summary>Human-readable error detail.</summary>
        public string? Detail { get; }

        /// <summary>Machine-readable error code, when provided.</summary>
        public string? Code { get; }

        public override string ToString()
            => string.IsNullOrEmpty(Title) ? (Detail ?? "Unknown error") : $"{Title}: {Detail}";
    }

    /// <summary>Thrown when the remove.bg API returns a non-success status code.</summary>
    public sealed class RemoveBgException : Exception
    {
        public RemoveBgException(HttpStatusCode statusCode, IReadOnlyList<RemoveBgError> errors, int? retryAfter)
            : base(BuildMessage(statusCode, errors))
        {
            StatusCode = statusCode;
            Errors = errors;
            RetryAfter = retryAfter;
        }

        /// <summary>The HTTP status code returned by the API.</summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>The errors reported in the response body.</summary>
        public IReadOnlyList<RemoveBgError> Errors { get; }

        /// <summary>Seconds to wait before retrying, when the call was rate limited.</summary>
        public int? RetryAfter { get; }

        /// <summary><c>true</c> when the request failed because of rate limiting (HTTP 429).</summary>
        public bool IsRateLimited => (int)StatusCode == 429;

        private static string BuildMessage(HttpStatusCode statusCode, IReadOnlyList<RemoveBgError> errors)
        {
            var detail = errors != null && errors.Count > 0 ? errors[0].ToString() : "Unknown error";
            return $"remove.bg request failed ({(int)statusCode} {statusCode}): {detail}";
        }
    }

}
