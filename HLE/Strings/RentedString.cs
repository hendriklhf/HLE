using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

public readonly unsafe struct RentedString : IDisposable, ICollection<char>, ICopyable<char>, IRefIndexAccessible<char>, ICountable
{
    public ref char this[int index] => ref Chars[index];

    public ref char this[Index index] => ref Chars[index];

    public Span<char> this[Range range] => Chars[range];

    public Span<char> Chars
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            char* chars = (char*)(_buffer.Pointer + sizeof(nuint) + sizeof(int));
            return new(chars, Length);
        }
    }

    public int Length => *(int*)(_buffer.Pointer + sizeof(nuint));

    int ICollection<char>.Count => Length;

    int ICountable.Count => Length;

    bool ICollection<char>.IsReadOnly => false;

    private readonly RentedArray<byte> _buffer;

    public static RentedString Empty => new();

    public RentedString()
    {
        _buffer = new(16);
        StoreString(ref _buffer.Reference, 0);
    }

    public RentedString(int length)
    {
        _buffer = new(sizeof(nuint) + sizeof(int) + (length << 1));
        StoreString(ref _buffer.Reference, length);
    }

    public RentedString(ReadOnlySpan<char> chars) : this(chars.Length)
    {
        MemoryHelper.CopyUnsafe(chars, Chars);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetString()
    {
        return MemoryHelper.GetReferenceFromRawDataPointer<string>(_buffer.Pointer)!;
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }

    public void CopyTo(List<char> destination, int offset = 0)
    {
        DefaultCopyableCopier<char> copier = new(Chars);
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(char[] destination, int offset)
    {
        DefaultCopyableCopier<char> copier = new(Chars);
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<char> destination)
    {
        DefaultCopyableCopier<char> copier = new(Chars);
        copier.CopyTo(destination);
    }

    public void CopyTo(Span<char> destination)
    {
        DefaultCopyableCopier<char> copier = new(Chars);
        copier.CopyTo(destination);
    }

    public void CopyTo(ref char destination)
    {
        DefaultCopyableCopier<char> copier = new(Chars);
        copier.CopyTo(ref destination);
    }

    public void CopyTo(char* destination)
    {
        DefaultCopyableCopier<char> copier = new(Chars);
        copier.CopyTo(destination);
    }

    void ICollection<char>.Add(char item)
    {
        throw new NotSupportedException();
    }

    void ICollection<char>.Clear()
    {
        Chars.Clear();
    }

    bool ICollection<char>.Remove(char item)
    {
        throw new NotSupportedException();
    }

    bool ICollection<char>.Contains(char item)
    {
        return Chars.Contains(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void StoreString(ref byte bufferReference, int stringLength)
    {
        Unsafe.As<byte, nuint>(ref bufferReference) = GetStringMethodTablePointer();
        SetStringLength(ref bufferReference, stringLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetStringLength(ref byte bufferReference, int stringLength)
    {
        ref int lengthReference = ref Unsafe.As<byte, int>(ref Unsafe.Add(ref bufferReference, sizeof(nuint)));
        lengthReference = stringLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nuint GetStringMethodTablePointer()
    {
        return *(nuint*)MemoryHelper.GetRawDataPointer(string.Empty);
    }

    /// <summary>
    /// Creates a safe <see cref="string"/> from the chars in the <see cref="RentedString"/>.
    /// </summary>
    /// <returns>A safe string.</returns>
    public override string ToString()
    {
        return new(Chars);
    }

    public IEnumerator<char> GetEnumerator()
    {
        int length = Length;
        for (int i = 0; i < length; i++)
        {
            yield return Chars[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Equals(RentedString other)
    {
        return Equals(other.GetString());
    }

    public bool Equals(string str)
    {
        return GetString() == str;
    }

    public override bool Equals(object? obj)
    {
        return obj is RentedString other && Equals(other);
    }

    public override int GetHashCode()
    {
        return ((nint)_buffer.Pointer).GetHashCode();
    }

    public static bool operator ==(RentedString left, RentedString right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RentedString left, RentedString right)
    {
        return !(left == right);
    }
}
