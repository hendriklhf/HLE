using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Strings;

/// <summary>
/// An array that is specialized in storing strings by optimizing data locality, which makes search operations faster, but comes at the cost of higher memory usage and initialization time.
/// </summary>
public sealed class StringArray : ICollection<string>, IReadOnlyCollection<string>, ICopyable<string>, ICountable, IIndexAccessible<string>,
    IEquatable<StringArray>, IReadOnlySpanProvider<string>, ICollectionProvider<string>
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
    public ReadOnlySpan<char> GetChars(int index) => _chars!.AsSpanUnsafe(_starts[index], _lengths[index]);

    [Pure]
    public ReadOnlySpan<string> AsSpan() => _strings;

    [Pure]
    public string[] ToArray()
    {
        if (Length == 0)
        {
            return [];
        }

        string[] result = new string[Length];
        CopyWorker<string>.Copy(_strings, result);
        return result;
    }

    [Pure]
    public List<string> ToList()
    {
        if (Length == 0)
        {
            return [];
        }

        List<string> result = new(Length);
        CopyWorker<string> copyWorker = new(_strings);
        copyWorker.CopyTo(result);
        return result;
    }

    ReadOnlySpan<string> IReadOnlySpanProvider<string>.GetReadOnlySpan() => AsSpan();

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

        int charsLength = chars.Length;
        ref char charsReference = ref MemoryMarshal.GetReference(chars);
        ReadOnlySpan<int> lengths = stringLengths[startIndex..];

        Span<int> indices;
        int[]? rentedIndices = null;
        if (!MemoryHelper.UseStackAlloc<int>(lengths.Length))
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
            int indicesLength = SpanHelpers.IndicesOf(ref MemoryMarshal.GetReference(stringLengths), stringLengths.Length, charsLength, indices);
            indices = indices.SliceUnsafe(..indicesLength);

            for (int i = 0; i < indices.Length; i++)
            {
                int actualIndex = indices[i] + startIndex;
                ReadOnlySpan<char> str = Unsafe.Add(ref stringsReference, actualIndex);
                ref char stringReference = ref MemoryMarshal.GetReference(str);
                if (Unsafe.AreSame(ref charsReference, ref stringReference))
                {
                    return actualIndex;
                }

                int strStart = Unsafe.Add(ref startsReference, actualIndex);
                if (chars.Equals(charBuffer.SliceUnsafe(strStart..(strStart + charsLength)), comparison))
                {
                    return actualIndex;
                }
            }

            return -1;
        }
        finally
        {
            ArrayPool<int>.Shared.Return(rentedIndices);
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

    void ICollection<string>.Add(string str) => throw new NotSupportedException();

    bool ICollection<string>.Remove(string str) => throw new NotSupportedException();

    internal void MoveString(int sourceIndex, int destinationIndex)
    {
        if (sourceIndex == destinationIndex)
        {
            return;
        }

        Span<string> strings = _strings;
        Span<int> lengths = _lengths;
        Span<int> starts = _starts;
        Span<char> chars = _chars;

        string sourceString = strings[sourceIndex];
        int sourceLength = lengths[sourceIndex];
        int sourceStart = starts[sourceIndex];
        int destinationLength = lengths[destinationIndex];
        int destinationStart = starts[destinationIndex];

        int greaterIndex = sourceIndex > destinationIndex ? sourceIndex : destinationIndex;
        bool isSourceRightOfDestination = greaterIndex == sourceIndex;
        int smallerIndex = isSourceRightOfDestination ? destinationIndex : sourceIndex;
        if (isSourceRightOfDestination)
        {
            strings[destinationIndex..sourceIndex].CopyTo(strings[(destinationIndex + 1)..]);
            lengths[destinationIndex..sourceIndex].CopyTo(lengths[(destinationIndex + 1)..]);
            chars[destinationStart..sourceStart].CopyTo(chars[(destinationStart + sourceLength)..]);
        }
        else
        {
            strings[(sourceIndex + 1)..(destinationIndex + 1)].CopyTo(strings[sourceIndex..]);
            lengths[(sourceIndex + 1)..(destinationIndex + 1)].CopyTo(lengths[sourceIndex..]);
            chars[(sourceStart + sourceLength)..(destinationStart + destinationLength)].CopyTo(chars[sourceStart..]);
        }

        strings[destinationIndex] = sourceString;
        lengths[destinationIndex] = sourceLength;
        UpdateStringStarts(starts, lengths, smallerIndex, greaterIndex);
        sourceString.CopyTo(chars[starts[destinationIndex]..]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateStringStarts(Span<int> starts, Span<int> lengths, int startIndex, int endIndex)
    {
        int nextStart = starts[startIndex] + lengths[startIndex];
        for (int i = startIndex + 1; i <= endIndex; i++)
        {
            starts[i] = nextStart;
            nextStart += lengths[i];
        }
    }

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

    public ArrayEnumerator<string> GetEnumerator() => new(_strings, 0, _strings.Length);

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
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length, nameof(index));

        Span<string> strings = _strings;
        Span<int> starts = _starts;
        Span<int> lengths = _lengths;
        Span<char> chars = _chars;

        int lengthDifference = str.Length - lengths[index];

        switch (lengthDifference)
        {
            case > 0: // new string is longer
            {
                GrowBufferIfNeeded(lengthDifference);
                chars = _chars;
                if (index != Length - 1)
                {
                    CopyWorker<char>.Copy(chars[starts[index + 1]..^_freeBufferSize], chars[(starts[index + 1] + lengthDifference)..]);
                }

                CopyWorker<char>.Copy(str, chars[starts[index]..]);
                SpanHelpers.Add(starts[(index + 1)..], lengthDifference);
                lengths[index] = str.Length;
                break;
            }
            case < 0: // new string is shorter
            {
                if (index != Length - 1)
                {
                    CopyWorker<char>.Copy(chars[starts[index + 1]..^_freeBufferSize], chars[(starts[index + 1] + lengthDifference)..]);
                }

                CopyWorker<char>.Copy(str, chars[starts[index]..]);
                SpanHelpers.Add(starts[(index + 1)..], lengthDifference);
                lengths[index] = str.Length;
                break;
            }
            default: // new string has same length
            {
                Span<char> destination = chars[starts[index]..];
                CopyWorker<char>.Copy(str, destination);
                break;
            }
        }

        strings[index] = str;
        _freeBufferSize -= lengthDifference;
    }

    private void GrowBufferIfNeeded(int sizeHint)
    {
        if (sizeHint < 1 || _freeBufferSize >= sizeHint)
        {
            return;
        }

        int currentBufferSize = _chars?.Length ?? 8;
        int newBufferSize = BufferHelpers.GrowByPow2(currentBufferSize, sizeHint - _freeBufferSize);
        char[]? oldBuffer = _chars;
        _chars = GC.AllocateUninitializedArray<char>(newBufferSize);
        if (oldBuffer is not null)
        {
            CopyWorker<char>.Copy(oldBuffer[..^_freeBufferSize], _chars);
            ArrayPool<char>.Shared.Return(oldBuffer);
        }

        _freeBufferSize += newBufferSize - currentBufferSize;
    }

    public bool Equals(StringArray? other) => ReferenceEquals(this, other);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(StringArray? left, StringArray? right) => Equals(left, right);

    public static bool operator !=(StringArray? left, StringArray? right) => !(left == right);
}
