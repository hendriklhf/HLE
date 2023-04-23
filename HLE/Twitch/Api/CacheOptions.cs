using System;

namespace HLE.Twitch.Api;

public sealed class CacheOptions
{
    public TimeSpan UserCacheTime { get; set; } = TimeSpan.FromDays(1);

    public TimeSpan StreamCacheTime { get; set; } = TimeSpan.FromMinutes(10);

    public TimeSpan GlobalEmotesCacheTime { get; set; } = TimeSpan.FromDays(1);

    public TimeSpan ChannelEmotesCacheTime { get; set; } = TimeSpan.FromDays(1);
}
