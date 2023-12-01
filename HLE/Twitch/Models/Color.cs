using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Strings;

namespace HLE.Twitch.Models;

[DebuggerDisplay("{ToString()}")]
public readonly struct Color : IBitwiseEquatable<Color>
{
    public byte Red { get; }

    public byte Green { get; }

    public byte Blue { get; }

    public bool IsEmpty { get; }

    public static Color Empty => new();

    public Color() => IsEmpty = true;

    public Color(byte red, byte green, byte blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    public Color(System.Drawing.Color color)
    {
        if (color.IsEmpty)
        {
            IsEmpty = true;
            return;
        }

        Red = color.R;
        Green = color.G;
        Blue = color.B;
    }

#pragma warning disable CA2225
    public static implicit operator Color(System.Drawing.Color color) => new(color);

    public static implicit operator System.Drawing.Color(Color color)
        => color.IsEmpty ? System.Drawing.Color.Empty : System.Drawing.Color.FromArgb(0xFF, color.Red, color.Green, color.Blue);
#pragma warning restore CA2225

    [Pure]
    public bool Equals(Color other) => StructMarshal.EqualsBitwise(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Color other && Equals(other);

    [Pure]
    public override unsafe int GetHashCode()
    {
        Debug.Assert(sizeof(Color) == sizeof(int));
        fixed (Color* self = &this)
        {
            return *(int*)self;
        }
    }

    public override string ToString()
    {
        if (IsEmpty)
        {
            return string.Empty;
        }

        ValueStringBuilder builder = new(stackalloc char[7]);
        builder.Append('#');
        builder.Append(Red, "X2");
        builder.Append(Green, "X2");
        builder.Append(Blue, "X2");
        return StringPool.Shared.GetOrAdd(builder.WrittenSpan);
    }

    public static bool operator ==(Color left, Color right) => left.Equals(right);

    public static bool operator !=(Color left, Color right) => !(left == right);
}
