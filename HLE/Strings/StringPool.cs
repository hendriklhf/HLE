using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Text;
using HLE.Memory;

namespace HLE.Strings;

public sealed class StringPool : IEquatable<StringPool>
{
    private readonly Dictionary<int, Bucket> _buckets = new();

    public static StringPool Shared { get; } = new();

    public string GetOrAdd(ReadOnlySpan<char> span)
    {
        if (span.Length == 0)
        {
            return string.Empty;
        }

        Bucket bucket = GetOrAddBucket(span.Length);
        return bucket.GetOrAdd(span);
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

        if (_buckets.TryGetValue(span.Length, out Bucket bucket))
        {
            return bucket.TryGet(span, out value);
        }

        value = null;
        return false;
    }

    public bool TryGet(ReadOnlySpan<byte> bytes, Encoding encoding, [MaybeNullWhen(false)] out string value)
    {
        if (bytes.Length == 0)
        {
            value = string.Empty;
            return true;
        }

        Bucket bucket;
        int charsWritten;
        if (!MemoryHelper.UseStackAlloc<char>(bytes.Length))
        {
            using RentedArray<char> rentedCharBuffer = new(bytes.Length);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer);
            ReadOnlySpan<char> chars = rentedCharBuffer[..charsWritten];
            if (_buckets.TryGetValue(chars.Length, out bucket))
            {
                return bucket.TryGet(chars, out value);
            }
        }

        Span<char> charBuffer = stackalloc char[bytes.Length];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        charBuffer = charBuffer[..charsWritten];
        if (_buckets.TryGetValue(charBuffer.Length, out bucket))
        {
            return bucket.TryGet(charBuffer, out value);
        }

        value = null;
        return false;
    }

    public void Reset()
    {
        _buckets.Clear();
    }

    private Bucket GetOrAddBucket(int stringLength)
    {
        ref Bucket bucket = ref CollectionsMarshal.GetValueRefOrAddDefault(_buckets, stringLength, out bool bucketExists);
        if (!bucketExists)
        {
            bucket = new();
        }

        return bucket;
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
        return !Equals(left, right);
    }

    private readonly struct Bucket
    {
        private readonly Dictionary<int, string> _strings = new();

        public Bucket()
        {
        }

        public string GetOrAdd(ReadOnlySpan<char> span)
        {
            int spanHash = string.GetHashCode(span);
            ref string? str = ref CollectionsMarshal.GetValueRefOrAddDefault(_strings, spanHash, out bool stringExists);
            if (stringExists)
            {
                Debug.Assert(str is not null);
                return str;
            }

            str = new(span);
            return str;
        }

        public void Add(string value)
        {
            int spanHash = string.GetHashCode(value);
            _strings.TryAdd(spanHash, value);
        }

        public bool TryGet(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
        {
            int spanHash = string.GetHashCode(span);
            return _strings.TryGetValue(spanHash, out value);
        }
    }
}
