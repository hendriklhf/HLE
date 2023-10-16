﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

public sealed class StringPool : IEquatable<StringPool>, IEnumerable<string>, IDisposable
{
    public int Capacity => _buckets.Length;

    public int BucketCapacity => _buckets[0]._strings.Length;

    internal readonly Bucket[] _buckets;

    public static StringPool Shared { get; } = new();

    private const int _defaultPoolCapacity = 4096;
    private const int _defaultBucketCapacity = 32;

    /// <summary>
    /// Constructor for a <see cref="StringPool"/>.
    /// </summary>
    /// <param name="poolCapacity">The amount of buckets in the pool.</param>
    /// <param name="bucketCapacity">The amount of strings per bucket in the pool.</param>
    public StringPool(int poolCapacity = _defaultPoolCapacity, int bucketCapacity = _defaultBucketCapacity)
    {
        _buckets = new Bucket[poolCapacity];
        for (int i = 0; i < poolCapacity; i++)
        {
            _buckets[i] = new(bucketCapacity);
        }
    }

    public void Dispose()
    {
        Span<Bucket> buckets = _buckets;
        for (int i = 0; i < buckets.Length; i++)
        {
            ref Bucket bucket = ref buckets[i];
            bucket.Dispose();
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _buckets.Length; i++)
        {
            _buckets[i].Clear();
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

        Bucket bucket = GetBucket(str);
        if (!bucket.Contains(str))
        {
            bucket.Add(str);
        }

        return str;
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
        if (!MemoryHelper.UseStackAlloc<char>(maxCharCount))
        {
            using RentedArray<char> rentedCharBuffer = ArrayPool<char>.Shared.CreateRentedArray(maxCharCount);
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
        if (!MemoryHelper.UseStackAlloc<char>(maxCharCount))
        {
            using RentedArray<char> rentedCharBuffer = ArrayPool<char>.Shared.CreateRentedArray(maxCharCount);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer.AsSpan());
            return TryGet(rentedCharBuffer[..charsWritten], out value);
        }

        Span<char> charBuffer = stackalloc char[maxCharCount];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        return TryGet(charBuffer[..charsWritten], out value);
    }

    [Pure]
    public bool Contains(string str) => Contains((ReadOnlySpan<char>)str);

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
        if (!MemoryHelper.UseStackAlloc<char>(maxCharCount))
        {
            using RentedArray<char> rentedCharBuffer = ArrayPool<char>.Shared.CreateRentedArray(maxCharCount);
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
        int hash = SimpleStringHasher.Hash(span);
        int index = (int)((uint)hash % _buckets.Length);
        return _buckets[index];
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
    public bool Equals(StringPool? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(StringPool? left, StringPool? right) => Equals(left, right);

    public static bool operator !=(StringPool? left, StringPool? right) => !(left == right);

    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "it does implement IDisposable?!")]
    internal struct Bucket(int bucketCapacity = _defaultBucketCapacity)
        : IEnumerable<string>, IDisposable
    {
        internal readonly StringArray _strings = new(bucketCapacity);
        private SemaphoreSlim? _stringsLock = new(1);

        public void Dispose()
        {
            _stringsLock?.Dispose();
            _stringsLock = null;
        }

        public readonly void Clear()
        {
            ObjectDisposedException.ThrowIf(_stringsLock is null, typeof(StringPool));

            _stringsLock.Wait();
            try
            {
                _strings.Clear();
            }
            finally
            {
                _stringsLock.Release();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string GetOrAdd(ReadOnlySpan<char> span)
        {
            if (TryGet(span, out string? value))
            {
                return value;
            }

            value = new(span);
            Add(value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Add(string value)
        {
            ObjectDisposedException.ThrowIf(_stringsLock is null, typeof(StringPool));

            _stringsLock.Wait();
            try
            {
                _strings[^1] = value;
                _strings.MoveString(_strings.Length - 1, 0);
            }
            finally
            {
                _stringsLock.Release();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGet(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
        {
            int index = IndexOf(_strings, span);
            if (index < 0)
            {
                value = null;
                return false;
            }

            value = _strings[index];
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(ReadOnlySpan<char> span) => TryGet(span, out _);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(StringArray stringArray, ReadOnlySpan<char> span)
        {
            int arrayLength = stringArray.Length;
            ReadOnlySpan<char> stringChars = stringArray._stringChars;
            ref string stringsReference = ref MemoryMarshal.GetArrayDataReference(stringArray._strings);
            ref int lengthsReference = ref MemoryMarshal.GetArrayDataReference(stringArray._stringLengths);
            ref int startReference = ref MemoryMarshal.GetArrayDataReference(stringArray._stringStarts);
            for (int i = 0; i < arrayLength; i++)
            {
                int length = Unsafe.Add(ref lengthsReference, i);
                if (length == 0)
                {
                    return -1;
                }

                if (length != span.Length)
                {
                    continue;
                }

                ref char spanReference = ref MemoryMarshal.GetReference(span);
                ref char stringReference = ref MemoryMarshal.GetReference(Unsafe.Add(ref stringsReference, i).AsSpan());
                if (Unsafe.AreSame(ref spanReference, ref stringReference))
                {
                    return i;
                }

                int start = Unsafe.Add(ref startReference, i);
                ReadOnlySpan<char> bufferString = stringChars.SliceUnsafe(start, length);
                if (span.SequenceEqual(bufferString))
                {
                    return i;
                }
            }

            return -1;
        }

        public readonly IEnumerator<string> GetEnumerator()
        {
            foreach (string? str in _strings)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (str is not null)
                {
                    yield return str;
                }
            }
        }

        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
