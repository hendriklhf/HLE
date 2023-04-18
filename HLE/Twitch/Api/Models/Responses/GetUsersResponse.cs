using System;
using System.Text.Json.Serialization;

namespace HLE.Twitch.Api.Models.Responses;

internal readonly struct GetUsersResponse
{
    [JsonPropertyName("data")]
    public required User[] Users { get; init; } = Array.Empty<User>();

    public GetUsersResponse()
    {
    }
}
