using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using HLE.Marshalling;

namespace HLE.Memory.Blocks;

[StructLayout(LayoutKind.Explicit, Size = 32)]
public readonly struct Block32 : IBitwiseEquatable<Block32>
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Block32 other) => Equals(ref other);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ref Block32 other)
    {
        Vector256<byte> self = Vector256.LoadUnsafe(ref Unsafe.As<Block32, byte>(ref Unsafe.AsRef(in this)));
        Vector256<byte> otherAsByte = Vector256.LoadUnsafe(ref Unsafe.As<Block32, byte>(ref other));
        return self == otherAsByte;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Block32 other && Equals(other);

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
    public static bool operator ==(Block32 left, Block32 right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Block32 left, Block32 right) => !(left == right);
}
