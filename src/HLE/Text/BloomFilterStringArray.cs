using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Text;

/// <summary>
/// An array that is specialized in storing strings by optimizing data storage,
/// which makes search operations significantly faster,
/// but comes at the cost of higher memory usage and initialization time.
/// </summary>
public sealed class BloomFilterStringArray :
    ICollection<string>,
    IReadOnlyCollection<string>,
    ICopyable<string>,
    IIndexable<string>,
    IEquatable<BloomFilterStringArray>,
    IReadOnlySpanProvider<string>,
    IReadOnlyMemoryProvider<string>,
    ICollectionProvider<string>
{
    public string this[int index]
    {
        get => _strings[index];
        set => SetString(index, value);
    }

    public string this[Index index]
    {
        get => this[index.GetOffset(Length)];
        set => this[index.GetOffset(Length)] = value;
    }

    public ReadOnlySpan<string> this[Range range] => _strings.AsSpan(range);

    public int Length => _strings.Length;

    int ICountable.Count => Length;

    int ICollection<string>.Count => Length;

    int IReadOnlyCollection<string>.Count => Length;

    bool ICollection<string>.IsReadOnly => false;

    private readonly string[] _strings;
    private readonly int[] _lengths;
    private char[] _chars = [];
    private int _minStringLength = int.MaxValue;
    private int _maxStringLength;

    private const int BitsPerMap = sizeof(uint) * 8;

    public static BloomFilterStringArray Empty { get; } = new(0);

    public BloomFilterStringArray(int length) => CtorCore(length, out _strings, out _lengths);

    public BloomFilterStringArray(List<string> strings) : this(strings.Count)
        => FillArray(CollectionsMarshal.AsSpan(strings));

    public BloomFilterStringArray(string[] strings) : this(strings.Length)
        => FillArray(strings);

    public BloomFilterStringArray(Span<string> strings) : this(strings.Length)
        => FillArray(strings);

    public BloomFilterStringArray(params ReadOnlySpan<string> strings) : this(strings.Length)
        => FillArray(strings);

    public BloomFilterStringArray(IEnumerable<string> strings)
    {
        if (strings.TryGetReadOnlySpan(out ReadOnlySpan<string> span))
        {
            CtorCore(span.Length, out _strings, out _lengths);
            FillArray(span);
            return;
        }

        _strings = strings.ToArray();
        _lengths = new int[_strings.Length];

        FillArray(_strings);
    }

    private static void CtorCore(int length, out string[] strings, out int[] lengths)
    {
        if (length == 0)
        {
            strings = [];
            lengths = [];
            return;
        }

        strings = new string[length];
        lengths = new int[length];
    }

    private void FillArray(ReadOnlySpan<string> strings)
    {
        Debug.Assert(strings.Length == _strings.Length);

        for (int i = 0; i < strings.Length; i++)
        {
            SetString(i, strings[i]);
        }
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetStringLength(int index) => _lengths[index];

    [Pure]
    public ReadOnlySpan<string> AsSpan() => _strings;

    [Pure]
    public ReadOnlySpan<string> AsSpan(int start) => _strings.AsSpan(start);

    [Pure]
    public ReadOnlySpan<string> AsSpan(int start, int length) => _strings.AsSpan(start, length);

    [Pure]
    public ReadOnlySpan<string> AsSpan(Range range) => _strings.AsSpan(range);

    [Pure]
    public ReadOnlyMemory<string> AsMemory() => _strings;

    [Pure]
    public ReadOnlyMemory<string> AsMemory(int start) => _strings.AsMemory(start);

    [Pure]
    public ReadOnlyMemory<string> AsMemory(int start, int length) => _strings.AsMemory(start, length);

    [Pure]
    public ReadOnlyMemory<string> AsMemory(Range range) => _strings.AsMemory(range);

    [Pure]
    public string[] ToArray()
    {
        int length = Length;
        if (length == 0)
        {
            return [];
        }

        string[] result = new string[length];
        SpanHelpers.Copy(_strings, result);
        return result;
    }

    [Pure]
    public string[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public string[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public string[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public List<string> ToList() => AsSpan().ToList();

    [Pure]
    public List<string> ToList(int start) => AsSpan().ToList(start);

    [Pure]
    public List<string> ToList(int start, int length) => AsSpan().ToList(start, length);

    [Pure]
    public List<string> ToList(Range range) => AsSpan().ToList(range);

    [Pure]
    public int IndexOf(string str, int startIndex = 0) => IndexOf(str.AsSpan(), startIndex);

    [Pure]
    public int IndexOf(ReadOnlySpan<char> str, int startIndex = 0)
    {
        if (Length == 0 || str.Length < _minStringLength || str.Length > _maxStringLength)
        {
            return -1;
        }

        return IndexOfCore(str, startIndex);
    }

    private int IndexOfCore(ReadOnlySpan<char> str, int startIndex = 0)
    {
        int iteration = 0;
        ref string strings = ref MemoryMarshal.GetArrayDataReference(_strings);
        int length = Length - startIndex;
        ref int stringLengths = ref ArrayMarshal.GetUnsafeElementAt(_lengths, startIndex);
        ref char chars = ref ArrayMarshal.GetUnsafeElementAt(_chars, startIndex);
        while (length >= BitsPerMap)
        {
            uint map = GetMatchingLengthsMap(ref stringLengths, str.Length);
            if (map == 0)
            {
                length -= BitsPerMap;
                stringLengths = ref Unsafe.Add(ref stringLengths, BitsPerMap);
                chars = ref Unsafe.Add(ref chars, BitsPerMap);
                iteration++;
                continue;
            }

            if (BitOperations.PopCount(map) == 1)
            {
                int index = GetActualIndex(BitOperations.TrailingZeroCount(map), iteration, startIndex);
                if (str.SequenceEqual(Unsafe.Add(ref strings, index)))
                {
                    return index;
                }
            }

            for (int i = 0; i < str.Length; i++)
            {
                map &= GetMatchingCharsMap(ref Unsafe.As<char, ushort>(ref Unsafe.Add(ref chars, i * Length)), str[i]);
                if (map == 0)
                {
                    goto Continue;
                }

                if (BitOperations.PopCount(map) == 1)
                {
                    int index = GetActualIndex(BitOperations.TrailingZeroCount(map), iteration, startIndex);
                    if (str.SequenceEqual(Unsafe.Add(ref strings, index)))
                    {
                        return index;
                    }

                    break;
                }
            }

            return GetActualIndex(BitOperations.TrailingZeroCount(map), iteration, startIndex);

        Continue:
            length -= BitsPerMap;
            stringLengths = ref Unsafe.Add(ref stringLengths, BitsPerMap);
            chars = ref Unsafe.Add(ref chars, BitsPerMap);
            iteration++;
        }

        if (length != 0)
        {
            ReadOnlySpan<string> remainder = _strings.AsSpanUnsafe(^length..);
            for (int i = 0; i < remainder.Length; i++)
            {
                if (str.SequenceEqual(remainder[i]))
                {
                    return GetActualIndex(i, iteration, startIndex);
                }
            }
        }

        return -1;
    }

    private static int GetActualIndex(int index, int iteration, int startIndex) => index + BitsPerMap * iteration + startIndex;

    private static uint GetMatchingCharsMap(ref ushort chars, ushort needle)
    {
        if (Vector512.IsHardwareAccelerated)
        {
            Vector512<ushort> needleVector = Vector512.Create(needle);
            Vector512<ushort> charsVector = Vector512.LoadUnsafe(ref chars);
            return (uint)Vector512.Equals(charsVector, needleVector).ExtractMostSignificantBits();
        }

        uint map = 0;
        if (Vector256.IsHardwareAccelerated)
        {
            Vector256<ushort> needleVector = Vector256.Create(needle);

            Vector256<ushort> firstCharsVector = Vector256.LoadUnsafe(ref chars);
            map |= Vector256.Equals(needleVector, firstCharsVector).ExtractMostSignificantBits();

            Vector256<ushort> secondCharsVector = Vector256.LoadUnsafe(ref Unsafe.Add(ref chars, Vector256<ushort>.Count));
            uint secondMapPart = Vector256.Equals(needleVector, secondCharsVector).ExtractMostSignificantBits();

            return map | (secondMapPart << Vector256<ushort>.Count);
        }

        if (Vector128.IsHardwareAccelerated)
        {
            Vector128<ushort> needleVector = Vector128.Create(needle);

            Vector128<ushort> firstCharsVector = Vector128.LoadUnsafe(ref chars);
            map |= Vector128.Equals(needleVector, firstCharsVector).ExtractMostSignificantBits();

            Vector128<ushort> secondCharsVector = Vector128.LoadUnsafe(ref Unsafe.Add(ref chars, Vector128<ushort>.Count));
            map |= (Vector128.Equals(needleVector, secondCharsVector).ExtractMostSignificantBits() << Vector128<ushort>.Count);

            Vector128<ushort> thirdCharsVector = Vector128.LoadUnsafe(ref Unsafe.Add(ref chars, Vector128<ushort>.Count * 2));
            map |= (Vector128.Equals(needleVector, thirdCharsVector).ExtractMostSignificantBits() << Vector128<ushort>.Count * 2);

            Vector128<ushort> forthCharsVector = Vector128.LoadUnsafe(ref Unsafe.Add(ref chars, Vector128<ushort>.Count * 3));
            map |= (Vector128.Equals(needleVector, forthCharsVector).ExtractMostSignificantBits() << Vector128<ushort>.Count * 3);

            return map;
        }

        for (int i = 0; i < BitsPerMap; i++)
        {
            if (Unsafe.Add(ref chars, i) == needle)
            {
                map |= (1U << i);
            }
        }

        return map;
    }

    private static uint GetMatchingLengthsMap(ref int stringLengths, int needle)
    {
        uint map = 0;
        if (Vector512.IsHardwareAccelerated)
        {
            Vector512<int> needleVector = Vector512.Create(needle);

            Vector512<int> firstLengthsVector = Vector512.LoadUnsafe(ref stringLengths);
            map |= (uint)Vector512.Equals(needleVector, firstLengthsVector).ExtractMostSignificantBits();

            Vector512<int> secondLengthsVector = Vector512.LoadUnsafe(ref Unsafe.Add(ref stringLengths, Vector512<int>.Count));
            uint secondMapPart = (uint)Vector512.Equals(needleVector, secondLengthsVector).ExtractMostSignificantBits();

            return map | (secondMapPart << Vector512<int>.Count);
        }

        if (Vector256.IsHardwareAccelerated)
        {
            Vector256<int> needleVector = Vector256.Create(needle);

            Vector256<int> firstCharsVector = Vector256.LoadUnsafe(ref stringLengths);
            map |= Vector256.Equals(needleVector, firstCharsVector).ExtractMostSignificantBits();

            Vector256<int> secondCharsVector = Vector256.LoadUnsafe(ref Unsafe.Add(ref stringLengths, Vector256<int>.Count));
            map |= (Vector256.Equals(needleVector, secondCharsVector).ExtractMostSignificantBits() << Vector256<int>.Count);

            Vector256<int> thirdCharsVector = Vector256.LoadUnsafe(ref Unsafe.Add(ref stringLengths, Vector256<int>.Count * 2));
            map |= (Vector256.Equals(needleVector, thirdCharsVector).ExtractMostSignificantBits() << Vector256<int>.Count * 2);

            Vector256<int> forthCharsVector = Vector256.LoadUnsafe(ref Unsafe.Add(ref stringLengths, Vector256<int>.Count * 3));
            map |= (Vector256.Equals(needleVector, forthCharsVector).ExtractMostSignificantBits() << Vector256<int>.Count * 3);

            return map;
        }

        if (Vector128.IsHardwareAccelerated)
        {
            Vector128<int> needleVector = Vector128.Create(needle);

            Vector128<int> firstCharsVector = Vector128.LoadUnsafe(ref stringLengths);
            map |= Vector128.Equals(needleVector, firstCharsVector).ExtractMostSignificantBits();

            Vector128<int> secondCharsVector = Vector128.LoadUnsafe(ref Unsafe.Add(ref stringLengths, Vector128<int>.Count));
            map |= (Vector128.Equals(needleVector, secondCharsVector).ExtractMostSignificantBits() << Vector128<int>.Count);

            Vector128<int> thirdCharsVector = Vector128.LoadUnsafe(ref Unsafe.Add(ref stringLengths, Vector128<int>.Count * 2));
            map |= (Vector128.Equals(needleVector, thirdCharsVector).ExtractMostSignificantBits() << Vector128<int>.Count * 2);

            Vector128<int> forthCharsVector = Vector128.LoadUnsafe(ref Unsafe.Add(ref stringLengths, Vector128<int>.Count * 3));
            map |= (Vector128.Equals(needleVector, forthCharsVector).ExtractMostSignificantBits() << Vector128<int>.Count * 3);

            Vector128<int> fifthCharsVector = Vector128.LoadUnsafe(ref Unsafe.Add(ref stringLengths, Vector128<int>.Count * 3));
            map |= (Vector128.Equals(needleVector, fifthCharsVector).ExtractMostSignificantBits() << Vector128<int>.Count * 4);

            Vector128<int> sixthCharsVector = Vector128.LoadUnsafe(ref Unsafe.Add(ref stringLengths, Vector128<int>.Count * 3));
            map |= (Vector128.Equals(needleVector, sixthCharsVector).ExtractMostSignificantBits() << Vector128<int>.Count * 5);

            Vector128<int> seventhCharsVector = Vector128.LoadUnsafe(ref Unsafe.Add(ref stringLengths, Vector128<int>.Count * 3));
            map |= (Vector128.Equals(needleVector, seventhCharsVector).ExtractMostSignificantBits() << Vector128<int>.Count * 6);

            Vector128<int> eighthsCharsVector = Vector128.LoadUnsafe(ref Unsafe.Add(ref stringLengths, Vector128<int>.Count * 3));
            map |= (Vector128.Equals(needleVector, eighthsCharsVector).ExtractMostSignificantBits() << Vector128<int>.Count * 7);

            return map;
        }

        for (int i = 0; i < BitsPerMap; i++)
        {
            if (Unsafe.Add(ref stringLengths, i) == needle)
            {
                map |= (1U << i);
            }
        }

        return map;
    }

    [Pure]
    public bool Contains(ReadOnlySpan<char> str) => IndexOf(str) >= 0;

    [Pure]
    public bool Contains(string str) => Contains(str.AsSpan());

    void ICollection<string>.Add(string item) => throw new NotSupportedException();

    bool ICollection<string>.Remove(string item) => throw new NotSupportedException();

    private void SetString(int index, string str)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)index, (uint)Length);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (str is null)
        {
            _strings[index] = null!;
            _lengths[index] = 0;
            return;
        }

        GrowCharBufferIfNeeded(str.Length);

        UpdateMinMaxStringLength(str.Length);

        _strings[index] = str;
        _lengths[index] = str.Length;

        int length = Length;
        ref char chars = ref ArrayMarshal.GetUnsafeElementAt(_chars, index);
        for (int i = 0; i < str.Length; i++)
        {
            Unsafe.Add(ref chars, length * i) = str[i];
        }
    }

    private void UpdateMinMaxStringLength(int stringLength)
    {
        if (stringLength > _maxStringLength)
        {
            _maxStringLength = stringLength;
        }

        if (stringLength < _minStringLength)
        {
            _minStringLength = stringLength;
        }
    }

    private void GrowCharBufferIfNeeded(int stringLength)
    {
        if (stringLength <= _maxStringLength)
        {
            return;
        }

        int requiredBufferLength = stringLength * Length;
        char[] newBuffer = GC.AllocateUninitializedArray<char>(requiredBufferLength);
        char[] oldBuffer = _chars;
        SpanHelpers.Copy(oldBuffer, newBuffer);
        ArrayPool<char>.Shared.Return(oldBuffer);
        _chars = newBuffer;
    }

    public void Clear()
    {
        _minStringLength = int.MaxValue;
        _maxStringLength = 0;
        Array.Clear(_strings);
        Array.Clear(_lengths);
        Array.Clear(_chars);
    }

    public void CopyTo(List<string> destination, int offset = 0)
    {
        CopyWorker<string> copyWorker = new(_strings);
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(string[] destination, int offset = 0)
    {
        CopyWorker<string> copyWorker = new(_strings);
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<string> destination)
    {
        CopyWorker<string> copyWorker = new(_strings);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(Span<string> destination)
    {
        CopyWorker<string> copyWorker = new(_strings);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(ref string destination)
    {
        CopyWorker<string> copyWorker = new(_strings);
        copyWorker.CopyTo(ref destination);
    }

    public unsafe void CopyTo(string* destination)
    {
        CopyWorker<string> copyWorker = new(_strings);
        copyWorker.CopyTo(destination);
    }

    public ArrayEnumerator<string> GetEnumerator() => new(_strings);

    // ReSharper disable once NotDisposedResourceIsReturned
    IEnumerator<string> IEnumerable<string>.GetEnumerator() => Length == 0 ? EmptyEnumeratorCache<string>.Enumerator : GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] BloomFilterStringArray? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(BloomFilterStringArray? left, BloomFilterStringArray? right) => Equals(left, right);

    public static bool operator !=(BloomFilterStringArray? left, BloomFilterStringArray? right) => !(left == right);
}
