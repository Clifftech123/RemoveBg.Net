namespace RemoveBg.Net
{

    /// <summary>
    /// The outcome of a successful background-removal call: the processed image bytes
    /// plus the metadata remove.bg returns in the response headers.
    /// </summary>
    public sealed class RemoveBgResult
    {
        internal RemoveBgResult(
            byte[] content,
            double? creditsCharged,
            string? detectedType,
            int? width,
            int? height,
            int? rateLimit,
            int? rateLimitRemaining,
            long? rateLimitReset,
            int? retryAfter)
        {
            Content = content;
            CreditsCharged = creditsCharged;
            DetectedType = detectedType;
            Width = width;
            Height = height;
            RateLimit = rateLimit;
            RateLimitRemaining = rateLimitRemaining;
            RateLimitReset = rateLimitReset;
            RetryAfter = retryAfter;
        }

        /// <summary>The processed image as raw bytes (PNG, JPG or ZIP depending on the request).</summary>
        public byte[] Content { get; }

        /// <summary>The processed image as a base64-encoded string.</summary>
        public string Base64 => Convert.ToBase64String(Content);

        /// <summary>Credits charged for this call (<c>X-Credits-Charged</c>).</summary>
        public double? CreditsCharged { get; }

        /// <summary>Foreground type the API detected (<c>X-Type</c>).</summary>
        public string? DetectedType { get; }

        /// <summary>Width of the result image in pixels (<c>X-Width</c>).</summary>
        public int? Width { get; }

        /// <summary>Height of the result image in pixels (<c>X-Height</c>).</summary>
        public int? Height { get; }

        /// <summary>Total rate limit in megapixel-images per minute (<c>X-RateLimit-Limit</c>).</summary>
        public int? RateLimit { get; }

        /// <summary>Remaining rate limit for the current window (<c>X-RateLimit-Remaining</c>).</summary>
        public int? RateLimitRemaining { get; }

        /// <summary>Unix timestamp when the rate limit resets (<c>X-RateLimit-Reset</c>).</summary>
        public long? RateLimitReset { get; }

        /// <summary>Seconds until the rate limit resets, present only when exceeded (<c>Retry-After</c>).</summary>
        public int? RetryAfter { get; }

        /// <summary>Writes the processed image to disk asynchronously.</summary>
        public async Task SaveAsync(string path, CancellationToken cancellationToken = default)
        {
#if NETSTANDARD2_0
        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await fs.WriteAsync(Content, 0, Content.Length, cancellationToken).ConfigureAwait(false);
        }
#else
            await File.WriteAllBytesAsync(path, Content, cancellationToken).ConfigureAwait(false);
#endif
        }

        /// <summary>Writes the processed image to disk.</summary>
        public void Save(string path) => File.WriteAllBytes(path, Content);
    }

}
