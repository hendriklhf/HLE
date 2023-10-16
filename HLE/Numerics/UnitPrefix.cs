using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Numerics;

/// <summary>
/// A struct that represents a unit prefix.
/// </summary>
// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("{Name}")]
public readonly struct UnitPrefix(string name, string symbol, double value)
    : IEquatable<UnitPrefix>
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

    #region Static UnitPrefixes

    /// <summary>
    /// The representation of the unit prefix Yotta.
    /// </summary>
    public static ref readonly UnitPrefix Yotta => ref _yotta;

    /// <summary>
    /// The representation of the unit prefix Zetta.
    /// </summary>
    public static ref readonly UnitPrefix Zetta => ref _zetta;

    /// <summary>
    /// The representation of the unit prefix Exa.
    /// </summary>
    public static ref readonly UnitPrefix Exa => ref _exa;

    /// <summary>
    /// The representation of the unit prefix Peta.
    /// </summary>
    public static ref readonly UnitPrefix Peta => ref _peta;

    /// <summary>
    /// The representation of the unit prefix Tera.
    /// </summary>
    public static ref readonly UnitPrefix Tera => ref _tera;

    /// <summary>
    /// The representation of the unit prefix Giga.
    /// </summary>
    public static ref readonly UnitPrefix Giga => ref _giga;

    /// <summary>
    /// The representation of the unit prefix Mega.
    /// </summary>
    public static ref readonly UnitPrefix Mega => ref _mega;

    /// <summary>
    /// The representation of the unit prefix Kilo.
    /// </summary>
    public static ref readonly UnitPrefix Kilo => ref _kilo;

    /// <summary>
    /// The representation of the unit prefix Hecto.
    /// </summary>
    public static ref readonly UnitPrefix Hecto => ref _hecto;

    /// <summary>
    /// The representation of the unit prefix Deca.
    /// </summary>
    public static ref readonly UnitPrefix Deca => ref _deca;

    /// <summary>
    /// The representation of no unit prefix.
    /// </summary>
    public static ref readonly UnitPrefix None => ref _none;

    /// <summary>
    /// The representation of the unit prefix Deci.
    /// </summary>
    public static ref readonly UnitPrefix Deci => ref _deci;

    /// <summary>
    /// The representation of the unit prefix Centi.
    /// </summary>
    public static ref readonly UnitPrefix Centi => ref _centi;

    /// <summary>
    /// The representation of the unit prefix Milli.
    /// </summary>
    public static ref readonly UnitPrefix Milli => ref _milli;

    /// <summary>
    /// The representation of the unit prefix Micro.
    /// </summary>
    public static ref readonly UnitPrefix Micro => ref _micro;

    /// <summary>
    /// The representation of the unit prefix Nano.
    /// </summary>
    public static ref readonly UnitPrefix Nano => ref _nano;

    /// <summary>
    /// The representation of the unit prefix Pico.
    /// </summary>
    public static ref readonly UnitPrefix Pico => ref _pico;

    /// <summary>
    /// The representation of the unit prefix Femto.
    /// </summary>
    public static ref readonly UnitPrefix Femto => ref _femto;

    /// <summary>
    /// The representation of the unit prefix Atto.
    /// </summary>
    public static ref readonly UnitPrefix Atto => ref _atto;

    /// <summary>
    /// The representation of the unit prefix Zepto.
    /// </summary>
    public static ref readonly UnitPrefix Zepto => ref _zepto;

    /// <summary>
    /// The representation of the unit prefix Yocto.
    /// </summary>
    public static ref readonly UnitPrefix Yocto => ref _yocto;

    private static readonly UnitPrefix _yotta = new("Yotta", "Y", 1e24);
    private static readonly UnitPrefix _zetta = new("Zetta", "Z", 1e21);
    private static readonly UnitPrefix _exa = new("Exa", "E", 1e18);
    private static readonly UnitPrefix _peta = new("Peta", "P", 1e15);
    private static readonly UnitPrefix _tera = new("Tera", "T", 1e12);
    private static readonly UnitPrefix _giga = new("Giga", "G", 1e9);
    private static readonly UnitPrefix _mega = new("Mega", "M", 1e6);
    private static readonly UnitPrefix _kilo = new("Kilo", "k", 1e3);
    private static readonly UnitPrefix _hecto = new("Hecto", "h", 1e2);
    private static readonly UnitPrefix _deca = new("Deca", "da", 1e1);
    private static readonly UnitPrefix _none = new(string.Empty, string.Empty, 1e0);
    private static readonly UnitPrefix _deci = new("Deci", "d", 1e-1);
    private static readonly UnitPrefix _centi = new("Centi", "c", 1e-2);
    private static readonly UnitPrefix _milli = new("Milli", "m", 1e-3);
    private static readonly UnitPrefix _micro = new("Micro", "µ", 1e-6);
    private static readonly UnitPrefix _nano = new("Nano", "n", 1e-9);
    private static readonly UnitPrefix _pico = new("Pico", "p", 1e-12);
    private static readonly UnitPrefix _femto = new("Femto", "f", 1e-15);
    private static readonly UnitPrefix _atto = new("Atto", "a", 1e-18);
    private static readonly UnitPrefix _zepto = new("Zepto", "z", 1e-21);
    private static readonly UnitPrefix _yocto = new("Yocto", "y", 1e-24);

    #endregion Static UnitPrefixes

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Convert(double value, UnitPrefix fromPrefix, UnitPrefix toPrefix)
        => value * (fromPrefix / toPrefix);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Convert(float value, UnitPrefix fromPrefix, UnitPrefix toPrefix)
        => (float)Convert((double)value, fromPrefix, toPrefix);

    [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
    public static implicit operator double(UnitPrefix? prefix) => prefix?.Value ?? 0;

    [Pure]
    public override string ToString() => Name;

    [Pure]
    public bool Equals(double value) => Math.Abs(Value - value) == 0;

    [Pure]
    public bool Equals(UnitPrefix other) => Name == other.Name && Symbol == other.Symbol && Math.Abs(Value - other.Value) == 0;

    [Pure]
    public override bool Equals(object? obj) => obj is UnitPrefix other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Name, Symbol, Value);

    public static bool operator ==(UnitPrefix left, UnitPrefix right) => Equals(left, right);

    public static bool operator !=(UnitPrefix left, UnitPrefix right) => !(left == right);
}
