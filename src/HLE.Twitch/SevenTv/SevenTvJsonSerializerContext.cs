using System.Text.Json.Serialization;
using HLE.Twitch.SevenTv.Models.Responses;

namespace HLE.Twitch.SevenTv;

[JsonSerializable(typeof(GetGlobalEmotesResponse))]
[JsonSerializable(typeof(GetChannelEmotesResponse))]
public sealed partial class SevenTvJsonSerializerContext : JsonSerializerContext;
