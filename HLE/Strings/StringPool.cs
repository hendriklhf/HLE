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

public sealed class StringPool : IEquatable<StringPool>, IEnumerable<string>, IDisposable
{
    public int Capacity => _buckets.Length;

    public int BucketCapacity => _buckets[0]._strings.Length;

    internal readonly Bucket[] _buckets;

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

    public void Dispose()
    {
        for (int i = 0; i < _buckets.Length; i++)
        {
            _buckets[i].Dispose();
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _buckets.Length; i++)
        {
            _buckets[i].Clear();
        }
    }

    public string GetOrAdd(string str)
    {
        if (str.Length == 0)
        {
            return string.Empty;
        }

        Bucket bucket = GetBucket(str);
        if (!bucket.Contains(str))
        {
            bucket.Add(str);
        }

        return str;
    }

    public string GetOrAdd(ReadOnlySpan<char> span)
    {
        return span.Length == 0 ? string.Empty : GetBucket(span).GetOrAdd(span);
    }

    [SkipLocalsInit]
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
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer.AsSpan());
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

        Bucket bucket = GetBucket(value);
        if (!bucket.Contains(value))
        {
            bucket.Add(value);
        }
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

    [SkipLocalsInit]
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
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer.AsSpan());
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
    [SkipLocalsInit]
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
            charsWritten = encoding.GetChars(bytes, rentedCharBuffer.AsSpan());
            return Contains(rentedCharBuffer[..charsWritten]);
        }

        Span<char> charBuffer = stackalloc char[bytes.Length];
        charsWritten = encoding.GetChars(bytes, charBuffer);
        return Contains(charBuffer[..charsWritten]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Bucket GetBucket(ReadOnlySpan<char> span)
    {
        int hash = SimpleStringHasher.Hash(span);
        int index = (int)((uint)hash % _buckets.Length);
        Debug.Assert(index >= 0 && index < _buckets.Length);
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
        return RuntimeHelpers.GetHashCode(this);
    }

    public static bool operator ==(StringPool? left, StringPool? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(StringPool? left, StringPool? right)
    {
        return !(left == right);
    }

    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "it does implement IDisposable?!")]
    internal readonly struct Bucket : IEnumerable<string>, IDisposable
    {
        internal readonly string?[] _strings;
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
