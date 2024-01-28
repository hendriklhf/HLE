using System;
using System.Runtime.CompilerServices;

namespace HLE.Twitch.Ffz;

public sealed class CacheOptions : IEquatable<CacheOptions>
{
    public TimeSpan ChannelEmotesCacheDuration { get; set; } = TimeSpan.FromHours(1);

    public TimeSpan GlobalEmotesCacheDuration { get; set; } = TimeSpan.FromDays(1);

    public bool Equals(CacheOptions? other) => ReferenceEquals(this, other);

    public override bool Equals(object? obj) => obj is CacheOptions other && Equals(other);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(CacheOptions? left, CacheOptions? right) => Equals(left, right);

    public static bool operator !=(CacheOptions? left, CacheOptions? right) => !(left == right);
}
