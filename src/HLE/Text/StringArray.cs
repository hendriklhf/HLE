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

namespace HLE.Text;

/// <summary>
/// An array that is specialized in storing strings by optimizing data locality,
/// which makes search operations significantly faster,
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
        get => this[index.GetOffset(Length)];
        set => this[index.GetOffset(Length)] = value;
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
    private int _minStringLength = int.MaxValue;
    private int _maxStringLength;

    public static StringArray Empty { get; } = new(0);

    public StringArray(int length) => CtorCore(length, out _strings, out _lengths, out _starts);

    public StringArray(List<string> strings) : this(strings.Count)
        => FillArray(CollectionsMarshal.AsSpan(strings));

    public StringArray(string[] strings) : this(strings.Length)
        => FillArray(strings);

    public StringArray(Span<string> strings) : this(strings.Length)
        => FillArray(strings);

    public StringArray(params ReadOnlySpan<string> strings) : this(strings.Length)
        => FillArray(strings);

    public StringArray(IEnumerable<string> strings)
    {
        if (strings.TryGetReadOnlySpan(out ReadOnlySpan<string> span))
        {
            CtorCore(span.Length, out _strings, out _lengths, out _starts);
            FillArray(span);
            return;
        }

        _strings = strings.ToArray();
        _lengths = new int[_strings.Length];
        _starts = new int[_strings.Length];

        FillArray(_strings);
    }

    private static void CtorCore(int length, out string[] strings, out int[] lengths, out int[] starts)
    {
        if (length == 0)
        {
            strings = [];
            lengths = [];
            starts = [];
            return;
        }

        strings = new string[length];
        lengths = new int[length];
        starts = new int[length];
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetStringLength(int index) => _lengths[index];

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> GetChars(int index)
    {
        if (_chars is null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)index, (uint)Length); // TODO: should be IndexOutOfRangeException
            return [];
        }

        return _chars.AsSpanUnsafe(_starts[index], _lengths[index]);
    }

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
    public List<string> ToList() => Length == 0 ? [] : ListMarshal.ConstructList(AsSpan());

    [Pure]
    public List<string> ToList(int start) => AsSpan().ToList(start);

    [Pure]
    public List<string> ToList(int start, int length) => AsSpan().ToList(start, length);

    [Pure]
    public List<string> ToList(Range range) => AsSpan().ToList(range);

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
        if (Length == 0 || chars.Length > _maxStringLength || chars.Length < _minStringLength)
        {
            return -1;
        }

        return IndexOfCore(chars, comparison, startIndex);
    }

    private unsafe int IndexOfCore(ReadOnlySpan<char> chars, StringComparison comparison, int startIndex = 0)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)startIndex, (uint)Length);

        ReadOnlySpan<int> lengths = _lengths.AsSpan(startIndex..);
        Span<int> indicesBuffer;
        int[]? rentedIndices = null;
        if (MemoryHelpers.UseStackalloc<int>(lengths.Length))
        {
            int* buffer = stackalloc int[lengths.Length];
            indicesBuffer = new(buffer, lengths.Length);
        }
        else
        {
            rentedIndices = ArrayPool<int>.Shared.Rent(lengths.Length);
            indicesBuffer = rentedIndices.AsSpan(..lengths.Length);
        }

        try
        {
            // TODO: maybe partition it, so not every index of the right length has to be found,
            // slow if the index is at the beginning and the whole array will be scanned
            int indicesLength = SpanHelpers.IndicesOf(ref MemoryMarshal.GetReference(lengths), lengths.Length, chars.Length, ref MemoryMarshal.GetReference(indicesBuffer));
            if (indicesLength == 0)
            {
                return -1;
            }

            ref char charsReference = ref MemoryMarshal.GetReference(chars);
            ref string stringsReference = ref MemoryMarshal.GetArrayDataReference(_strings);
            ref int startsReference = ref MemoryMarshal.GetArrayDataReference(_starts);
            ReadOnlySpan<char> charBuffer = _chars;
            ReadOnlySpan<int> indices = indicesBuffer.SliceUnsafe(..indicesLength);
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

        for (int i = 0; i < strings.Length; i++)
        {
            SetString(i, strings[i]);
        }
    }

    public void Clear()
    {
        Array.Clear(_strings);
        Array.Clear(_lengths);
        Array.Clear(_starts);
        _freeBufferSize = _chars?.Length ?? 0;
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

    private void SetString(int index, string str)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);

        Span<string> strings = _strings;
        Span<int> starts = _starts;
        Span<int> lengths = _lengths;
        Span<char> chars = _chars;

        UpdateMinMaxStringLength(str.Length);

        int lengthDifference = str.Length - lengths[index];

        switch (lengthDifference)
        {
            case > 0: // new string is longer
                GrowBufferIfNeeded(lengthDifference);
                chars = _chars; // _chars might have changed through GrowBufferIfNeeded
                if (index != Length - 1)
                {
                    SpanHelpers.Copy(chars[starts[index + 1]..^_freeBufferSize], chars[(starts[index + 1] + lengthDifference)..]);
                }

                SpanHelpers.Copy(str, chars[starts[index]..]);
                SpanHelpers.Add(starts[(index + 1)..], lengthDifference);
                lengths[index] = str.Length;
                break;
            case < 0: // new string is shorter
                if (index != Length - 1)
                {
                    SpanHelpers.Copy(chars[starts[index + 1]..^_freeBufferSize], chars[(starts[index + 1] + lengthDifference)..]);
                }

                SpanHelpers.Copy(str, chars[starts[index]..]);
                SpanHelpers.Add(starts[(index + 1)..], lengthDifference);
                lengths[index] = str.Length;
                break;
            default: // new string has same length
                Span<char> destination = chars[starts[index]..];
                Debug.Assert(destination.Length >= str.Length);
                SpanHelpers.Copy(str, destination);
                break;
        }

        strings[index] = str;
        _freeBufferSize -= lengthDifference;
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
        const int AssumedAverageStringLength = 8;

        char[]? chars = _chars;
        int startingBufferSize = Length * AssumedAverageStringLength;
        int currentBufferSize = chars?.Length ?? startingBufferSize;
        uint neededSize = (uint)(sizeHint - freeBufferSize);

        int newBufferSize = BufferHelpers.GrowArray((uint)currentBufferSize, neededSize);
        char[]? oldBuffer = chars;
        chars = GC.AllocateUninitializedArray<char>(newBufferSize);

        if (oldBuffer is not null)
        {
            SpanHelpers.Copy(oldBuffer.AsSpan(..^freeBufferSize), chars);
            ArrayPool<char>.Shared.Return(oldBuffer);
        }

        _chars = chars;
        _freeBufferSize += newBufferSize - currentBufferSize;
    }

    public ArrayEnumerator<string> GetEnumerator() => new(_strings);

    // ReSharper disable once NotDisposedResourceIsReturned
    IEnumerator<string> IEnumerable<string>.GetEnumerator() => Length == 0 ? EmptyEnumeratorCache<string>.Enumerator : GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals([NotNullWhen(true)] StringArray? other) => ReferenceEquals(this, other);

    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(StringArray? left, StringArray? right) => Equals(left, right);

    public static bool operator !=(StringArray? left, StringArray? right) => !(left == right);
}
