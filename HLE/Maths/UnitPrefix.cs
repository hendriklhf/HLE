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
    public static UnitPrefix Null { get; } = new(string.Empty, string.Empty, 1e0);

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
