using System.Text.Json.Serialization;
using HLE.Twitch.Bttv.Models.Responses;

namespace HLE.Twitch.Bttv;

[JsonSerializable(typeof(GetUserResponse))]
public sealed partial class BttvJsonSerializerContext : JsonSerializerContext;
