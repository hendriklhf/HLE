using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

public sealed class StringPool : IEquatable<StringPool>
{
    private readonly ConcurrentDictionary<int, Bucket> _buckets = new();

    public static StringPool Shared { get; } = new();

    public string GetOrAdd(ReadOnlySpan<char> span)
    {
        return span.Length == 0 ? string.Empty : GetOrAddBucket(span.Length).GetOrAdd(span);
    }

    public string GetOrAdd(ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        int charsWritten;
        Bucket bucket;
        if (!MemoryHelper.UseStackAlloc<char>(bytes.Length))
        {
            using RentedArray<char> rentedCharBuffer = new(bytes.Length);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer);
            bucket = GetOrAddBucket(charsWritten);
            return bucket.GetOrAdd(rentedCharBuffer[..charsWritten]);
        }

        Span<char> charBuffer = stackalloc char[bytes.Length];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        bucket = GetOrAddBucket(charsWritten);
        return bucket.GetOrAdd(charBuffer[..charsWritten]);
    }

    public void Add(string value)
    {
        if (value.Length == 0)
        {
            return;
        }

        GetOrAddBucket(value.Length).Add(value);
    }

    public bool TryGet(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
    {
        if (span.Length == 0)
        {
            value = string.Empty;
            return true;
        }

        value = null;
        return TryGetBucket(span.Length, out Bucket bucket) && bucket.TryGet(span, out value);
    }

    public bool TryGet(ReadOnlySpan<byte> bytes, Encoding encoding, [MaybeNullWhen(false)] out string value)
    {
        if (bytes.Length == 0)
        {
            value = string.Empty;
            return true;
        }

        value = null;
        Bucket bucket;
        int charsWritten;
        if (!MemoryHelper.UseStackAlloc<char>(bytes.Length))
        {
            using RentedArray<char> rentedCharBuffer = new(bytes.Length);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer);
            ReadOnlySpan<char> chars = rentedCharBuffer[..charsWritten];
            return TryGetBucket(chars.Length, out bucket) && bucket.TryGet(chars, out value);
        }

        Span<char> charBuffer = stackalloc char[bytes.Length];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        charBuffer = charBuffer[..charsWritten];
        return TryGetBucket(charBuffer.Length, out bucket) && bucket.TryGet(charBuffer, out value);
    }

    [Pure]
    public bool Contains(ReadOnlySpan<char> span)
    {
        if (span.Length == 0)
        {
            return false;
        }

        return TryGetBucket(span.Length, out Bucket bucket) && bucket.Contains(span);
    }

    [Pure]
    public bool Contains(ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        if (bytes.Length == 0)
        {
            return false;
        }

        Bucket bucket;
        int charsWritten;
        if (!MemoryHelper.UseStackAlloc<char>(bytes.Length))
        {
            using RentedArray<char> rentedCharBuffer = new(bytes.Length);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer);
            ReadOnlySpan<char> chars = rentedCharBuffer[..charsWritten];
            return TryGetBucket(chars.Length, out bucket) && bucket.Contains(chars);
        }

        Span<char> charBuffer = stackalloc char[bytes.Length];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        charBuffer = charBuffer[..charsWritten];
        return TryGetBucket(charsWritten, out bucket) && bucket.Contains(charBuffer);
    }

    public void Reset()
    {
        _buckets.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Bucket GetOrAddBucket(int stringLength)
    {
        if (TryGetBucket(stringLength, out Bucket bucket))
        {
            return bucket;
        }

        bucket = new();
        _buckets.AddOrSet(stringLength, bucket);
        return bucket;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryGetBucket(int stringLength, out Bucket bucket)
    {
        return _buckets.TryGetValue(stringLength, out bucket);
    }

    [Pure]
    public bool Equals(StringPool? other)
    {
        return ReferenceEquals(this, other);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is StringPool other && Equals(other);
    }

    [Pure]
    public override int GetHashCode()
    {
        return MemoryHelper.GetRawDataPointer(this).GetHashCode();
    }

    public static bool operator ==(StringPool? left, StringPool? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(StringPool? left, StringPool? right)
    {
        return !(left == right);
    }

    private readonly struct Bucket
    {
        private readonly ConcurrentDictionary<int, string> _strings = new();

        public Bucket()
        {
        }

        public string GetOrAdd(ReadOnlySpan<char> span)
        {
            int spanHash = string.GetHashCode(span);
            if (_strings.TryGetValue(spanHash, out string? str))
            {
                return str;
            }

            str = new(span);
            _strings.AddOrSet(spanHash, str);
            return str;
        }

        public void Add(string value)
        {
            int spanHash = string.GetHashCode(value);
            _strings.AddOrSet(spanHash, value);
        }

        public bool TryGet(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
        {
            int spanHash = string.GetHashCode(span);
            return _strings.TryGetValue(spanHash, out value);
        }

        public bool Contains(ReadOnlySpan<char> span)
        {
            return _strings.ContainsKey(string.GetHashCode(span));
        }
    }
}
