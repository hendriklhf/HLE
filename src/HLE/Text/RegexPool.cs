using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HLE.Memory;

namespace HLE.Text;

public sealed partial class RegexPool : IEquatable<RegexPool>
{
    public static RegexPool Shared { get; } = new();

    private Buckets _buckets;

    private const int DefaultPoolCapacity = 4096;
    private const int DefaultBucketCapacity = 32;

    public RegexPool()
    {
        Span<Bucket> buckets = InlineArrayHelpers.AsSpan<Buckets, Bucket>(ref _buckets, Buckets.Length);
        for (int i = 0; i < buckets.Length; i++)
        {
            buckets[i] = new();
        }
    }

    public void Clear()
    {
        Span<Bucket> buckets = InlineArrayHelpers.AsSpan<Buckets, Bucket>(ref _buckets, Buckets.Length);
        for (int i = 0; i < buckets.Length; i++)
        {
            buckets[i].Clear();
        }
    }

    public Regex GetOrAdd(Regex regex)
    {
        ref Bucket bucket = ref GetBucket(regex);
        if (!bucket.Contains(regex))
        {
            bucket.Add(regex);
        }

        return regex;
    }

    public Regex GetOrAdd([StringSyntax(StringSyntaxAttribute.Regex)] string pattern, RegexOptions options = RegexOptions.None, TimeSpan timeout = default)
    {
        if (timeout == default)
        {
            timeout = Regex.InfiniteMatchTimeout;
        }

        return GetBucket(pattern, options, timeout).GetOrAdd(pattern, options, timeout);
    }

    public Regex GetOrAdd([StringSyntax(StringSyntaxAttribute.Regex)] ref PooledInterpolatedStringHandler pattern, RegexOptions options = RegexOptions.None, TimeSpan timeout = default)
    {
        Regex regex = GetOrAdd(pattern.Text, options, timeout);
        pattern.Dispose();
        return regex;
    }

    public Regex GetOrAdd([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, RegexOptions options = RegexOptions.None, TimeSpan timeout = default)
    {
        if (timeout == default)
        {
            timeout = Regex.InfiniteMatchTimeout;
        }

        return GetBucket(pattern, options, timeout).GetOrAdd(pattern, options, timeout);
    }

    public void Add(string pattern, RegexOptions options = RegexOptions.None, TimeSpan timeout = default)
    {
        if (timeout == default)
        {
            timeout = Regex.InfiniteMatchTimeout;
        }

        Regex regex = new(pattern, options, timeout);
        Add(regex);
    }

    public void Add(Regex regex)
    {
        ref Bucket bucket = ref GetBucket(regex);
        bucket.Add(regex);
    }

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, [MaybeNullWhen(false)] out Regex regex)
        => TryGet(pattern, RegexOptions.None, Regex.InfiniteMatchTimeout, out regex);

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, RegexOptions options, [MaybeNullWhen(false)] out Regex regex)
        => TryGet(pattern, options, Regex.InfiniteMatchTimeout, out regex);

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout, [MaybeNullWhen(false)] out Regex regex)
        => GetBucket(pattern, options, timeout).TryGet(pattern, options, timeout, out regex);

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ref PooledInterpolatedStringHandler pattern, [MaybeNullWhen(false)] out Regex regex)
    {
        bool success = TryGet(pattern.Text, RegexOptions.None, Regex.InfiniteMatchTimeout, out regex);
        pattern.Dispose();
        return success;
    }

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ref PooledInterpolatedStringHandler pattern, RegexOptions options, [MaybeNullWhen(false)] out Regex regex)
    {
        bool success = TryGet(pattern.Text, options, Regex.InfiniteMatchTimeout, out regex);
        pattern.Dispose();
        return success;
    }

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ref PooledInterpolatedStringHandler pattern, RegexOptions options, TimeSpan timeout, [MaybeNullWhen(false)] out Regex regex)
    {
        bool success = GetBucket(pattern.Text, options, timeout).TryGet(pattern.Text, options, timeout, out regex);
        pattern.Dispose();
        return success;
    }

    [Pure]
    public bool Contains(Regex regex) => Contains(regex.ToString(), regex.Options, regex.MatchTimeout);

    [Pure]
    public bool Contains([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, RegexOptions options = RegexOptions.None, TimeSpan timeout = default)
    {
        if (timeout == default)
        {
            timeout = Regex.InfiniteMatchTimeout;
        }

        return GetBucket(pattern, options, timeout).Contains(pattern, options, timeout);
    }

    [Pure]
    public bool Contains([StringSyntax(StringSyntaxAttribute.Regex)] ref PooledInterpolatedStringHandler pattern, RegexOptions options = RegexOptions.None, TimeSpan timeout = default)
    {
        if (timeout == default)
        {
            timeout = Regex.InfiniteMatchTimeout;
        }

        bool contains = GetBucket(pattern.Text, options, timeout).Contains(pattern.Text, options, timeout);
        pattern.Dispose();
        return contains;
    }

    private ref Bucket GetBucket(Regex regex) => ref GetBucket(regex.ToString(), regex.Options, regex.MatchTimeout);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Bucket GetBucket(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout)
    {
        uint hash = SimpleStringHasher.Hash(pattern);
        hash = (uint)HashCode.Combine(hash, (int)options, timeout);
        uint index = hash % DefaultPoolCapacity;
        return ref Unsafe.Add(ref InlineArrayHelpers.GetReference<Buckets, Bucket>(ref _buckets), index);
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] RegexPool? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(RegexPool? left, RegexPool? right) => Equals(left, right);

    public static bool operator !=(RegexPool? left, RegexPool? right) => !(left == right);
}
