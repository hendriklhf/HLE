using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace HLE.Strings;

public sealed partial class RegexPool : IEquatable<RegexPool>
{
    private readonly Bucket[] _buckets;

    public static RegexPool Shared { get; } = new();

    private const int DefaultPoolCapacity = 4096;
    private const int DefaultBucketCapacity = 32;

    public RegexPool()
    {
        Bucket[] buckets = GC.AllocateArray<Bucket>(DefaultPoolCapacity, true);
        for (int i = 0; i < buckets.Length; i++)
        {
            buckets[i] = new();
        }

        _buckets = buckets;
    }

    public void Clear()
    {
        Span<Bucket> buckets = _buckets;
        for (int i = 0; i < buckets.Length; i++)
        {
            buckets[i].Clear();
        }
    }

    public Regex GetOrAdd(Regex regex)
    {
        Bucket bucket = GetBucket(regex);
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

    public void Add(Regex regex) => GetBucket(regex).Add(regex);

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, [MaybeNullWhen(false)] out Regex regex)
        => TryGet(pattern, RegexOptions.None, Regex.InfiniteMatchTimeout, out regex);

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, RegexOptions options, [MaybeNullWhen(false)] out Regex regex)
        => TryGet(pattern, options, Regex.InfiniteMatchTimeout, out regex);

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout, [MaybeNullWhen(false)] out Regex regex)
        => GetBucket(pattern, options, timeout).TryGet(pattern, options, timeout, out regex);

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

    private ref Bucket GetBucket(Regex regex) => ref GetBucket(regex.ToString(), regex.Options, regex.MatchTimeout);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Bucket GetBucket(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout)
    {
        uint patternHash = SimpleStringHasher.Hash(pattern);
        int hash = HashCode.Combine(patternHash, (int)options, timeout);
        Span<Bucket> buckets = _buckets;
        int index = (int)((uint)hash % (uint)buckets.Length);
        return ref buckets[index];
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
