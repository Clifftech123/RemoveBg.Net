namespace RemoveBg.Net
{
    /// <summary>
    /// Provides background-removal operations against the remove.bg API.
    /// Register via <c>services.AddRemoveBg(apiKey)</c> and inject this interface.
    /// </summary>
    public interface IRemoveBgClient
    {
        /// <summary>Removes the background from an image at a public URL.</summary>
        Task<RemoveBgResult> RemoveFromUrlAsync(
            string imageUrl,
            RemoveBgOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>Removes the background from a local image file.</summary>
        Task<RemoveBgResult> RemoveFromFileAsync(
            string filePath,
            RemoveBgOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>Removes the background from raw image bytes.</summary>
        Task<RemoveBgResult> RemoveFromBytesAsync(
            byte[] imageBytes,
            string fileName = "image.png",
            RemoveBgOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>Retrieves account credit balance and API quota information.</summary>
        Task<AccountInfo> GetAccountInfoAsync(CancellationToken cancellationToken = default);
    }
}
