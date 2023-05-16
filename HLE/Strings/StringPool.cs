using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using HLE.Memory;

namespace HLE.Strings;

public sealed class StringPool : IEquatable<StringPool>
{
    private readonly Bucket[] _buckets = new Bucket[_defaultPoolCapacity];
    private readonly SemaphoreSlim _bucketsLock = new(1);

    public static StringPool Shared { get; set; } = new();

    private const int _defaultPoolCapacity = 4096;
    private const int _defaultBucketCapacity = 32;

    public StringPool()
    {
        Reset();
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

    public void Reset()
    {
        _bucketsLock.Wait();
        try
        {
            for (int i = 0; i < _defaultPoolCapacity; i++)
            {
                _buckets[i].Dispose();
                _buckets[i] = new(_defaultBucketCapacity);
            }
        }
        finally
        {
            _bucketsLock.Release();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Bucket GetBucket(ReadOnlySpan<char> span)
    {
        int hash = string.GetHashCode(span);
        int index = (int)(Unsafe.As<int, uint>(ref hash) % _defaultPoolCapacity);
        return _buckets[index];
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

    private readonly struct Bucket : IDisposable
    {
        private readonly string[] _strings;
        private readonly SemaphoreSlim _stringsLock = new(1);

        public Bucket(int capacity = _defaultBucketCapacity)
        {
            _strings = new string[capacity];
            _strings.AsSpan().Fill(string.Empty);
        }

        public void Dispose()
        {
            _stringsLock?.Dispose();
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
                Span<string> strings = _strings;
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
            ref string strings = ref MemoryMarshal.GetArrayDataReference(_strings);
            for (int i = 0; i < _defaultBucketCapacity; i++)
            {
                string currentString = Unsafe.Add(ref strings, i);
                if (!span.SequenceEqual(currentString))
                {
                    continue;
                }

                switch (i)
                {
                    case < 4:
                        break;
                    case < 8:
                        Move(i, 3);
                        break;
                    case < 12:
                        Move(i, 6);
                        break;
                    case < 16:
                        Move(i, 9);
                        break;
                    case < 20:
                        Move(i, 12);
                        break;
                    case < 24:
                        Move(i, 15);
                        break;
                    case < 28:
                        Move(i, 18);
                        break;
                    default:
                        Move(i, 21);
                        break;
                }

                value = currentString;
                return true;
            }

            value = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ReadOnlySpan<char> span)
        {
            return TryGet(span, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Move(int sourceIndex, int destinationIndex)
        {
            if (sourceIndex == destinationIndex)
            {
                return;
            }

            _stringsLock.Wait();
            try
            {
                Span<string> strings = _strings;
                string value = strings[sourceIndex];
                if (sourceIndex > destinationIndex)
                {
                    strings[destinationIndex..sourceIndex].CopyTo(strings[(destinationIndex + 1)..]);
                }
                else
                {
                    strings[(sourceIndex + 1)..(destinationIndex + 1)].CopyTo(strings[sourceIndex..]);
                }

                strings[destinationIndex] = value;
            }
            finally
            {
                _stringsLock.Release();
            }
        }
    }
}
