using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Numerics;

/// <summary>
/// A struct that represents a unit prefix.
/// </summary>
/// <param name="name">The name of the prefix.</param>
/// <param name="symbol">The symbol or abbreviation of the prefix.</param>
/// <param name="value">The value of the prefix.</param>
// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("{Name}")]
public sealed class UnitPrefix(string name, string symbol, double value) :
    IEquatable<UnitPrefix>,
    IComparable<UnitPrefix>,
    IComparable
{
    /// <summary>
    /// The name of the prefix.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The symbol of the prefix.
    /// </summary>
    public string Symbol { get; } = symbol;

    /// <summary>
    /// The value of the prefix.
    /// </summary>
    public double Value { get; } = value;

    internal readonly decimal _dValue = (decimal)value;
    internal readonly float _fValue = (float)value;

    #region Static UnitPrefixes

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

    #endregion Static UnitPrefixes

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Convert(double value, UnitPrefix fromPrefix, UnitPrefix toPrefix)
        => value * (fromPrefix.Value / toPrefix.Value);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Convert(decimal value, UnitPrefix fromPrefix, UnitPrefix toPrefix)
        => value * (fromPrefix._dValue / toPrefix._dValue);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Convert(float value, UnitPrefix fromPrefix, UnitPrefix toPrefix)
        => value * (fromPrefix._fValue / toPrefix._fValue);

    [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
    public static implicit operator double(UnitPrefix prefix) => prefix.Value;

    [Pure]
    public override string ToString() => Name;

    [Pure]
    public bool Equals(double value) => Math.Abs(Value - value) <= 0.0001;

    [Pure]
    public bool Equals([NotNullWhen(true)] UnitPrefix? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public int CompareTo(UnitPrefix? other) => Value.CompareTo(other?.Value ?? 0);

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            UnitPrefix other => CompareTo(other),
            _ => ThrowInvalidArgumentType(nameof(obj))
        };

        [DoesNotReturn]
        static int ThrowInvalidArgumentType(string paramName) => throw new ArgumentException("Invalid argument type.", paramName);
    }

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Name, Symbol, Value);

    public static bool operator ==(UnitPrefix left, UnitPrefix right) => left.Equals(right);

    public static bool operator !=(UnitPrefix left, UnitPrefix right) => !(left == right);

    public static bool operator <(UnitPrefix left, UnitPrefix right) => left.CompareTo(right) < 0;

    public static bool operator <=(UnitPrefix left, UnitPrefix right) => left.CompareTo(right) <= 0;

    public static bool operator >(UnitPrefix left, UnitPrefix right) => left.CompareTo(right) > 0;

    public static bool operator >=(UnitPrefix left, UnitPrefix right) => left.CompareTo(right) >= 0;
}
