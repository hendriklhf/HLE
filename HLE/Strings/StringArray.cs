using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Memory;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Strings;

/// <summary>
/// An array that is specialized in storing strings by optimizing data locality, which makes search operations faster, but comes at the cost of higher memory usage.
/// </summary>
public sealed class StringArray : ICollection<string>, IReadOnlyCollection<string>, ICopyable<string>, ICountable, IIndexAccessible<string>, IEquatable<StringArray>
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
    internal readonly int[] _stringLengths;
    internal readonly int[] _stringStarts;
    internal char[] _stringChars;

    public StringArray(int length)
    {
        _strings = new string[length];
        _stringLengths = new int[length];
        _stringStarts = new int[length];
        _stringChars = new char[(nuint)length << 2];
    }

    [CollectionAccess(CollectionAccessType.UpdatedContent)]
    public StringArray(ReadOnlySpan<string> strings) : this(strings.Length)
    {
        FillArray(strings);
    }

    [CollectionAccess(CollectionAccessType.UpdatedContent)]
    public StringArray(IEnumerable<string> strings)
    {
        _strings = strings.ToArray();
        _stringLengths = new int[_strings.Length];
        _stringStarts = new int[_strings.Length];
        _stringChars = new char[(nuint)_strings.Length << 2];

        FillArray(_strings);
    }

    [Pure]
    public int GetStringLength(int index) => _stringLengths[index];

    [Pure]
    public ReadOnlySpan<char> GetChars(int index) => _stringChars.AsSpan().SliceUnsafe(_stringStarts[index], _stringLengths[index]);

    [Pure]
    public ReadOnlySpan<string> AsSpan() => _strings;

    [Pure]
    public string[] ToArray() => AsSpan().ToArray();

    [Pure]
    public int IndexOf(string str, int startIndex = 0) => IndexOf(str.AsSpan(), startIndex);

    [Pure]
    public int IndexOf(ReadOnlySpan<char> chars, int startIndex = 0)
    {
        int arrayLength = Length;
        ReadOnlySpan<char> charBuffer = _stringChars;
        ref string stringsReference = ref MemoryMarshal.GetArrayDataReference(_strings);
        ref int lengthsReference = ref MemoryMarshal.GetArrayDataReference(_stringLengths);
        ref int startReference = ref MemoryMarshal.GetArrayDataReference(_stringStarts);
        for (int i = startIndex; i < arrayLength; i++)
        {
            int length = Unsafe.Add(ref lengthsReference, i);
            if (length != chars.Length)
            {
                continue;
            }

            ref char charsReference = ref MemoryMarshal.GetReference(chars);
            ref char stringReference = ref MemoryMarshal.GetReference(Unsafe.Add(ref stringsReference, i).AsSpan());
            if (Unsafe.AreSame(ref charsReference, ref stringReference))
            {
                return i;
            }

            int start = Unsafe.Add(ref startReference, i);
            ReadOnlySpan<char> bufferString = charBuffer.SliceUnsafe(start, length);
            if (chars.SequenceEqual(bufferString))
            {
                return i;
            }
        }

        return -1;
    }

    [Pure]
    public int IndexOf(string str, StringComparison comparison, int startIndex = 0) => IndexOf(str.AsSpan(), comparison, startIndex);

    [Pure]
    public int IndexOf(ReadOnlySpan<char> chars, StringComparison comparison, int startIndex = 0)
    {
        int arrayLength = Length;
        ReadOnlySpan<char> charBuffer = _stringChars;
        ref string stringsReference = ref MemoryMarshal.GetArrayDataReference(_strings);
        ref int lengthsReference = ref MemoryMarshal.GetArrayDataReference(_stringLengths);
        ref int startReference = ref MemoryMarshal.GetArrayDataReference(_stringStarts);
        for (int i = startIndex; i < arrayLength; i++)
        {
            int length = Unsafe.Add(ref lengthsReference, i);
            if (length != chars.Length)
            {
                continue;
            }

            ref char charsReference = ref MemoryMarshal.GetReference(chars);
            ref char stringReference = ref MemoryMarshal.GetReference(Unsafe.Add(ref stringsReference, i).AsSpan());
            if (Unsafe.AreSame(ref charsReference, ref stringReference))
            {
                return i;
            }

            int start = Unsafe.Add(ref startReference, i);
            ReadOnlySpan<char> bufferSlice = charBuffer.SliceUnsafe(start, length);
            if (chars.Equals(bufferSlice, comparison))
            {
                return i;
            }
        }

        return -1;
    }

    [Pure]
    public bool Contains(string str) => Contains(str.AsSpan());

    [Pure]
    public bool Contains(ReadOnlySpan<char> chars) => IndexOf(chars) >= 0;

    void ICollection<string>.Add(string str) => throw new NotSupportedException();

    bool ICollection<string>.Remove(string str) => throw new NotSupportedException();

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
        _strings.AsSpan().Clear();
        _stringLengths.AsSpan().Clear();
        _stringStarts.AsSpan().Clear();
        _stringChars.AsSpan().Clear();
    }

    public IEnumerator<string> GetEnumerator()
    {
        foreach (string str in _strings)
        {
            yield return str;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void CopyTo(List<string> destination, int offset = 0)
    {
        DefaultCopier<string> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(string[] destination, int offset)
    {
        DefaultCopier<string> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<string> destination)
    {
        DefaultCopier<string> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public void CopyTo(Span<string> destination)
    {
        DefaultCopier<string> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public void CopyTo(ref string destination)
    {
        DefaultCopier<string> copier = new(AsSpan());
        copier.CopyTo(ref destination);
    }

    public unsafe void CopyTo(string* destination)
    {
        DefaultCopier<string> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    private void SetString(int index, string str)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length, nameof(index));

        int stringLength = str.Length;
        if (stringLength == 0)
        {
            _stringLengths[index] = 0;
            _strings[index] = string.Empty;
            _stringStarts[index] = 0;
            return;
        }

        int currentStringLength = _stringLengths[index];
        int lengthOfLeftStrings = GetStringLengthsSum(..index);
        _strings[index] = str;
        _stringLengths[index] = stringLength;
        _stringStarts[index] = lengthOfLeftStrings;

        if (currentStringLength > stringLength)
        {
            str.CopyTo(_stringChars.AsSpan(lengthOfLeftStrings..));

            Span<char> charsToCopy = _stringChars.AsSpan((lengthOfLeftStrings + currentStringLength)..);
            Span<char> charDestination = _stringChars.AsSpan((lengthOfLeftStrings + stringLength)..);
            charsToCopy.CopyTo(charDestination);
            return;
        }

        if (currentStringLength < stringLength)
        {
            GrowBufferIfNeeded(currentStringLength, stringLength);

            Span<char> charsToCopy = _stringChars.AsSpan((lengthOfLeftStrings + currentStringLength)..(lengthOfLeftStrings + currentStringLength + (_stringChars.Length - (lengthOfLeftStrings + stringLength))));
            Span<char> charDestination = _stringChars.AsSpan((lengthOfLeftStrings + stringLength)..);
            charsToCopy.CopyTo(charDestination);

            str.CopyTo(_stringChars.AsSpan(lengthOfLeftStrings..));
            return;
        }

        str.CopyTo(_stringChars.AsSpan(lengthOfLeftStrings..));
    }

    private void GrowBufferIfNeeded(int currentStringLength, int newStringLength)
    {
        int stringLengthsSum = GetStringLengthsSum(..);
        int freeBufferSpace = _stringChars.Length - stringLengthsSum;
        int neededSpace = newStringLength - currentStringLength;
        if (freeBufferSpace >= neededSpace)
        {
            return;
        }

        nuint newBufferLength = BitOperations.RoundUpToPowerOf2((nuint)(_stringChars.Length + neededSpace));
        char[] newBuffer = new char[newBufferLength];
        _stringChars.CopyTo(newBuffer.AsSpan());
        _stringChars = newBuffer;
    }

    private int GetStringLengthsSum(Range range)
    {
        // TODO: optimize
        int sum = 0;
        ReadOnlySpan<int> stringLengths = _stringLengths.AsSpan(range);
        for (int i = 0; i < stringLengths.Length; i++)
        {
            sum += stringLengths[i];
        }

        return sum;
    }

    public bool Equals(StringArray? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is StringArray other && Equals(other);
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    public static bool operator ==(StringArray? left, StringArray? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(StringArray? left, StringArray? right)
    {
        return !(left == right);
    }
}
