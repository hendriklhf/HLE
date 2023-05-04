using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

public sealed class RegexPool : IEquatable<RegexPool>
{
    private readonly Dictionary<int, Bucket> _buckets = new();

    public static RegexPool Shared { get; } = new();

    public Regex GetOrAdd([StringSyntax(StringSyntaxAttribute.Regex)] string pattern, RegexOptions options = RegexOptions.None, TimeSpan timeout = default)
    {
        if (timeout == default)
        {
            timeout = Regex.InfiniteMatchTimeout;
        }

        return GetOrAddBucket(pattern.Length).GetOrAdd(pattern, options, timeout);
    }

    public void Add(Regex regex)
    {
        GetOrAddBucket(regex.ToString().Length).Add(regex);
    }

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, [MaybeNullWhen(false)] out Regex regex)
    {
        regex = null;
        return TryGetBucket(pattern.Length, out Bucket bucket) && bucket.TryGet(pattern, RegexOptions.None, Regex.InfiniteMatchTimeout, out regex);
    }

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, RegexOptions options, [MaybeNullWhen(false)] out Regex regex)
    {
        regex = null;
        return TryGetBucket(pattern.Length, out Bucket bucket) && bucket.TryGet(pattern, options, Regex.InfiniteMatchTimeout, out regex);
    }

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout, [MaybeNullWhen(false)] out Regex regex)
    {
        regex = null;
        return TryGetBucket(pattern.Length, out Bucket bucket) && bucket.TryGet(pattern, options, timeout, out regex);
    }

    public void Reset()
    {
        _buckets.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Bucket GetOrAddBucket(int patternLength)
    {
        if (TryGetBucket(patternLength, out Bucket bucket))
        {
            return bucket;
        }

        bucket = new();
        lock (_buckets)
        {
            _buckets.AddOrSet(patternLength, bucket);
        }

        return bucket;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryGetBucket(int patternLength, out Bucket bucket)
    {
        return _buckets.TryGetValue(patternLength, out bucket);
    }

    [Pure]
    public bool Equals(RegexPool? other)
    {
        return ReferenceEquals(this, other);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is RegexPool other && Equals(other);
    }

    [Pure]
    public override int GetHashCode()
    {
        return MemoryHelper.GetRawDataPointer(this).GetHashCode();
    }

    public static bool operator ==(RegexPool? left, RegexPool? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RegexPool? left, RegexPool? right)
    {
        return !(left == right);
    }

    private readonly struct Bucket
    {
        private readonly Dictionary<int, Regex> _regexes = new();

        public Bucket()
        {
        }

        public Regex GetOrAdd(string pattern, RegexOptions options, TimeSpan timeout)
        {
            int regexHash = BuildRegexHash(pattern, options, timeout);
            // ReSharper disable once InconsistentlySynchronizedField
            if (_regexes.TryGetValue(regexHash, out Regex? regex))
            {
                return regex;
            }

            regex = new(pattern, options, timeout);
            lock (_regexes)
            {
                _regexes.AddOrSet(regexHash, regex);
            }

            return regex;
        }

        public void Add(Regex regex)
        {
            int regexHash = BuildRegexHash(regex);
            lock (_regexes)
            {
                _regexes.AddOrSet(regexHash, regex);
            }
        }

        public bool TryGet(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout, [MaybeNullWhen(false)] out Regex regex)
        {
            int regexHash = BuildRegexHash(pattern, options, timeout);
            // ReSharper disable once InconsistentlySynchronizedField
            return _regexes.TryGetValue(regexHash, out regex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int BuildRegexHash(Regex regex)
        {
            return BuildRegexHash(regex.ToString(), regex.Options, regex.MatchTimeout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int BuildRegexHash(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout)
        {
            int patternHash = string.GetHashCode(pattern);
            return HashCode.Combine(patternHash, (int)options, timeout);
        }
    }
}
