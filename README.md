# RemoveBg.Net

An unofficial .NET client for the [remove.bg](https://www.remove.bg/api) background-removal API.

This is the .NET counterpart to the existing [Node](https://www.npmjs.com/package/remove.bg),
[Ruby](https://github.com/remove-bg/ruby) and PHP clients. It is **not** affiliated with
or endorsed by Kaleido (the company behind remove.bg).

- Targets `netstandard2.0` and `net10`
- URL, file, byte-array, stream and base64 inputs
- Strongly-typed options and result metadata
- Friendly error handling via `RemoveBgException`
- Account / credit lookup

## Install

```bash
dotnet add package RemoveBg.Net
```

## Quick start

```csharp
using RemoveBg;

using var client = new RemoveBgClient("YOUR_API_KEY");

var result = await client.RemoveFromFileAsync("input.jpg");
await result.SaveAsync("output.png");

Console.WriteLine($"Charged {result.CreditsCharged} credits, detected: {result.DetectedType}");
```

## Inputs

```csharp
// From a public URL
var fromUrl = await client.RemoveFromUrlAsync("https://example.com/photo.jpg");

// From a local file
var fromFile = await client.RemoveFromFileAsync("photo.jpg");

// From raw bytes
byte[] bytes = await File.ReadAllBytesAsync("photo.jpg");
var fromBytes = await client.RemoveFromBytesAsync(bytes, "photo.jpg");

// From a stream
await using var stream = File.OpenRead("photo.jpg");
var fromStream = await client.RemoveFromStreamAsync(stream, "photo.jpg");

// From a base64 string
var fromBase64 = await client.RemoveFromBase64Async(base64String);
```

Every method returns a `RemoveBgResult`. Access the bytes directly via `result.Content`,
the base64 form via `result.Base64`, or persist with `result.Save(path)` / `result.SaveAsync(path)`.

## Options

```csharp
var options = new RemoveBgOptions
{
    Size            = ImageSize.Full,      // preview (default) | full | 4k | auto | ...
    Type            = ImageType.Person,    // auto | person | product | car | ...
    Format          = OutputFormat.Png,    // auto | png | jpg | zip
    Crop            = true,
    CropMargin      = "10%",
    BackgroundColor = "81d4fa",            // hex or color name
    Channels        = ChannelsType.Rgba,   // rgba (default) | alpha
    AddShadow       = false,
    Scale           = "80%",
    Position        = "center"
};

var result = await client.RemoveFromFileAsync("car.jpg", options);
await result.SaveAsync("car-no-bg.png");
```

Only the properties you set are sent; everything else uses the remove.bg defaults.

## Error handling

A non-success response throws `RemoveBgException`, which carries the status code and the
structured errors from the API:

```csharp
try
{
    var result = await client.RemoveFromFileAsync("photo.jpg");
}
catch (RemoveBgException ex)
{
    if (ex.IsRateLimited)
        Console.WriteLine($"Rate limited. Retry after {ex.RetryAfter}s.");

    foreach (var error in ex.Errors)
        Console.WriteLine($"{error.Title}: {error.Detail}");
}
```

## Account & credits

```csharp
var account = await client.GetAccountAsync();
Console.WriteLine($"Total credits: {account.TotalCredits}");
Console.WriteLine($"Free preview calls left: {account.FreeApiCalls}");
```

## Dependency injection / IHttpClientFactory

Pass your own `HttpClient` to use the factory and avoid socket exhaustion. In this overload
the client does **not** dispose the `HttpClient` for you:

```csharp
services.AddHttpClient();

services.AddTransient(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    return new RemoveBgClient("YOUR_API_KEY", http);
});
```




## Building

```bash
dotnet build
dotnet test
dotnet pack src/RemoveBg.Net -c Release   # produces the .nupkg
```

## License

MIT
