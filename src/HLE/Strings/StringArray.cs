using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Strings;

/// <summary>
/// An array that is specialized in storing strings by optimizing data locality,
/// which makes search operations faster,
/// but comes at the cost of higher memory usage and initialization time.
/// </summary>
public sealed class StringArray :
    ICollection<string>,
    IReadOnlyCollection<string>,
    ICopyable<string>,
    IIndexable<string>,
    IEquatable<StringArray>,
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
        get => _strings[index];
        set => SetString(index.GetOffset(Length), value);
    }

    public ReadOnlySpan<string> this[Range range] => _strings.AsSpan(range);

    public int Length => _strings.Length;

    int ICountable.Count => Length;

    int ICollection<string>.Count => Length;

    int IReadOnlyCollection<string>.Count => Length;

    bool ICollection<string>.IsReadOnly => false;

    internal readonly string[] _strings;
    internal readonly int[] _lengths;
    internal readonly int[] _starts;
    internal char[]? _chars;
    private int _freeBufferSize;

    public static StringArray Empty { get; } = new(0);

    public StringArray(int length)
    {
        if (length == 0)
        {
            _strings = [];
            _lengths = [];
            _starts = [];
            return;
        }

        _strings = new string[length];
        _lengths = new int[length];
        _starts = new int[length];
    }

    public StringArray(List<string> strings) : this(strings.Count)
        => FillArray(CollectionsMarshal.AsSpan(strings));

    public StringArray(string[] strings) : this(strings.Length)
        => FillArray(strings);

    public StringArray(Span<string> strings) : this(strings.Length)
        => FillArray(strings);

    public StringArray(ReadOnlySpan<string> strings) : this(strings.Length)
        => FillArray(strings);

    public StringArray(IEnumerable<string> strings)
    {
        _strings = strings.ToArray();
        _lengths = new int[_strings.Length];
        _starts = new int[_strings.Length];

        FillArray(_strings);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetStringLength(int index) => _lengths[index];

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> GetChars(int index) => _chars is null ? [] : _chars.AsSpanUnsafe(_starts[index], _lengths[index]);

    [Pure]
    public ReadOnlySpan<string> AsSpan() => _strings;

    [Pure]
    public ReadOnlyMemory<string> AsMemory() => _strings;

    [Pure]
    public string[] ToArray()
    {
        int length = Length;
        if (length == 0)
        {
            return [];
        }

        string[] result = new string[length];
        SpanHelpers<string>.Copy(_strings, result);
        return result;
    }

    [Pure]
    public string[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public string[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public string[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public List<string> ToList()
    {
        int length = Length;
        if (length == 0)
        {
            return [];
        }

        List<string> result = new(length);
        CopyWorker<string> copyWorker = new(_strings);
        copyWorker.CopyTo(result);
        return result;
    }

    ReadOnlySpan<string> IReadOnlySpanProvider<string>.GetReadOnlySpan() => AsSpan();

    ReadOnlyMemory<string> IReadOnlyMemoryProvider<string>.GetReadOnlyMemory() => AsMemory();

    [Pure]
    public int IndexOf(string str, int startIndex = 0) => IndexOf(str.AsSpan(), StringComparison.Ordinal, startIndex);

    [Pure]
    public int IndexOf(ReadOnlySpan<char> chars, int startIndex = 0) => IndexOf(chars, StringComparison.Ordinal, startIndex);

    [Pure]
    public int IndexOf(string str, StringComparison comparison, int startIndex = 0) => IndexOf(str.AsSpan(), comparison, startIndex);

    [Pure]
    [SkipLocalsInit]
    public int IndexOf(ReadOnlySpan<char> chars, StringComparison comparison, int startIndex = 0)
    {
        ref string stringsReference = ref MemoryMarshal.GetArrayDataReference(_strings);
        ref int startsReference = ref MemoryMarshal.GetArrayDataReference(_starts);
        ReadOnlySpan<char> charBuffer = _chars;
        ReadOnlySpan<int> stringLengths = _lengths;

        ref char charsReference = ref MemoryMarshal.GetReference(chars);
        ReadOnlySpan<int> lengths = stringLengths[startIndex..];

        Span<int> indices;
        int[]? rentedIndices = null;
        if (!MemoryHelpers.UseStackalloc<int>(lengths.Length))
        {
            rentedIndices = ArrayPool<int>.Shared.Rent(lengths.Length);
            indices = rentedIndices.AsSpan(..lengths.Length);
        }
        else
        {
            indices = SpanMarshal.ReturnStackAlloced(stackalloc int[lengths.Length]);
        }

        try
        {
            // TODO: maybe partition it, so not every index of the right length has to be found,
            // slow if the index is at the beginning and the whole array will be scanned
            int indicesLength = SpanHelpers.IndicesOf(ref MemoryMarshal.GetReference(stringLengths), stringLengths.Length, chars.Length, ref MemoryMarshal.GetReference(indices));
            if (indicesLength == 0)
            {
                return -1;
            }

            indices = indices.SliceUnsafe(..indicesLength);

            for (int i = 0; i < indices.Length; i++)
            {
                int actualIndex = indices[i] + startIndex;
                string str = Unsafe.Add(ref stringsReference, actualIndex);
                ref char stringReference = ref StringMarshal.GetReference(str);
                if (Unsafe.AreSame(ref charsReference, ref stringReference))
                {
                    return actualIndex;
                }

                int strStart = Unsafe.Add(ref startsReference, actualIndex);
                if (chars.Equals(charBuffer.SliceUnsafe(strStart, chars.Length), comparison))
                {
                    return actualIndex;
                }
            }

            return -1;
        }
        finally
        {
            if (rentedIndices is not null)
            {
                ArrayPool<int>.Shared.Return(rentedIndices);
            }
        }
    }

    [Pure]
    public bool Contains(string str) => Contains(str.AsSpan());

    [Pure]
    public bool Contains(string str, StringComparison comparison) => IndexOf(str.AsSpan(), comparison) >= 0;

    [Pure]
    public bool Contains(ReadOnlySpan<char> chars) => IndexOf(chars) >= 0;

    [Pure]
    public bool Contains(ReadOnlySpan<char> chars, StringComparison comparison) => IndexOf(chars, comparison) >= 0;

    void ICollection<string>.Add(string item) => throw new NotSupportedException();

    bool ICollection<string>.Remove(string item) => throw new NotSupportedException();

    private void FillArray(ReadOnlySpan<string> strings)
    {
        Debug.Assert(strings.Length == Length);

        ref string stringsReference = ref MemoryMarshal.GetReference(strings);
        int stringsLength = strings.Length;
        for (int i = 0; i < stringsLength; i++)
        {
            SetString(i, Unsafe.Add(ref stringsReference, i));
        }
    }

    public void Clear()
    {
        Array.Clear(_strings);
        Array.Clear(_lengths);
        Array.Clear(_starts);
        _freeBufferSize = _chars?.Length ?? 0;
    }

    public ArrayEnumerator<string> GetEnumerator() => new(_strings);

    IEnumerator<string> IEnumerable<string>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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

    private void SetString(int index, string str)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);

        Span<string> strings = _strings;
        Span<int> starts = _starts;
        Span<int> lengths = _lengths;
        Span<char> chars = _chars;

        int lengthDifference = str.Length - lengths[index];

        switch (lengthDifference)
        {
            case > 0: // new string is longer
                GrowBufferIfNeeded(lengthDifference);
                chars = _chars;
                if (index != Length - 1)
                {
                    SpanHelpers<char>.Copy(chars[starts[index + 1]..^_freeBufferSize], chars[(starts[index + 1] + lengthDifference)..]);
                }

                SpanHelpers<char>.Copy(str, chars[starts[index]..]);
                SpanHelpers.Add(starts[(index + 1)..], lengthDifference);
                lengths[index] = str.Length;
                break;
            case < 0: // new string is shorter
                if (index != Length - 1)
                {
                    SpanHelpers<char>.Copy(chars[starts[index + 1]..^_freeBufferSize], chars[(starts[index + 1] + lengthDifference)..]);
                }

                SpanHelpers<char>.Copy(str, chars[starts[index]..]);
                SpanHelpers.Add(starts[(index + 1)..], lengthDifference);
                lengths[index] = str.Length;
                break;
            default: // new string has same length
                Span<char> destination = chars[starts[index]..];
                SpanHelpers<char>.Copy(str, destination);
                break;
        }

        strings[index] = str;
        _freeBufferSize -= lengthDifference;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // inline as fast path
    private void GrowBufferIfNeeded(int sizeHint)
    {
        int freeBufferSize = _freeBufferSize;
        if (sizeHint < 1 || freeBufferSize >= sizeHint)
        {
            return;
        }

        GrowBuffer(sizeHint, freeBufferSize);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // don't inline as slow path
    private void GrowBuffer(int sizeHint, int freeBufferSize)
    {
        char[]? chars = _chars;
        int currentBufferSize = chars?.Length ?? 8;
        uint neededSize = (uint)(sizeHint - freeBufferSize);

        int newBufferSize = BufferHelpers.GrowArray((uint)currentBufferSize, neededSize);
        char[]? oldBuffer = chars;
        chars = GC.AllocateUninitializedArray<char>(newBufferSize);

        if (oldBuffer is not null)
        {
            SpanHelpers<char>.Copy(oldBuffer.AsSpan(..^freeBufferSize), chars);
            ArrayPool<char>.Shared.Return(oldBuffer);
        }

        _chars = chars;
        _freeBufferSize += newBufferSize - currentBufferSize;
    }

    public bool Equals([NotNullWhen(true)] StringArray? other) => ReferenceEquals(this, other);

    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(StringArray? left, StringArray? right) => Equals(left, right);

    public static bool operator !=(StringArray? left, StringArray? right) => !(left == right);
}
