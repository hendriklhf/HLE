using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using HLE.Marshalling;

namespace HLE.Memory.Blocks;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public readonly struct Block16 : IBitwiseEquatable<Block16>
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Block16 other) => Equals(ref other);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ref Block16 other)
    {
        Vector128<byte> self = Vector128.LoadUnsafe(ref Unsafe.As<Block16, byte>(ref Unsafe.AsRef(in this)));
        Vector128<byte> otherAsByte = Vector128.LoadUnsafe(ref Unsafe.As<Block16, byte>(ref other));
        return self == otherAsByte;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Block16 other && Equals(other);

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
    public static bool operator ==(Block16 left, Block16 right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Block16 left, Block16 right) => !(left == right);
}
