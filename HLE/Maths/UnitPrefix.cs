using System.Collections.Generic;

namespace HLE.Maths;

/// <summary>
/// A class that represents a unit prefix.
/// </summary>
public sealed class UnitPrefix
{
    /// <summary>
    /// The representation of the unit prefix Yotta.
    /// </summary>
    public static UnitPrefix Yotta { get; } = new("Yotta", "Y", 10e24);

    /// <summary>
    /// The representation of the unit prefix Zetta.
    /// </summary>
    public static UnitPrefix Zetta { get; } = new("Zetta", "Z", 10e21);

    /// <summary>
    /// The representation of the unit prefix Exa.
    /// </summary>
    public static UnitPrefix Exa { get; } = new("Exa", "E", 10e18);

    /// <summary>
    /// The representation of the unit prefix Peta.
    /// </summary>
    public static UnitPrefix Peta { get; } = new("Peta", "P", 10e15);

    /// <summary>
    /// The representation of the unit prefix Tera.
    /// </summary>
    public static UnitPrefix Tera { get; } = new("Tera", "T", 10e12);

    /// <summary>
    /// The representation of the unit prefix Giga.
    /// </summary>
    public static UnitPrefix Giga { get; } = new("Giga", "G", 10e9);

    /// <summary>
    /// The representation of the unit prefix Mega.
    /// </summary>
    public static UnitPrefix Mega { get; } = new("Mega", "M", 10e6);

    /// <summary>
    /// The representation of the unit prefix Kilo.
    /// </summary>
    public static UnitPrefix Kilo { get; } = new("Kilo", "k", 10e3);

    /// <summary>
    /// The representation of the unit prefix Hecto.
    /// </summary>
    public static UnitPrefix Hecto { get; } = new("Hecto", "h", 10e2);

    /// <summary>
    /// The representation of the unit prefix Deca.
    /// </summary>
    public static UnitPrefix Deca { get; } = new("Deca", "da", 10e1);

    /// <summary>
    /// The representation of no unit prefix.
    /// </summary>
    public static UnitPrefix Null { get; } = new("", "", 10e0);

    /// <summary>
    /// The representation of the unit prefix Deci.
    /// </summary>
    public static UnitPrefix Deci { get; } = new("Deci", "d", 10e-1);

    /// <summary>
    /// The representation of the unit prefix Centi.
    /// </summary>
    public static UnitPrefix Centi { get; } = new("Centi", "c", 10e-2);

    /// <summary>
    /// The representation of the unit prefix Milli.
    /// </summary>
    public static UnitPrefix Milli { get; } = new("Milli", "m", 10e-3);

    /// <summary>
    /// The representation of the unit prefix Micro.
    /// </summary>
    public static UnitPrefix Micro { get; } = new("Micro", "µ", 10e-6);

    /// <summary>
    /// The representation of the unit prefix Nano.
    /// </summary>
    public static UnitPrefix Nano { get; } = new("Nano", "n", 10e-9);

    /// <summary>
    /// The representation of the unit prefix Pico.
    /// </summary>
    public static UnitPrefix Pico { get; } = new("Pico", "p", 10e-12);

    /// <summary>
    /// The representation of the unit prefix Femto.
    /// </summary>
    public static UnitPrefix Femto { get; } = new("Femto", "f", 10e-15);

    /// <summary>
    /// The representation of the unit prefix Atto.
    /// </summary>
    public static UnitPrefix Atto { get; } = new("Atto", "a", 10e-18);

    /// <summary>
    /// The representation of the unit prefix Zepto.
    /// </summary>
    public static UnitPrefix Zepto { get; } = new("Zepto", "z", 10e-21);

    /// <summary>
    /// The representation of the unit prefix Yocto.
    /// </summary>
    public static UnitPrefix Yocto { get; } = new("Yocto", "y", 10e-24);

    /// <summary>
    /// A <see cref="IEnumerable{UnitPrefix}"/> that contains every unit prefix.
    /// </summary>
    public static UnitPrefix[] UnitPrefixCollection { get; } =
    {
        Yotta,
        Zetta,
        Exa,
        Peta,
        Tera,
        Giga,
        Mega,
        Kilo,
        Hecto,
        Deca,
        Null,
        Deci,
        Centi,
        Milli,
        Micro,
        Nano,
        Pico,
        Femto,
        Atto,
        Zepto,
        Yocto
    };

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

    private UnitPrefix(string name, string symbol, double value)
    {
        Name = name;
        Symbol = symbol;
        Value = value;
    }

    public static implicit operator double(UnitPrefix prefix)
    {
        return prefix.Value;
    }
}
