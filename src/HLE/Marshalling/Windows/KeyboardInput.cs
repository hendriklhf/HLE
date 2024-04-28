using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace HLE.Marshalling.Windows;

[StructLayout(LayoutKind.Explicit)]
[SuppressMessage("Critical Code Smell", "S4000:Pointers to unmanaged memory should not be visible")]
public unsafe struct KeyboardInput : IInput, IEquatable<KeyboardInput>
{
    [FieldOffset(4)]
    public VirtualKey Key;

    [FieldOffset(6)]
    public char Scan;

    [FieldOffset(8)]
    public KeyboardInputFlags Flags;

    [FieldOffset(12)]
    public uint Time;

    [FieldOffset(16)]
    public uint* ExtraInfo;

    [FieldOffset(0)]
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly InputType _type = InputType.Keyboard;

    public KeyboardInput()
    {
    }

    [Pure]
    public readonly bool Equals(KeyboardInput other)
        => Key == other.Key && Scan == other.Scan && Flags == other.Flags &&
           Time == other.Time && ExtraInfo == other.ExtraInfo;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is KeyboardInput other && Equals(other);

    [Pure]
    public override readonly int GetHashCode() => HashCode.Combine(Key, Scan, Flags, Time, (nuint)ExtraInfo);

    public static bool operator ==(KeyboardInput left, KeyboardInput right) => left.Equals(right);

    public static bool operator !=(KeyboardInput left, KeyboardInput right) => !(left == right);
}
