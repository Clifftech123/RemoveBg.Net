# RemoveBg.Net

An unofficial .NET client for the [remove.bg](https://www.remove.bg/api) background-removal API.

> Not affiliated with or endorsed by Kaleido (the company behind remove.bg).

## Install

```bash
dotnet add package RemoveBg.Net
```

## 1 — Register

One line in `Program.cs`:

```csharp
builder.Services.AddRemoveBg("YOUR_API_KEY");
```

That's it. `IHttpClientFactory` is wired up automatically so connection pooling and DNS refresh are handled for you.

## 2 — Inject and use

Inject `IRemoveBgClient` into any service, controller, or minimal-API handler:

```csharp
public class ImageService(IRemoveBgClient removeBg)
{
    public async Task<byte[]> RemoveBackgroundAsync(string imageUrl)
    {
        var result = await removeBg.RemoveFromUrlAsync(imageUrl);
        return result.Content;
    }
}
```

## Methods

| Method | Input |
|---|---|
| `RemoveFromUrlAsync(url, options?, ct?)` | Public image URL |
| `RemoveFromFileAsync(path, options?, ct?)` | Local file path |
| `RemoveFromBytesAsync(bytes, fileName?, options?, ct?)` | Raw byte array |
| `GetAccountInfoAsync(ct?)` | — returns `AccountInfo` |

The result exposes `result.Content` (bytes), `result.Base64`, `result.Save(path)`, and `result.SaveAsync(path)`.

## Options

Pass a `RemoveBgOptions` to any remove method — only the properties you set are sent:

```csharp
var result = await removeBg.RemoveFromFileAsync("photo.jpg", new RemoveBgOptions
{
    Size            = ImageSize.Full,
    Type            = ImageType.Person,
    Format          = OutputFormat.Png,
    BackgroundColor = "81d4fa",
    Crop            = true,
    CropMargin      = "10%",
});
```

## Error handling

```csharp
try
{
    var result = await removeBg.RemoveFromUrlAsync(url);
}
catch (RemoveBgException ex)
{
    if (ex.IsRateLimited)
        Console.WriteLine($"Rate limited — retry after {ex.RetryAfter}s");

    foreach (var error in ex.Errors)
        Console.WriteLine($"{error.Title}: {error.Detail} (code: {error.Code})");
}
```

## Account & credits

```csharp
var info = await removeBg.GetAccountInfoAsync();
Console.WriteLine($"Credits remaining: {info.TotalCredits}");
Console.WriteLine($"Free calls left:   {info.FreeApiCalls}");
```

---

**No DI?** For scripts or console apps, instantiate directly and dispose when done:

```csharp
using var client = new RemoveBgClient("YOUR_API_KEY");
var result = await client.RemoveFromFileAsync("input.jpg");
await result.SaveAsync("output.png");
```

## License

MIT
