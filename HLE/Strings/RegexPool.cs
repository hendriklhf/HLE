using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using HLE.Collections;

namespace HLE.Strings;

public sealed class RegexPool : IEquatable<RegexPool>, IEnumerable<Regex>
{
    private readonly Bucket[] _buckets = new Bucket[_defaultPoolCapacity];

    public static RegexPool Shared { get; set; } = new();

    private const int _defaultPoolCapacity = 4096;
    private const int _defaultBucketCapacity = 32;

    public RegexPool()
    {
        Span<Bucket> buckets = _buckets;
        for (int i = 0; i < buckets.Length; i++)
        {
            buckets[i] = new();
        }
    }

    public void Clear()
    {
        ReadOnlySpan<Bucket> buckets = _buckets;
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
    public bool Contains(Regex regex)
    {
        return Contains(regex.ToString(), regex.Options, regex.MatchTimeout);
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
    private int GetBucketIndex(Regex regex)
    {
        return GetBucketIndex(regex.ToString(), regex.Options, regex.MatchTimeout);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBucketIndex(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout)
    {
        int patternHash = SimpleStringHasher.Hash(pattern);
        int hash = HashCode.Combine(patternHash, (int)options, timeout);
        int index = (int)((uint)hash % _buckets.Length);
        Debug.Assert(index >= 0 && index < _buckets.Length);
        return index;
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
        return RuntimeHelpers.GetHashCode(this);
    }

    public static bool operator ==(RegexPool? left, RegexPool? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RegexPool? left, RegexPool? right)
    {
        return !(left == right);
    }

    public IEnumerator<Regex> GetEnumerator()
    {
        foreach (Bucket bucket in _buckets)
        {
            foreach (Regex regex in bucket)
            {
                yield return regex;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private readonly struct Bucket : IEnumerable<Regex>
    {
        private readonly Regex?[] _regexes = new Regex[_defaultBucketCapacity];
        private readonly SemaphoreSlim _regexesLock = new(1);

        public Bucket()
        {
        }

        public void Clear()
        {
            _regexes.AsSpan().Clear();
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
            ref Regex? regexesReference = ref MemoryMarshal.GetArrayDataReference(_regexes);
            int regexesLength = _regexes.Length;
            for (int i = 0; i < regexesLength; i++)
            {
                Regex? current = Unsafe.Add(ref regexesReference, i);
                if (current is null)
                {
                    // a null reference can only be followed by more null references,
                    // so we can exit early because the regex can definitely not be found
                    regex = null;
                    return false;
                }

                if (options != current.Options || timeout != current.MatchTimeout || !pattern.SequenceEqual(current.ToString()))
                {
                    continue;
                }

                if (i > 3)
                {
                    MoveRegexByFourIndices(i);
                }

                regex = current;
                return true;
            }

            regex = null;
            return false;
        }

        /// <summary>
        /// Moves a matching item by four places, so that it can be found faster next time.
        /// </summary>
        private void MoveRegexByFourIndices(int indexOfMatchingRegex)
        {
            _regexesLock.Wait();
            try
            {
                _regexes.MoveItem(indexOfMatchingRegex, indexOfMatchingRegex - 4);
            }
            finally
            {
                _regexesLock.Release();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Regex regex)
        {
            return TryGet(regex.ToString(), regex.Options, regex.MatchTimeout, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout)
        {
            return TryGet(pattern, options, timeout, out _);
        }

        public IEnumerator<Regex> GetEnumerator()
        {
            foreach (Regex? regex in _regexes)
            {
                if (regex is not null)
                {
                    yield return regex;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
