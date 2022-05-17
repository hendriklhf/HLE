using System;
using System.Collections.Generic;

namespace HLE.Numbers
{
    /// <summary>
    /// A class that represents a unit prefix.
    /// </summary>
    public class UnitPrefix
    {
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

        /// <summary>
        /// The representation of the unit prefix Yotta.
        /// </summary>
        public static UnitPrefix Yotta { get; } = new("Yotta", "Y", Math.Pow(10, 24));

        /// <summary>
        /// The representation of the unit prefix Zetta.
        /// </summary>
        public static UnitPrefix Zetta { get; } = new("Zetta", "Z", Math.Pow(10, 21));

        /// <summary>
        /// The representation of the unit prefix Exa.
        /// </summary>
        public static UnitPrefix Exa { get; } = new("Exa", "E", Math.Pow(10, 18));

        /// <summary>
        /// The representation of the unit prefix Peta.
        /// </summary>
        public static UnitPrefix Peta { get; } = new("Peta", "P", Math.Pow(10, 15));

        /// <summary>
        /// The representation of the unit prefix Tera.
        /// </summary>
        public static UnitPrefix Tera { get; } = new("Tera", "T", Math.Pow(10, 12));

        /// <summary>
        /// The representation of the unit prefix Giga.
        /// </summary>
        public static UnitPrefix Giga { get; } = new("Giga", "G", Math.Pow(10, 9));

        /// <summary>
        /// The representation of the unit prefix Mega.
        /// </summary>
        public static UnitPrefix Mega { get; } = new("Mega", "M", Math.Pow(10, 6));

        /// <summary>
        /// The representation of the unit prefix Kilo.
        /// </summary>
        public static UnitPrefix Kilo { get; } = new("Kilo", "k", Math.Pow(10, 3));

        /// <summary>
        /// The representation of the unit prefix Hecto.
        /// </summary>
        public static UnitPrefix Hecto { get; } = new("Hecto", "h", Math.Pow(10, 2));

        /// <summary>
        /// The representation of the unit prefix Deca.
        /// </summary>
        public static UnitPrefix Deca { get; } = new("Deca", "da", Math.Pow(10, 1));

        /// <summary>
        /// The representation of no unit prefix. 
        /// </summary>
        public static UnitPrefix Null { get; } = new("", "", Math.Pow(10, 0));

        /// <summary>
        /// The representation of the unit prefix Deci.
        /// </summary>
        public static UnitPrefix Deci { get; } = new("Deci", "d", Math.Pow(10, -1));

        /// <summary>
        /// The representation of the unit prefix Centi.
        /// </summary>
        public static UnitPrefix Centi { get; } = new("Centi", "c", Math.Pow(10, -2));

        /// <summary>
        /// The representation of the unit prefix Milli.
        /// </summary>
        public static UnitPrefix Milli { get; } = new("Milli", "m", Math.Pow(10, -3));

        /// <summary>
        /// The representation of the unit prefix Micro.
        /// </summary>
        public static UnitPrefix Micro { get; } = new("Micro", "µ", Math.Pow(10, -6));

        /// <summary>
        /// The representation of the unit prefix Nano.
        /// </summary>
        public static UnitPrefix Nano { get; } = new("Nano", "n", Math.Pow(10, -9));

        /// <summary>
        /// The representation of the unit prefix Pico.
        /// </summary>
        public static UnitPrefix Pico { get; } = new("Pico", "p", Math.Pow(10, -12));

        /// <summary>
        /// The representation of the unit prefix Femto.
        /// </summary>
        public static UnitPrefix Femto { get; } = new("Femto", "f", Math.Pow(10, -15));

        /// <summary>
        /// The representation of the unit prefix Atto.
        /// </summary>
        public static UnitPrefix Atto { get; } = new("Atto", "a", Math.Pow(10, -18));

        /// <summary>
        /// The representation of the unit prefix Zepto.
        /// </summary>
        public static UnitPrefix Zepto { get; } = new("Zepto", "z", Math.Pow(10, -21));

        /// <summary>
        /// The representation of the unit prefix Yocto.
        /// </summary>
        public static UnitPrefix Yocto { get; } = new("Yocto", "y", Math.Pow(10, -24));

        private static readonly UnitPrefix[] _unitPrefixes =
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
        /// A <see cref="IEnumerable{UnitPrefix}"/> that contains every unit prefix.
        /// </summary>
        public static IEnumerable<UnitPrefix> UnitPrefixCollection => _unitPrefixes;
    }
}
