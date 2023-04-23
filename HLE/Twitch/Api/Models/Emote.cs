using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using HLE.Twitch.Api.JsonConverters;

namespace HLE.Twitch.Api.Models;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("\"{Name}\"")]
public abstract class Emote
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("format")]
    [JsonConverter(typeof(EmoteImageFormatConverter))]
    public required EmoteImageFormats Formats { get; init; }

    [JsonPropertyName("scale")]
    [JsonConverter(typeof(EmoteScaleJsonConverter))]
    public required EmoteScales Scales { get; init; }

    [JsonPropertyName("theme_mode")]
    [JsonConverter(typeof(EmoteThemeJsonConverter))]
    public required EmoteThemes Themes { get; init; }

    internal static Dictionary<EmoteImageFormats, string> ImageFormatValues { get; } = new()
    {
        { EmoteImageFormats.Static, "static" },
        { EmoteImageFormats.Animated, "animated" }
    };

    internal static Dictionary<EmoteScales, string> ScaleValues { get; } = new()
    {
        { EmoteScales.One, "1.0" },
        { EmoteScales.Two, "2.0" },
        { EmoteScales.Three, "3.0" }
    };

    internal static Dictionary<EmoteThemes, string> ThemeValues { get; } = new()
    {
        { EmoteThemes.Light, "light" },
        { EmoteThemes.Dark, "dark" }
    };

    public override string ToString()
    {
        return Name;
    }
}
