using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using HLE.Memory;

namespace HLE.Strings;

public sealed partial class StringPool : IEquatable<StringPool>, IEnumerable<string>
{
    public int BucketCount => _buckets.Length;

    public int BucketCapacity => _buckets[0]._strings.Length;

    public int Capacity => BucketCount * BucketCapacity;

    private readonly Bucket[] _buckets;

    public static StringPool Shared { get; } = [];

    private const int DefaultPoolCapacity = 4096;
    private const int DefaultBucketCapacity = 32;

    /// <summary>
    /// Constructor for a <see cref="StringPool"/>.
    /// </summary>
    /// <param name="poolCapacity">The amount of buckets in the pool.</param>
    /// <param name="bucketCapacity">The amount of strings per bucket in the pool.</param>
    public StringPool(int poolCapacity = DefaultPoolCapacity, int bucketCapacity = DefaultBucketCapacity)
    {
        Bucket[] buckets = new Bucket[poolCapacity];
        for (int i = 0; i < poolCapacity; i++)
        {
            buckets[i] = new(bucketCapacity);
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

    [Pure]
    public string GetOrAdd(string str)
    {
        switch (str.Length)
        {
            case 0:
                return string.Empty;
            case 1:
                SingleCharStringPool.Add(str);
                return str;
        }

        return GetBucket(str).GetOrAdd(str);
    }

    [Pure]
    public string GetOrAdd(ReadOnlySpan<char> span) =>
        span.Length switch
        {
            0 => string.Empty,
            1 => SingleCharStringPool.GetOrAdd(span[0]),
            _ => GetBucket(span).GetOrAdd(span)
        };

    [Pure]
    [SkipLocalsInit]
    public string GetOrAdd(ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        int charsWritten;
        int maxCharCount = encoding.GetMaxCharCount(bytes.Length);
        if (!MemoryHelpers.UseStackAlloc<char>(maxCharCount))
        {
            using RentedArray<char> rentedCharBuffer = ArrayPool<char>.Shared.RentAsRentedArray(maxCharCount);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer.AsSpan());
            return GetOrAdd(rentedCharBuffer[..charsWritten]);
        }

        Span<char> charBuffer = stackalloc char[maxCharCount];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        return GetOrAdd(charBuffer[..charsWritten]);
    }

    public void Add(string value)
    {
        switch (value.Length)
        {
            case 0:
                return;
            case 1:
                SingleCharStringPool.Add(value);
                break;
        }

        Bucket bucket = GetBucket(value);
        if (!bucket.Contains(value))
        {
            bucket.Add(value);
        }
    }

    public bool TryGet(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
    {
        switch (span.Length)
        {
            case 0:
                value = string.Empty;
                return true;
            case 1:
                return SingleCharStringPool.TryGet(span[0], out value);
            default:
                return GetBucket(span).TryGet(span, out value);
        }
    }

    [SkipLocalsInit]
    public bool TryGet(ReadOnlySpan<byte> bytes, Encoding encoding, [MaybeNullWhen(false)] out string value)
    {
        if (bytes.Length == 0)
        {
            value = string.Empty;
            return true;
        }

        int charsWritten;
        int maxCharCount = encoding.GetMaxCharCount(bytes.Length);
        if (!MemoryHelpers.UseStackAlloc<char>(maxCharCount))
        {
            using RentedArray<char> rentedCharBuffer = ArrayPool<char>.Shared.RentAsRentedArray(maxCharCount);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer.AsSpan());
            return TryGet(rentedCharBuffer[..charsWritten], out value);
        }

        Span<char> charBuffer = stackalloc char[maxCharCount];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        return TryGet(charBuffer[..charsWritten], out value);
    }

    [Pure]
    public bool Contains(string str) => Contains(str.AsSpan());

    [Pure]
    public bool Contains(ReadOnlySpan<char> span) => span.Length == 0 || GetBucket(span).Contains(span);

    [Pure]
    [SkipLocalsInit]
    public bool Contains(ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        if (bytes.Length == 0)
        {
            return true;
        }

        int charsWritten;
        int maxCharCount = encoding.GetMaxCharCount(bytes.Length);
        if (!MemoryHelpers.UseStackAlloc<char>(maxCharCount))
        {
            using RentedArray<char> rentedCharBuffer = ArrayPool<char>.Shared.RentAsRentedArray(maxCharCount);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer.AsSpan());
            return Contains(rentedCharBuffer[..charsWritten]);
        }

        Span<char> charBuffer = stackalloc char[maxCharCount];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        return Contains(charBuffer[..charsWritten]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Bucket GetBucket(ReadOnlySpan<char> span)
    {
        ref Bucket bucketReference = ref MemoryMarshal.GetArrayDataReference(_buckets);
        uint hash = SimpleStringHasher.Hash(span);
        int index = (int)(hash % (uint)_buckets.Length);
        return Unsafe.Add(ref bucketReference, index);
    }

    public IEnumerator<string> GetEnumerator()
    {
        foreach (Bucket bucket in _buckets)
        {
            foreach (string str in bucket)
            {
                yield return str;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] StringPool? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(StringPool? left, StringPool? right) => Equals(left, right);

    public static bool operator !=(StringPool? left, StringPool? right) => !(left == right);
}
