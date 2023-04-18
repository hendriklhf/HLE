using System;
using System.Text.Json.Serialization;

namespace HLE.Twitch.Api.Models.Responses;

internal readonly struct GetStreamsResponse
{
    [JsonPropertyName("data")]
    public required Stream[] Streams { get; init; } = Array.Empty<Stream>();

    public GetStreamsResponse()
    {
    }
}
