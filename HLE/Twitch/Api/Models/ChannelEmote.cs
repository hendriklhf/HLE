using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using HLE.Strings;
using HLE.Twitch.Api.JsonConverters;

namespace HLE.Twitch.Api.Models;

public sealed class ChannelEmote : Emote, IEquatable<ChannelEmote>
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("emote_type")]
    [JsonConverter(typeof(EmoteTypeJsonConverter))]
    public required EmoteType Type { get; init; }

    [JsonPropertyName("emote_set_id")]
    public required string SetId { get; init; }

    [JsonPropertyName("tier")]
    [JsonConverter(typeof(EmoteTierJsonConverter))]
    public required EmoteTier Tier { get; init; }

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

    public bool Equals(ChannelEmote? other)
    {
        return ReferenceEquals(this, other) || Id == other?.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is ChannelEmote other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(ChannelEmote? left, ChannelEmote? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ChannelEmote? left, ChannelEmote? right)
    {
        return !(left == right);
    }
}
