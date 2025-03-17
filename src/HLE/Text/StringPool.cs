using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Text;

public sealed partial class StringPool : IEquatable<StringPool>
{
    public static StringPool Shared { get; } = new();

    private Buckets _buckets;

    /// <summary>
    /// The amount of buckets in the pool.
    /// </summary>
    private const int DefaultPoolCapacity = 4096;

    /// <summary>
    /// The amount of strings per bucket.
    /// </summary>
    private const int DefaultBucketCapacity = 32;

    /// <summary>
    /// Constructor for a <see cref="StringPool"/>.
    /// </summary>
    public StringPool()
    {
        Span<Bucket> buckets = GetBuckets();
        for (int i = 0; i < buckets.Length; i++)
        {
            buckets[i] = new();
        }
    }

    public void Clear()
    {
        Span<Bucket> buckets = GetBuckets();
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

        ref Bucket bucket = ref GetBucket(str);
        return bucket.GetOrAdd(str);
    }

    [Pure]
    public string GetOrAdd(ref PooledInterpolatedStringHandler span)
    {
        string str = GetOrAdd(span.Text);
        span.Dispose();
        return str;
    }

    [Pure]
    public string GetOrAdd(ReadOnlySpan<char> span)
    {
        switch (span.Length)
        {
            case 0:
                return string.Empty;
            case 1:
                return SingleCharStringPool.GetOrAdd(span[0]);
            default:
                ref Bucket bucket = ref GetBucket(span);
                return bucket.GetOrAdd(span);
        }
    }

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
        if (!MemoryHelpers.UseStackalloc<char>(maxCharCount))
        {
            char[] rentedCharBuffer = ArrayPool<char>.Shared.Rent(maxCharCount);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer.AsSpan());
            string result = GetOrAdd(rentedCharBuffer.AsSpanUnsafe(..charsWritten));
            ArrayPool<char>.Shared.Return(rentedCharBuffer);
            return result;
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

        ref Bucket bucket = ref GetBucket(value);
        if (!bucket.Contains(value))
        {
            bucket.Add(value);
        }
    }

    public bool TryGet(ref PooledInterpolatedStringHandler span, [MaybeNullWhen(false)] out string value)
    {
        bool success = TryGet(span.Text, out value);
        span.Dispose();
        return success;
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
                ref Bucket bucket = ref GetBucket(span);
                return bucket.TryGet(span, out value);
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
        if (!MemoryHelpers.UseStackalloc<char>(maxCharCount))
        {
            char[] rentedCharBuffer = ArrayPool<char>.Shared.Rent(maxCharCount);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer.AsSpan());
            bool result = TryGet(rentedCharBuffer.AsSpanUnsafe(..charsWritten), out value);
            ArrayPool<char>.Shared.Return(rentedCharBuffer);
            return result;
        }

        Span<char> charBuffer = stackalloc char[maxCharCount];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        return TryGet(charBuffer[..charsWritten], out value);
    }

    [Pure]
    public bool Contains(string str) => Contains(str.AsSpan());

    [Pure]
    public bool Contains(ref PooledInterpolatedStringHandler span)
    {
        bool contains = Contains(span.Text);
        span.Dispose();
        return contains;
    }

    [Pure]
    public bool Contains(ReadOnlySpan<char> span)
    {
        switch (span.Length)
        {
            case 0:
                return true;
            case 1:
                return SingleCharStringPool.Contains(span[0]);
            default:
                ref Bucket bucket = ref GetBucket(span);
                return bucket.Contains(span);
        }
    }

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
        if (!MemoryHelpers.UseStackalloc<char>(maxCharCount))
        {
            char[] rentedCharBuffer = ArrayPool<char>.Shared.Rent(maxCharCount);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer.AsSpan());
            bool result = Contains(rentedCharBuffer.AsSpanUnsafe(..charsWritten));
            ArrayPool<char>.Shared.Return(rentedCharBuffer);
            return result;
        }

        Span<char> charBuffer = stackalloc char[maxCharCount];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        return Contains(charBuffer[..charsWritten]);
    }

    private Span<Bucket> GetBuckets() => InlineArrayHelpers.AsSpan<Buckets, Bucket>(ref _buckets);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Bucket GetBucket(ReadOnlySpan<char> str)
    {
        uint hash = SimpleStringHasher.Hash(str);
        uint index = hash % DefaultPoolCapacity;
        return ref Unsafe.Add(ref InlineArrayHelpers.GetReference<Buckets, Bucket>(ref _buckets), index);
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] StringPool? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(StringPool? left, StringPool? right) => Equals(left, right);

    public static bool operator !=(StringPool? left, StringPool? right) => !(left == right);
}
