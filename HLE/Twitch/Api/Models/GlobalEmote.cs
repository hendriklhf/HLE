using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using HLE.Strings;
using HLE.Twitch.Api.JsonConverters;

namespace HLE.Twitch.Api.Models;

public sealed class GlobalEmote : Emote, IEquatable<GlobalEmote>
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(Int64StringJsonConverter))]
    public required long Id { get; init; }

    public bool TryGetImageUrl(EmoteImageFormats format, EmoteThemes theme, EmoteScales scale, [MaybeNullWhen(false)] out string url)
    {
        url = null;
        ValueStringBuilder urlBuilder = stackalloc char[250];
        urlBuilder.Append("https://static-cdn.jtvnw.net/emoticons/v2/");
        if ((Formats & format) != format)
        {
            return false;
        }

        urlBuilder.Append(Id);
        urlBuilder.Append('/');
        urlBuilder.Append(ImageFormatValues[format]);

        if ((Themes & theme) != theme)
        {
            return false;
        }

        urlBuilder.Append('/');
        urlBuilder.Append(ThemeValues[theme]);

        if ((Scales & scale) != scale)
        {
            return false;
        }

        urlBuilder.Append('/');
        urlBuilder.Append(ScaleValues[scale]);
        url = StringPool.Shared.GetOrAdd(urlBuilder.WrittenSpan);
        return true;
    }

    public bool Equals(GlobalEmote? other)
    {
        return ReferenceEquals(this, other) || Id == other?.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is GlobalEmote other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(GlobalEmote? left, GlobalEmote? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(GlobalEmote? left, GlobalEmote? right)
    {
        return !(left == right);
    }
}
