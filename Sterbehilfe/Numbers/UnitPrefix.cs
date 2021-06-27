using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sterbehilfe.Numbers
{
    public class UnitPrefix
    {
        public string Name { get; }

        public string Symbol { get; }

        public double Value { get; }

        public static readonly ReadOnlyCollection<UnitPrefix> UnitPrefixCollection = new(_unitPrefixes);

        private UnitPrefix(string name, string symbol, double value)
        {
            Name = name;
            Symbol = symbol;
            Value = value;
        }

        public static readonly UnitPrefix Yotta = new("Yotta", "Y", Math.Pow(10, 24));
        public static readonly UnitPrefix Zetta = new("Zetta", "Z", Math.Pow(10, 21));
        public static readonly UnitPrefix Exa = new("Exa", "E", Math.Pow(10, 18));
        public static readonly UnitPrefix Peta = new("Peta", "P", Math.Pow(10, 15));
        public static readonly UnitPrefix Tera = new("Tera", "T", Math.Pow(10, 12));
        public static readonly UnitPrefix Giga = new("Giga", "G", Math.Pow(10, 9));
        public static readonly UnitPrefix Mega = new("Mega", "M", Math.Pow(10, 6));
        public static readonly UnitPrefix Kilo = new("Kilo", "k", Math.Pow(10, 3));
        public static readonly UnitPrefix Hecto = new("Hecto", "h", Math.Pow(10, 2));
        public static readonly UnitPrefix Deca = new("Deca", "da", Math.Pow(10, 1));
        public static readonly UnitPrefix Null = new("", "", Math.Pow(10, 0));
        public static readonly UnitPrefix Deci = new("Deci", "d", Math.Pow(10, -1));
        public static readonly UnitPrefix Centi = new("Centi", "c", Math.Pow(10, -2));
        public static readonly UnitPrefix Milli = new("Milli", "m", Math.Pow(10, -3));
        public static readonly UnitPrefix Micro = new("Micro", "µ", Math.Pow(10, -6));
        public static readonly UnitPrefix Nano = new("Nano", "n", Math.Pow(10, -9));
        public static readonly UnitPrefix Pico = new("Pico", "p", Math.Pow(10, -12));
        public static readonly UnitPrefix Femto = new("Femto", "f", Math.Pow(10, -15));
        public static readonly UnitPrefix Atto = new("Atto", "a", Math.Pow(10, -18));
        public static readonly UnitPrefix Zepto = new("Zepto", "z", Math.Pow(10, -21));
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
    }
}
