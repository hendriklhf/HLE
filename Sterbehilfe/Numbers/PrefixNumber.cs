using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sterbehilfe.Numbers
{
    public struct PrefixNumber
    {
        public double Number { get; }

        public UnitPrefix UnitPrefix { get; }

        public string Unit { get; }

        public double Value => Number * UnitPrefix.Value;

        public PrefixNumber(double number, UnitPrefix unitPrefix, string unit)
        {
            Number = number;
            UnitPrefix = unitPrefix;
            Unit = unit;
        }

        public PrefixNumber(double number, UnitPrefix unitPrefix) : this(number, unitPrefix, "")
        {
        }

        public PrefixNumber(double number, string unit) : this(number, UnitPrefix.Null, unit)
        {
        }

        public PrefixNumber(double number) : this(number, UnitPrefix.Null, "")
        {
        }

        public static bool operator >(PrefixNumber left, PrefixNumber right)
        {
            return left.Number * left.UnitPrefix.Value > right.Number * right.UnitPrefix.Value;
        }

        public static bool operator <(PrefixNumber left, PrefixNumber right)
        {
            return left.Number * left.UnitPrefix.Value < right.Number * right.UnitPrefix.Value;
        }

        public static bool operator >=(PrefixNumber left, PrefixNumber right)
        {
            return left.Number * left.UnitPrefix.Value >= right.Number * right.UnitPrefix.Value;
        }

        public static bool operator <=(PrefixNumber left, PrefixNumber right)
        {
            return left.Number * left.UnitPrefix.Value <= right.Number * right.UnitPrefix.Value;
        }

        public static PrefixNumber operator +(PrefixNumber left, PrefixNumber right)
        {
            
        }

        public static PrefixNumber operator -(PrefixNumber left, PrefixNumber right)
        {

        }

        public static PrefixNumber operator *(PrefixNumber left, PrefixNumber right)
        {

        }

        public static PrefixNumber operator /(PrefixNumber left, PrefixNumber right)
        {

        }

        public static PrefixNumber operator ++(PrefixNumber prefixNumber)
        {

        }

        public static PrefixNumber operator --(PrefixNumber prefixNumber)
        {

        }

        public static PrefixNumber operator !(PrefixNumber prefixNumber)
        {

        }

        public bool Equals(PrefixNumber prefixNumber)
        {
            return Number * UnitPrefix.Value == prefixNumber.Number * prefixNumber.UnitPrefix.Value;
        }

        public override string ToString()
        {
            return $"{Number} {UnitPrefix.Symbol}{Unit}".Trim();
        }
    }
}
