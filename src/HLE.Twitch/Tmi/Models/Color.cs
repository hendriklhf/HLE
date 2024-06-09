using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Text;

namespace HLE.Twitch.Tmi.Models;

[DebuggerDisplay("{ToString()}")]
[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "ConvertToAutoProperty")]
[SuppressMessage("Roslynator", "RCS1085:Use auto-implemented property")]
[SuppressMessage("Style", "IDE0032:Use auto property")]
public readonly struct Color : IBitwiseEquatable<Color>
{
    public byte Red => _red;

    public byte Green => _green;

    public byte Blue => _blue;

    public bool IsEmpty => _empty;

    public static Color Empty => new();

    private readonly byte _blue;
    private readonly byte _green;
    private readonly byte _red;
    private readonly bool _empty;

    public Color() => _empty = true;

    public Color(byte red, byte green, byte blue)
    {
        _red = red;
        _green = green;
        _blue = blue;
    }

    public Color(System.Drawing.Color color)
    {
        if (color.IsEmpty)
        {
            _empty = true;
            return;
        }

        _red = color.R;
        _green = color.G;
        _blue = color.B;
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

        using ValueStringBuilder builder = new(stackalloc char[7]);
        builder.Append('#');
        builder.Append(Red, "X2");
        builder.Append(Green, "X2");
        builder.Append(Blue, "X2");
        return StringPool.Shared.GetOrAdd(builder.WrittenSpan);
    }

    public static bool operator ==(Color left, Color right) => left.Equals(right);

    public static bool operator !=(Color left, Color right) => !(left == right);
}
