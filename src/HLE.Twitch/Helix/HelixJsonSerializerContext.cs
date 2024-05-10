using System.Text.Json.Serialization;
using HLE.Twitch.Helix.Models;
using HLE.Twitch.Helix.Models.Responses;
using Stream = HLE.Twitch.Helix.Models.Stream;

namespace HLE.Twitch.Helix;

[JsonSerializable(typeof(AccessToken))]
[JsonSerializable(typeof(HelixResponse<User>))]
[JsonSerializable(typeof(HelixResponse<Emote>))]
[JsonSerializable(typeof(HelixResponse<Stream>))]
[JsonSerializable(typeof(HelixResponse<ChannelEmote>))]
public sealed partial class HelixJsonSerializerContext : JsonSerializerContext;
