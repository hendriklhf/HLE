using System;
using System.Diagnostics.Contracts;

namespace HLE.Twitch.Helix.Models.Cache;

public readonly struct CacheEntry<T> : IEquatable<CacheEntry<T>>
{
    public T? Value { get; } = default;

    internal readonly DateTime _timeOfRequest = DateTime.UtcNow;

    public static CacheEntry<T> Empty => new();

    public CacheEntry()
    {
        _timeOfRequest = default;
    }

    public CacheEntry(T value)
    {
        Value = value;
    }

    [Pure]
    public bool IsValid(TimeSpan cacheTime)
    {
        return _timeOfRequest + cacheTime > DateTime.UtcNow;
    }

    [Pure]
    public bool Equals(CacheEntry<T> other)
    {
        return ((Value is null && other.Value is null) || Value?.Equals(other.Value) == true) && _timeOfRequest == other._timeOfRequest;
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is CacheEntry<T> other && Equals(other);
    }

    [Pure]
    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    [Pure]
    public static bool operator ==(CacheEntry<T> left, CacheEntry<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CacheEntry<T> left, CacheEntry<T> right)
    {
        return !(left == right);
    }
}
