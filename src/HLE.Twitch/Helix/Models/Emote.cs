using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using HLE.Strings;
using HLE.Twitch.JsonConverters;

namespace HLE.Twitch.Helix.Models;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("\"{Name}\"")]
public class Emote : IEquatable<Emote>
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

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

    internal static readonly FrozenDictionary<EmoteImageFormats, string> s_imageFormatValues = new Dictionary<EmoteImageFormats, string>(2)
    {
        { EmoteImageFormats.Static, "static" },
        { EmoteImageFormats.Animated, "animated" }
    }.ToFrozenDictionary();

    internal static readonly FrozenDictionary<EmoteScales, string> s_scaleValues = new Dictionary<EmoteScales, string>(3)
    {
        { EmoteScales.One, "1.0" },
        { EmoteScales.Two, "2.0" },
        { EmoteScales.Three, "3.0" }
    }.ToFrozenDictionary();

    internal static readonly FrozenDictionary<EmoteThemes, string> s_themeValues = new Dictionary<EmoteThemes, string>(2)
    {
        { EmoteThemes.Light, "light" },
        { EmoteThemes.Dark, "dark" }
    }.ToFrozenDictionary();

    [SkipLocalsInit]
    public bool TryGetImageUrl(EmoteImageFormats format, EmoteThemes theme, EmoteScales scale, [MaybeNullWhen(false)] out string url)
    {
        url = null;
        using ValueStringBuilder urlBuilder = new(stackalloc char[256]);
        urlBuilder.Append("https://static-cdn.jtvnw.net/emoticons/v2/");
        if ((Formats & format) == 0 || !BitOperations.IsPow2((int)format))
        {
            return false;
        }

        urlBuilder.Append(Id);
        urlBuilder.Append('/');
        urlBuilder.Append(s_imageFormatValues[format]);

        if ((Themes & theme) == 0 || !BitOperations.IsPow2((int)theme))
        {
            return false;
        }

        urlBuilder.Append('/');
        urlBuilder.Append(s_themeValues[theme]);

        if ((Scales & scale) == 0 || !BitOperations.IsPow2((int)scale))
        {
            return false;
        }

        urlBuilder.Append('/');
        urlBuilder.Append(s_scaleValues[scale]);
        url = StringPool.Shared.GetOrAdd(urlBuilder.WrittenSpan);
        return true;
    }

    public override string ToString() => Name;

    public bool Equals(Emote? other) => ReferenceEquals(this, other);

    public override bool Equals(object? obj) => obj is Emote other && Equals(other);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Emote? left, Emote? right) => Equals(left, right);

    public static bool operator !=(Emote? left, Emote? right) => !(left == right);
}
