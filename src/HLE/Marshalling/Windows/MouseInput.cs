using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace HLE.Marshalling.Windows;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct MouseInput : IInput, IEquatable<MouseInput>
{
    [FieldOffset(4)]
    public int X;

    [FieldOffset(8)]
    public int Y;

    [FieldOffset(12)]
    public uint MouseData;

    [FieldOffset(16)]
    public MouseInputFlags Flags;

    [FieldOffset(20)]
    public uint Time;

    [FieldOffset(24)]
    public uint* ExtraInfo;

    [FieldOffset(0)]
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly InputType _type = InputType.Mouse;

    public const uint WheelDelta = 120;

    public MouseInput()
    {
    }

    [Pure]
    public readonly bool Equals(MouseInput other)
        => X == other.X && Y == other.Y && MouseData == other.MouseData && Flags == other.Flags &&
           Time == other.Time && ExtraInfo == other.ExtraInfo;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is MouseInput other && Equals(other);

    [Pure]
    public override readonly int GetHashCode() => HashCode.Combine(X, Y, MouseData, Flags, Time, (nuint)ExtraInfo);

    public static bool operator ==(MouseInput left, MouseInput right) => left.Equals(right);

    public static bool operator !=(MouseInput left, MouseInput right) => !(left == right);
}
