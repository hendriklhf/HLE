using System;
using System.Runtime.CompilerServices;

namespace HLE.Twitch.SevenTv;

public sealed class CacheOptions : IEquatable<CacheOptions>
{
    public TimeSpan GlobalEmotesCacheDuration { get; set; } = TimeSpan.FromDays(1);

    public TimeSpan ChannelEmotesCacheDuration { get; set; } = TimeSpan.FromHours(1);

    public bool Equals(CacheOptions? other) => ReferenceEquals(this, other);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(CacheOptions? left, CacheOptions? right) => Equals(left, right);

    public static bool operator !=(CacheOptions? left, CacheOptions? right) => !(left == right);
}
