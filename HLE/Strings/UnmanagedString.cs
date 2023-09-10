using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Strings;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("\"{String}\"")]
public readonly unsafe struct UnmanagedString : IDisposable, IEquatable<UnmanagedString>, ICountable, IIndexAccessible<char>, ISpanProvider<char>
{
    public ref char this[int index] => ref AsSpan()[index];

    char IIndexAccessible<char>.this[int index] => AsSpan()[index];

    public ref char this[Index index] => ref AsSpan()[index];

    public Span<char> this[Range range] => AsSpan()[range];

    public string String => Length == 0 ? string.Empty : RawDataMarshal.GetObjectFromRawData<string>(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), sizeof(nuint)));

    public int Length { get; }

    int ICountable.Count => Length;

    private readonly byte[] _buffer = Array.Empty<byte>();

    public static UnmanagedString Empty => new();

    public UnmanagedString()
    {
    }

    private UnmanagedString(int length, byte[] buffer)
    {
        _buffer = buffer;
        Length = length;
    }

    [Pure]
    public Span<char> AsSpan()
    {
        ref byte byteReference = ref MemoryMarshal.GetArrayDataReference(_buffer);
        ref char charsReference = ref Unsafe.As<byte, char>(ref Unsafe.Add(ref byteReference, sizeof(nuint) + sizeof(int)));
        return MemoryMarshal.CreateSpan(ref charsReference, Length);
    }

    Span<char> ISpanProvider<char>.GetSpan() => AsSpan();

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(_buffer);
    }

    [Pure]
    public static UnmanagedString Create(int length)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(GetNeededByteCount(length));
        ref byte bufferReference = ref MemoryMarshal.GetArrayDataReference(buffer);
        WriteMetadata(length, ref bufferReference);
        buffer.AsSpan(sizeof(nuint) + sizeof(int), length * sizeof(char)).Clear();
        return new(length, buffer);
    }

    [Pure]
    public static UnmanagedString Create(ReadOnlySpan<char> chars)
    {
        int length = chars.Length;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(GetNeededByteCount(length));
        ref byte bufferReference = ref MemoryMarshal.GetArrayDataReference(buffer);
        WriteMetadata(length, ref bufferReference);
        Span<char> charBuffer = MemoryMarshal.CreateSpan(ref Unsafe.As<byte, char>(ref Unsafe.Add(ref bufferReference, sizeof(nuint) + sizeof(int))), length + 1);
        chars.CopyToUnsafe(charBuffer);
        charBuffer[^1] = '\0';
        return new(length, buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteMetadata(int length, ref byte bufferReference)
    {
        ref nuint methodTableReference = ref Unsafe.As<byte, nuint>(ref bufferReference);
        methodTableReference = RawDataMarshal.GetMethodTablePointer(string.Empty);
        ref int lengthReference = ref Unsafe.As<nuint, int>(ref Unsafe.Add(ref methodTableReference, 1));
        lengthReference = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetNeededByteCount(int length)
    {
        return sizeof(nuint) + sizeof(int) + length * sizeof(char) + sizeof(char);
    }

    public override string ToString()
    {
        return String;
    }

    public bool Equals(UnmanagedString other)
    {
        return String == other.String;
    }

    public override bool Equals(object? obj)
    {
        return obj is UnmanagedString other && Equals(other);
    }

    public override int GetHashCode()
    {
        return String.GetHashCode();
    }

    public static bool operator ==(UnmanagedString left, UnmanagedString right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(UnmanagedString left, UnmanagedString right)
    {
        return !(left == right);
    }
}
