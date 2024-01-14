using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using HLE.Marshalling;

namespace HLE.Memory.Blocks;

[StructLayout(LayoutKind.Explicit, Size = 64)]
public readonly struct Block64 : IBitwiseEquatable<Block64>
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Block64 other) => Equals(ref other);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ref Block64 other)
    {
        Vector512<byte> self = Vector512.LoadUnsafe(ref Unsafe.As<Block64, byte>(ref Unsafe.AsRef(in this)));
        Vector512<byte> otherAsByte = Vector512.LoadUnsafe(ref Unsafe.As<Block64, byte>(ref other));
        return self == otherAsByte;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Block64 other && Equals(other);

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
    public static bool operator ==(Block64 left, Block64 right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Block64 left, Block64 right) => !(left == right);
}
