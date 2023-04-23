using System;
using System.Text.Json.Serialization;

namespace HLE.Twitch.Api.Models.Responses;

internal readonly struct GetResponse<T>
{
    [JsonPropertyName("data")]
    public required T[] Items { get; init; } = Array.Empty<T>();

    public GetResponse()
    {
    }
}
