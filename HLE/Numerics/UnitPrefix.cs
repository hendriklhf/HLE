using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HLE.Numerics;

/// <summary>
/// A class that represents a unit prefix.
/// </summary>
// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("{Name}")]
public sealed class UnitPrefix : IEquatable<UnitPrefix>
{
    /// <summary>
    /// The representation of the unit prefix Yotta.
    /// </summary>
    public static UnitPrefix Yotta { get; } = new("Yotta", "Y", 1e24);

    /// <summary>
    /// The representation of the unit prefix Zetta.
    /// </summary>
    public static UnitPrefix Zetta { get; } = new("Zetta", "Z", 1e21);

    /// <summary>
    /// The representation of the unit prefix Exa.
    /// </summary>
    public static UnitPrefix Exa { get; } = new("Exa", "E", 1e18);

    /// <summary>
    /// The representation of the unit prefix Peta.
    /// </summary>
    public static UnitPrefix Peta { get; } = new("Peta", "P", 1e15);

    /// <summary>
    /// The representation of the unit prefix Tera.
    /// </summary>
    public static UnitPrefix Tera { get; } = new("Tera", "T", 1e12);

    /// <summary>
    /// The representation of the unit prefix Giga.
    /// </summary>
    public static UnitPrefix Giga { get; } = new("Giga", "G", 1e9);

    /// <summary>
    /// The representation of the unit prefix Mega.
    /// </summary>
    public static UnitPrefix Mega { get; } = new("Mega", "M", 1e6);

    /// <summary>
    /// The representation of the unit prefix Kilo.
    /// </summary>
    public static UnitPrefix Kilo { get; } = new("Kilo", "k", 1e3);

    /// <summary>
    /// The representation of the unit prefix Hecto.
    /// </summary>
    public static UnitPrefix Hecto { get; } = new("Hecto", "h", 1e2);

    /// <summary>
    /// The representation of the unit prefix Deca.
    /// </summary>
    public static UnitPrefix Deca { get; } = new("Deca", "da", 1e1);

    /// <summary>
    /// The representation of no unit prefix.
    /// </summary>
    public static UnitPrefix None { get; } = new(string.Empty, string.Empty, 1e0);

    /// <summary>
    /// The representation of the unit prefix Deci.
    /// </summary>
    public static UnitPrefix Deci { get; } = new("Deci", "d", 1e-1);

    /// <summary>
    /// The representation of the unit prefix Centi.
    /// </summary>
    public static UnitPrefix Centi { get; } = new("Centi", "c", 1e-2);

    /// <summary>
    /// The representation of the unit prefix Milli.
    /// </summary>
    public static UnitPrefix Milli { get; } = new("Milli", "m", 1e-3);

    /// <summary>
    /// The representation of the unit prefix Micro.
    /// </summary>
    public static UnitPrefix Micro { get; } = new("Micro", "µ", 1e-6);

    /// <summary>
    /// The representation of the unit prefix Nano.
    /// </summary>
    public static UnitPrefix Nano { get; } = new("Nano", "n", 1e-9);

    /// <summary>
    /// The representation of the unit prefix Pico.
    /// </summary>
    public static UnitPrefix Pico { get; } = new("Pico", "p", 1e-12);

    /// <summary>
    /// The representation of the unit prefix Femto.
    /// </summary>
    public static UnitPrefix Femto { get; } = new("Femto", "f", 1e-15);

    /// <summary>
    /// The representation of the unit prefix Atto.
    /// </summary>
    public static UnitPrefix Atto { get; } = new("Atto", "a", 1e-18);

    /// <summary>
    /// The representation of the unit prefix Zepto.
    /// </summary>
    public static UnitPrefix Zepto { get; } = new("Zepto", "z", 1e-21);

    /// <summary>
    /// The representation of the unit prefix Yocto.
    /// </summary>
    public static UnitPrefix Yocto { get; } = new("Yocto", "y", 1e-24);

    /// <summary>
    /// The name of the prefix.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The symbol of the prefix.
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// The value of the prefix.
    /// </summary>
    public double Value { get; }

    public UnitPrefix(string name, string symbol, double value)
    {
        Name = name;
        Symbol = symbol;
        Value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Convert(double value, UnitPrefix fromPrefix, UnitPrefix toPrefix)
    {
        return value * (fromPrefix / toPrefix);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Convert(float value, UnitPrefix fromPrefix, UnitPrefix toPrefix)
    {
        return (float)(value * (fromPrefix / toPrefix));
    }

    public static implicit operator double(UnitPrefix prefix)
    {
        return prefix.Value;
    }

    public bool Equals(UnitPrefix? other)
    {
        return Name == other?.Name && Symbol == other.Symbol && Math.Abs(Value - other.Value) == 0;
    }

    public override bool Equals(object? obj)
    {
        return obj is UnitPrefix other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Symbol, Value);
    }

    public static bool operator ==(UnitPrefix? left, UnitPrefix? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(UnitPrefix? left, UnitPrefix? right)
    {
        return !(left == right);
    }
}
