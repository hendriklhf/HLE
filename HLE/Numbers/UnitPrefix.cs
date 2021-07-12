using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        public static readonly UnitPrefix Yotta = new("Yotta", "Y", Math.Pow(10, 24));
        /// <summary>
        /// The representation of the unit prefix Zetta.
        /// </summary>
        public static readonly UnitPrefix Zetta = new("Zetta", "Z", Math.Pow(10, 21));
        /// <summary>
        /// The representation of the unit prefix Exa.
        /// </summary>
        public static readonly UnitPrefix Exa = new("Exa", "E", Math.Pow(10, 18));
        /// <summary>
        /// The representation of the unit prefix Peta.
        /// </summary>
        public static readonly UnitPrefix Peta = new("Peta", "P", Math.Pow(10, 15));
        /// <summary>
        /// The representation of the unit prefix Tera.
        /// </summary>
        public static readonly UnitPrefix Tera = new("Tera", "T", Math.Pow(10, 12));
        /// <summary>
        /// The representation of the unit prefix Giga.
        /// </summary>
        public static readonly UnitPrefix Giga = new("Giga", "G", Math.Pow(10, 9));
        /// <summary>
        /// The representation of the unit prefix Mega.
        /// </summary>
        public static readonly UnitPrefix Mega = new("Mega", "M", Math.Pow(10, 6));
        /// <summary>
        /// The representation of the unit prefix Kilo.
        /// </summary>
        public static readonly UnitPrefix Kilo = new("Kilo", "k", Math.Pow(10, 3));
        /// <summary>
        /// The representation of the unit prefix Hecto.
        /// </summary>
        public static readonly UnitPrefix Hecto = new("Hecto", "h", Math.Pow(10, 2));
        /// <summary>
        /// The representation of the unit prefix Deca.
        /// </summary>
        public static readonly UnitPrefix Deca = new("Deca", "da", Math.Pow(10, 1));
        /// <summary>
        /// The representation of no unit prefix. 
        /// </summary>
        public static readonly UnitPrefix Null = new("", "", Math.Pow(10, 0));
        /// <summary>
        /// The representation of the unit prefix Deci.
        /// </summary>
        public static readonly UnitPrefix Deci = new("Deci", "d", Math.Pow(10, -1));
        /// <summary>
        /// The representation of the unit prefix Centi.
        /// </summary>
        public static readonly UnitPrefix Centi = new("Centi", "c", Math.Pow(10, -2));
        /// <summary>
        /// The representation of the unit prefix Milli.
        /// </summary>
        public static readonly UnitPrefix Milli = new("Milli", "m", Math.Pow(10, -3));
        /// <summary>
        /// The representation of the unit prefix Micro.
        /// </summary>
        public static readonly UnitPrefix Micro = new("Micro", "µ", Math.Pow(10, -6));
        /// <summary>
        /// The representation of the unit prefix Nano.
        /// </summary>
        public static readonly UnitPrefix Nano = new("Nano", "n", Math.Pow(10, -9));
        /// <summary>
        /// The representation of the unit prefix Pico.
        /// </summary>
        public static readonly UnitPrefix Pico = new("Pico", "p", Math.Pow(10, -12));
        /// <summary>
        /// The representation of the unit prefix Femto.
        /// </summary>
        public static readonly UnitPrefix Femto = new("Femto", "f", Math.Pow(10, -15));
        /// <summary>
        /// The representation of the unit prefix Atto.
        /// </summary>
        public static readonly UnitPrefix Atto = new("Atto", "a", Math.Pow(10, -18));
        /// <summary>
        /// The representation of the unit prefix Zepto.
        /// </summary>
        public static readonly UnitPrefix Zepto = new("Zepto", "z", Math.Pow(10, -21));
        /// <summary>
        /// The representation of the unit prefix Yocto.
        /// </summary>
        public static readonly UnitPrefix Yocto = new("Yocto", "y", Math.Pow(10, -24));

        private static readonly List<UnitPrefix> _unitPrefixes = new()
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
        /// A <see cref="ReadOnlyCollection{UnitPrefix}"/> that contains every unit prefix.
        /// </summary>
        public static readonly ReadOnlyCollection<UnitPrefix> UnitPrefixCollection = new(_unitPrefixes);
    }
}
