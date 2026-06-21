namespace RemoveBg.Net
{

    /// <summary>Output resolution. See https://www.remove.bg/api for credit costs.</summary>
    /// 

    public enum ImageSize
    {
        Auto,
        Preview,
        Small,
        Regular,
        Medium,
        Hd,
        Full,
        FourK
    }

    /// <summary>Foreground type hint sent to the API.</summary>
    public enum ImageType
    {
        Auto,
        Person,
        Product,
        Car,
        Animal,
        Graphic,
        Transportation
    }

    /// <summary>Result image format.</summary>
    public enum OutputFormat
    {
        Auto,
        Png,
        Jpg,
        Zip
    }

    /// <summary>Whether to return the finished image or just the alpha mask.</summary>
    public enum ChannelsType
    {
        Rgba,
        Alpha
    }

    internal static class ApiValues
    {
        public static string ToApiValue(this ImageSize size)
            => size == ImageSize.FourK ? "4k" : size.ToString().ToLowerInvariant();

        public static string ToApiValue(this ImageType type) => type.ToString().ToLowerInvariant();

        public static string ToApiValue(this OutputFormat format) => format.ToString().ToLowerInvariant();

        public static string ToApiValue(this ChannelsType channels) => channels.ToString().ToLowerInvariant();
    }


    /// <summary>
    /// Optional parameters for a background-removal request. Only properties that are
    /// set are sent to the API; everything else falls back to the API defaults.
    /// </summary>
    public sealed class RemoveBgOptions
    {
        /// <summary>Maximum output resolution (<c>size</c>).</summary>
        public ImageSize? Size { get; set; }

        /// <summary>Foreground type hint (<c>type</c>).</summary>
        public ImageType? Type { get; set; }

        /// <summary>Result image format (<c>format</c>).</summary>
        public OutputFormat? Format { get; set; }

        /// <summary>Scale of the subject, e.g. <c>"50%"</c> or <c>"original"</c> (<c>scale</c>).</summary>
        public string? Scale { get; set; }

        /// <summary>Subject position, e.g. <c>"center"</c> or <c>"30% 40%"</c> (<c>position</c>).</summary>
        public string? Position { get; set; }

        /// <summary>Region of interest, e.g. <c>"0% 0% 100% 100%"</c> (<c>roi</c>).</summary>
        public string? RegionOfInterest { get; set; }

        /// <summary>Crop off empty regions around the subject (<c>crop</c>).</summary>
        public bool? Crop { get; set; }

        /// <summary>Margin added around the cropped subject, e.g. <c>"30px"</c> (<c>crop_margin</c>).</summary>
        public string? CropMargin { get; set; }

        /// <summary>Solid background color, e.g. <c>"81d4fa"</c> or <c>"green"</c> (<c>bg_color</c>).</summary>
        public string? BackgroundColor { get; set; }

        /// <summary>Background image URL (<c>bg_image_url</c>).</summary>
        public string? BackgroundImageUrl { get; set; }

        /// <summary>Request the finished image (<c>rgba</c>) or the alpha mask (<c>channels</c>).</summary>
        public ChannelsType? Channels { get; set; }

        /// <summary>Add an artificial shadow (car photos only, currently) (<c>add_shadow</c>).</summary>
        public bool? AddShadow { get; set; }

        /// <summary>Keep semi-transparent regions, e.g. glass or hair (<c>semitransparency</c>).</summary>
        public bool? Semitransparency { get; set; }

        internal IEnumerable<KeyValuePair<string, string>> ToFormFields()
        {
            if (Size.HasValue) yield return Field("size", Size.Value.ToApiValue());
            if (Type.HasValue) yield return Field("type", Type.Value.ToApiValue());
            if (Format.HasValue) yield return Field("format", Format.Value.ToApiValue());
            if (!string.IsNullOrEmpty(Scale)) yield return Field("scale", Scale!);
            if (!string.IsNullOrEmpty(Position)) yield return Field("position", Position!);
            if (!string.IsNullOrEmpty(RegionOfInterest)) yield return Field("roi", RegionOfInterest!);
            if (Crop.HasValue) yield return Field("crop", Bool(Crop.Value));
            if (!string.IsNullOrEmpty(CropMargin)) yield return Field("crop_margin", CropMargin!);
            if (!string.IsNullOrEmpty(BackgroundColor)) yield return Field("bg_color", BackgroundColor!);
            if (!string.IsNullOrEmpty(BackgroundImageUrl)) yield return Field("bg_image_url", BackgroundImageUrl!);
            if (Channels.HasValue) yield return Field("channels", Channels.Value.ToApiValue());
            if (AddShadow.HasValue) yield return Field("add_shadow", Bool(AddShadow.Value));
            if (Semitransparency.HasValue) yield return Field("semitransparency", Bool(Semitransparency.Value));
        }

        private static string Bool(bool value) => value ? "true" : "false";

        private static KeyValuePair<string, string> Field(string key, string value)
            => new KeyValuePair<string, string>(key, value);
    }

}
