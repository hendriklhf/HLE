using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

public sealed class StringPool : IDisposable, IEquatable<StringPool>, IEnumerable<string>
{
    private readonly Bucket[] _buckets;

    public static StringPool Shared { get; } = new();

    private const int _defaultPoolCapacity = 4096;
    private const int _defaultBucketCapacity = 32;

    public StringPool(int poolCapacity = _defaultPoolCapacity, int bucketCapacity = _defaultBucketCapacity)
    {
        _buckets = new Bucket[poolCapacity];
        for (int i = 0; i < poolCapacity; i++)
        {
            _buckets[i] = new(bucketCapacity);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _buckets.Length; i++)
        {
            _buckets[i].Clear();
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < _buckets.Length; i++)
        {
            _buckets[i].Clear();
            _buckets[i].Dispose();
        }
    }

    public string GetOrAdd(ReadOnlySpan<char> span)
    {
        return span.Length == 0 ? string.Empty : GetBucket(span).GetOrAdd(span);
    }

    public string GetOrAdd(ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        int charsWritten;
        if (!MemoryHelper.UseStackAlloc<char>(bytes.Length))
        {
            using RentedArray<char> rentedCharBuffer = new(bytes.Length);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer);
            return GetOrAdd(rentedCharBuffer[..charsWritten]);
        }

        Span<char> charBuffer = stackalloc char[bytes.Length];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        return GetOrAdd(charBuffer[..charsWritten]);
    }

    public void Add(string value)
    {
        if (value.Length == 0)
        {
            return;
        }

        GetBucket(value).Add(value);
    }

    public bool TryGet(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
    {
        if (span.Length != 0)
        {
            return GetBucket(span).TryGet(span, out value);
        }

        value = string.Empty;
        return true;
    }

    public bool TryGet(ReadOnlySpan<byte> bytes, Encoding encoding, [MaybeNullWhen(false)] out string value)
    {
        if (bytes.Length == 0)
        {
            value = string.Empty;
            return true;
        }

        value = null;
        int charsWritten;
        if (!MemoryHelper.UseStackAlloc<char>(bytes.Length))
        {
            using RentedArray<char> rentedCharBuffer = new(bytes.Length);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer);
            return TryGet(rentedCharBuffer[..charsWritten], out value);
        }

        Span<char> charBuffer = stackalloc char[bytes.Length];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        return TryGet(charBuffer[..charsWritten], out value);
    }

    [Pure]
    public bool Contains(string str)
    {
        return Contains((ReadOnlySpan<char>)str);
    }

    [Pure]
    public bool Contains(ReadOnlySpan<char> span)
    {
        return span.Length != 0 && GetBucket(span).Contains(span);
    }

    [Pure]
    public bool Contains(ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        if (bytes.Length == 0)
        {
            return false;
        }

        int charsWritten;
        if (!MemoryHelper.UseStackAlloc<char>(bytes.Length))
        {
            using RentedArray<char> rentedCharBuffer = new(bytes.Length);
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer);
            return Contains(rentedCharBuffer[..charsWritten]);
        }

        Span<char> charBuffer = stackalloc char[bytes.Length];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        return Contains(charBuffer[..charsWritten]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Bucket GetBucket(ReadOnlySpan<char> span)
    {
        int hash = GetSimpleStringHash(span);
        int index = (int)(Unsafe.As<int, uint>(ref hash) % _buckets.Length);
        Debug.Assert(index >= 0 && index < _buckets.Length);
        return _buckets[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetSimpleStringHash(ReadOnlySpan<char> chars)
    {
        Debug.Assert(chars.Length > 0);
        int length = chars.Length;
        ref char firstChar = ref MemoryMarshal.GetReference(chars);
        char middleChar = Unsafe.Add(ref firstChar, length >> 1);
        char lastChar = Unsafe.Add(ref firstChar, length - 1);
        return ((firstChar + middleChar + lastChar) * length) ^ ~length;
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

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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

    private readonly struct Bucket : IDisposable, IEnumerable<string>
    {
        private readonly string?[] _strings;
        private readonly SemaphoreSlim _stringsLock = new(1);

        public Bucket(int bucketCapacity = _defaultBucketCapacity)
        {
            _strings = new string[bucketCapacity];
        }

        public void Dispose()
        {
            _stringsLock.Dispose();
        }

        public void Clear()
        {
            _strings.AsSpan().Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetOrAdd(ReadOnlySpan<char> span)
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
        public void Add(string value)
        {
            _stringsLock.Wait();
            try
            {
                Span<string?> strings = _strings;
                strings[..^1].CopyTo(strings[1..]);
                strings[0] = value;
            }
            finally
            {
                _stringsLock.Release();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
        {
            ref string? stringsReference = ref MemoryMarshal.GetArrayDataReference(_strings);
            int stringsLength = _strings.Length;
            for (int i = 0; i < stringsLength; i++)
            {
                string? current = Unsafe.Add(ref stringsReference, i);
                if (current is null)
                {
                    // a null reference can only be followed by more null references,
                    // so we can exit early because the string can definitely not be found
                    value = null;
                    return false;
                }

                if (!span.SequenceEqual(current))
                {
                    continue;
                }

                if (i > 3)
                {
                    MoveStringByFourIndices(i);
                }

                value = current;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Moves a matching item by four places, so that it can be found faster next time.
        /// </summary>
        private void MoveStringByFourIndices(int indexOfMatchingString)
        {
            _stringsLock.Wait();
            try
            {
                _strings.MoveItem(indexOfMatchingString, indexOfMatchingString - 4);
            }
            finally
            {
                _stringsLock.Release();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ReadOnlySpan<char> span)
        {
            return TryGet(span, out _);
        }

        public IEnumerator<string> GetEnumerator()
        {
            foreach (string? str in _strings)
            {
                if (str is not null)
                {
                    yield return str;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
