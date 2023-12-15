using System.Text.Json.Serialization;
using HLE.Twitch.Ffz.Models.Responses;

namespace HLE.Twitch.Ffz;

[JsonSerializable(typeof(GetRoomResponse))]
[JsonSerializable(typeof(GetGlobalEmotesResponse))]
public sealed partial class FfzJsonSerializerContext : JsonSerializerContext;
