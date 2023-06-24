using System;
using System.Diagnostics;

namespace HLE.Twitch.Models;

[DebuggerDisplay("{ToString()}")]
public readonly struct Color : IEquatable<Color>
{
    public byte Red { get; }

    public byte Green { get; }

    public byte Blue { get; }

    public bool IsEmpty { get; }

    public static Color Empty => new();

    public Color()
    {
        IsEmpty = true;
    }

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

    public static implicit operator Color(System.Drawing.Color color)
    {
        return new(color);
    }

    public static implicit operator System.Drawing.Color(Color color)
    {
        return color.IsEmpty ? System.Drawing.Color.Empty : System.Drawing.Color.FromArgb(0xFF, color.Red, color.Green, color.Blue);
    }

    public bool Equals(Color other)
    {
        return IsEmpty && other.IsEmpty && Red == other.Red && Green == other.Green && Blue == other.Blue;
    }

    public override bool Equals(object? obj)
    {
        return obj is Color other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsEmpty, Red, Green, Blue);
    }

    public override string ToString()
    {
        return IsEmpty ? string.Empty : $"#{Red:X}{Green:X}{Blue:X}";
    }

    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Color left, Color right)
    {
        return !(left == right);
    }
}
