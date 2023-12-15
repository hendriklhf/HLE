using System.Text.Json.Serialization;
using HLE.Twitch.Helix.Models;
using HLE.Twitch.Helix.Models.Responses;
using Stream = HLE.Twitch.Helix.Models.Stream;

namespace HLE.Twitch.Helix;

[JsonSerializable(typeof(AccessToken))]
[JsonSerializable(typeof(GetResponse<User>))]
[JsonSerializable(typeof(GetResponse<Emote>))]
[JsonSerializable(typeof(GetResponse<Stream>))]
[JsonSerializable(typeof(GetResponse<ChannelEmote>))]
public sealed partial class HelixJsonSerializerContext : JsonSerializerContext;
