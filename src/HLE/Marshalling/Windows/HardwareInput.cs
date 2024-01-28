using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace HLE.Marshalling.Windows;

[StructLayout(LayoutKind.Explicit)]
public struct HardwareInput : IInput, IEquatable<HardwareInput>
{
    [FieldOffset(4)]
    public uint Msg;

    [FieldOffset(8)]
    public ushort ParamL;

    [FieldOffset(10)]
    public ushort ParamH;

    [FieldOffset(0)]
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly InputType _type = InputType.Hardware;

    public HardwareInput()
    {
    }

    [Pure]
    public readonly bool Equals(HardwareInput other) => Msg == other.Msg && ParamL == other.ParamL && ParamH == other.ParamH;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is HardwareInput other && Equals(other);

    [Pure]
    public override readonly int GetHashCode() => HashCode.Combine(Msg, ParamL, ParamH);

    public static bool operator ==(HardwareInput left, HardwareInput right) => left.Equals(right);

    public static bool operator !=(HardwareInput left, HardwareInput right) => !(left == right);
}
