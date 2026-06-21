using Microsoft.Extensions.DependencyInjection;

namespace RemoveBg.Net
{
    public static class RemoveBgServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="IRemoveBgClient"/> with the DI container.
        /// The underlying <see cref="HttpClient"/> is managed by <c>IHttpClientFactory</c>.
        /// </summary>
        /// <example>
        /// // Program.cs
        /// builder.Services.AddRemoveBg("your-api-key");
        ///
        /// // Anywhere via DI
        /// public class MyService(IRemoveBgClient removeBg) { ... }
        /// </example>
        public static IServiceCollection AddRemoveBg(this IServiceCollection services, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("An API key is required.", nameof(apiKey));

            services.AddHttpClient(nameof(RemoveBgClient));

            services.AddSingleton<IRemoveBgClient>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                return new RemoveBgClient(apiKey, factory.CreateClient(nameof(RemoveBgClient)));
            });

            return services;
        }
    }
}
