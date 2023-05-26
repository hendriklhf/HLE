using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

public sealed class RegexPool : IEquatable<RegexPool>, IDisposable
{
    private readonly Bucket[] _buckets = new Bucket[_defaultPoolCapacity];

    public static RegexPool Shared { get; set; } = new();

    private const int _defaultPoolCapacity = 4096;
    private const int _defaultBucketCapacity = 32;

    public RegexPool()
    {
        for (int i = 0; i < _defaultPoolCapacity; i++)
        {
            _buckets[i] = new();
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < _buckets.Length; i++)
        {
            _buckets[i].Dispose();
        }
    }

    public Regex GetOrAdd([StringSyntax(StringSyntaxAttribute.Regex)] string pattern, RegexOptions options = RegexOptions.None, TimeSpan timeout = default)
    {
        if (timeout == default)
        {
            timeout = Regex.InfiniteMatchTimeout;
        }

        return GetBucket(pattern, options, timeout).GetOrAdd(pattern, options, timeout);
    }

    public void Add(Regex regex)
    {
        GetBucket(regex).Add(regex);
    }

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, [MaybeNullWhen(false)] out Regex regex)
    {
        return TryGet(pattern, RegexOptions.None, Regex.InfiniteMatchTimeout, out regex);
    }

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, RegexOptions options, [MaybeNullWhen(false)] out Regex regex)
    {
        return TryGet(pattern, options, Regex.InfiniteMatchTimeout, out regex);
    }

    public bool TryGet([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout, [MaybeNullWhen(false)] out Regex regex)
    {
        return GetBucket(pattern, options, timeout).TryGet(pattern, options, timeout, out regex);
    }

    [Pure]
    public bool Contains([StringSyntax(StringSyntaxAttribute.Regex)] ReadOnlySpan<char> pattern, RegexOptions options = RegexOptions.None, TimeSpan timeout = default)
    {
        if (timeout == default)
        {
            timeout = Regex.InfiniteMatchTimeout;
        }

        return GetBucket(pattern, options, timeout).Contains(pattern, options, timeout);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Bucket GetBucket(Regex regex)
    {
        int index = GetBucketIndex(regex);
        return _buckets[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Bucket GetBucket(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout)
    {
        int index = GetBucketIndex(pattern, options, timeout);
        return _buckets[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetBucketIndex(Regex regex)
    {
        return GetBucketIndex(regex.ToString(), regex.Options, regex.MatchTimeout);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetBucketIndex(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout)
    {
        int patternHash = string.GetHashCode(pattern);
        int hash = HashCode.Combine(patternHash, (int)options, timeout);
        return (int)(Unsafe.As<int, uint>(ref hash) % _defaultPoolCapacity);
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

    private readonly struct Bucket : IDisposable
    {
        private readonly Regex?[] _regexes = new Regex[_defaultBucketCapacity];
        private readonly SemaphoreSlim _regexesLock = new(1);

        public Bucket()
        {
        }

        public void Dispose()
        {
            _regexesLock.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Regex GetOrAdd(string pattern, RegexOptions options, TimeSpan timeout)
        {
            if (TryGet(pattern, options, timeout, out Regex? regex))
            {
                return regex;
            }

            regex = new(pattern, options, timeout);
            Add(regex);
            return regex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Regex regex)
        {
            _regexesLock.Wait();
            try
            {
                Span<Regex?> regexes = _regexes;
                regexes[..^1].CopyTo(regexes[1..]);
                regexes[0] = regex;
            }
            finally
            {
                _regexesLock.Release();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout, [MaybeNullWhen(false)] out Regex regex)
        {
            Span<Regex?> regexes = _regexes;
            for (int i = 0; i < _defaultBucketCapacity; i++)
            {
                Regex? current = regexes[i];
                if (current is null || options != current.Options || timeout != current.MatchTimeout || !pattern.SequenceEqual(current.ToString()))
                {
                    continue;
                }

                if (i > 3)
                {
                    _regexesLock.Wait();
                    try
                    {
                        _regexes.MoveItem(i, i - 4);
                    }
                    finally
                    {
                        _regexesLock.Release();
                    }
                }

                regex = current;
                return true;
            }

            regex = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout)
        {
            return TryGet(pattern, options, timeout, out _);
        }
    }
}
