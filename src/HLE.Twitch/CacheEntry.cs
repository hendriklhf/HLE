using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Twitch;

internal readonly struct CacheEntry<T> : IEquatable<CacheEntry<T>>
{
    public T? Value { get; }

    internal readonly DateTime _timeOfCreation;

    public static CacheEntry<T> Empty => new();

    public CacheEntry()
    {
        Value = default;
        _timeOfCreation = default;
    }

    public CacheEntry(T value)
    {
        Value = value;
        _timeOfCreation = DateTime.UtcNow;
    }

    [Pure]
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsValid(TimeSpan cacheTime) => this != default && _timeOfCreation + cacheTime > DateTime.UtcNow;

    [Pure]
    public bool Equals(CacheEntry<T> other) => Value?.Equals(other.Value) == true && _timeOfCreation == other._timeOfCreation;

    [Pure]
    public override bool Equals(object? obj) => obj is CacheEntry<T> other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Value, _timeOfCreation);

    [Pure]
    public static bool operator ==(CacheEntry<T> left, CacheEntry<T> right) => left.Equals(right);

    public static bool operator !=(CacheEntry<T> left, CacheEntry<T> right) => !(left == right);
}
