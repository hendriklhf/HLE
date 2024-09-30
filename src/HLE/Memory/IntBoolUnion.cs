using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

[DebuggerDisplay("Integer = {Integer}, Bool = {Bool}")]
public struct IntBoolUnion<T> : IBitwiseEquatable<IntBoolUnion<T>>
    where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T>
{
    public T Integer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get
        {
            EnsureValidIntegerType();
            return _value & GetIntegerMask();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            EnsureValidIntegerType();
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _value = (_value & GetBooleanMask()) | value;
        }
    }

    public unsafe bool Bool
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get
        {
            EnsureValidIntegerType();
            return (_value & GetBooleanMask()) != T.Zero;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            EnsureValidIntegerType();
            T boolValue = value ? T.One : T.Zero;
            _value = (_value & GetIntegerMask()) | (boolValue << ((sizeof(T) << 3) - 1));
        }
    }

    private T _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IntBoolUnion(T integer, bool boolean)
    {
        EnsureValidIntegerType();
        Integer = integer;
        Bool = boolean;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetIntegerUnsafe(T value)
    {
        EnsureValidIntegerType();
        Debug.Assert(value >= T.Zero);
        _value = (_value & GetBooleanMask()) | value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnsureValidIntegerType()
    {
        if (typeof(T) != typeof(sbyte) && typeof(T) != typeof(short) &&
            typeof(T) != typeof(int) && typeof(T) != typeof(long))
        {
            ThrowIntegerTypeNotSupported();
        }

        return;

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowIntegerTypeNotSupported()
            => throw new NotSupportedException($"Only {typeof(sbyte)}, {typeof(short)}, {typeof(int)} and {typeof(long)} are supported.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe T GetIntegerMask()
    {
        switch (sizeof(T))
        {
            case sizeof(sbyte):
                return T.CreateTruncating(0x7F);
            case sizeof(short):
                return T.CreateTruncating(0x7FFF);
            case sizeof(int):
                return T.CreateTruncating(0x7FFF_FFFF);
            case sizeof(long):
                return T.CreateTruncating(0x7FFF_FFFF_FFFF_FFFF);
        }

        ThrowHelper.ThrowUnreachableException();
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe T GetBooleanMask()
    {
        switch (sizeof(T))
        {
            case sizeof(sbyte):
                return T.CreateTruncating(0x80);
            case sizeof(short):
                return T.CreateTruncating(0x8000);
            case sizeof(int):
                return T.CreateTruncating(0x8000_0000);
            case sizeof(long):
                return T.CreateTruncating(0x8000_0000_0000_0000);
        }

        ThrowHelper.ThrowUnreachableException();
        return default;
    }

    [Pure]
    public readonly bool Equals(IntBoolUnion<T> other) => _value == other._value;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is IntBoolUnion<T> other && Equals(other);

    [Pure]
    public override readonly int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(IntBoolUnion<T> left, IntBoolUnion<T> right) => left.Equals(right);

    public static bool operator !=(IntBoolUnion<T> left, IntBoolUnion<T> right) => !(left == right);
}
