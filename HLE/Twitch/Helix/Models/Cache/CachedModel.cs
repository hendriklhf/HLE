using System;
using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;

namespace HLE.Twitch.Helix.Models.Cache;

public abstract class CachedModel
{
    [JsonIgnore]
    internal readonly DateTime _timeOfRequest = DateTime.UtcNow;

    [Pure]
    public bool IsValid(TimeSpan cacheTime)
    {
        return _timeOfRequest + cacheTime > DateTime.UtcNow;
    }
}
