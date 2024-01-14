using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;

namespace HLE.Memory.Blocks;

[StructLayout(LayoutKind.Explicit, Size = 128)]
public readonly struct Block128 : IBitwiseEquatable<Block128>
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Block128 other) => Equals(ref other);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ref Block128 other)
    {
        ref Block64 self64 = ref Unsafe.As<Block128, Block64>(ref Unsafe.AsRef(in this));
        ref Block64 other64 = ref Unsafe.As<Block128, Block64>(ref other);
        return self64 == other64 && Unsafe.Add(ref self64, 1) == Unsafe.Add(ref other64, 1);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Block128 other && Equals(other);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        ReadOnlySpan<byte> bytes = StructMarshal.GetBytes(ref Unsafe.AsRef(in this));
        HashCode hash = new();
        hash.AddBytes(bytes);
        return hash.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Block128 left, Block128 right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Block128 left, Block128 right) => !(left == right);
}
