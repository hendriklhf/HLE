using System;

namespace Sterbehilfe.Numbers
{
    public struct PrefixNumber
    {
        public double Number { get; }

        public UnitPrefix UnitPrefix { get; }

        public double Value => Number * UnitPrefix.Value;

        public PrefixNumber(double number, UnitPrefix unitPrefix)
        {
            Number = number;
            UnitPrefix = unitPrefix;
        }

        public PrefixNumber(double number) : this(number, UnitPrefix.Null)
        {
        }

        public static bool operator >(PrefixNumber left, PrefixNumber right)
        {
            return left.Value > right.Value;
        }

        public static bool operator <(PrefixNumber left, PrefixNumber right)
        {
            return left.Value < right.Value;
        }

        public static bool operator >=(PrefixNumber left, PrefixNumber right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <=(PrefixNumber left, PrefixNumber right)
        {
            return left.Value <= right.Value;
        }

        //public static PrefixNumber operator +(PrefixNumber left, PrefixNumber right)
        //{

        //}

        //public static PrefixNumber operator -(PrefixNumber left, PrefixNumber right)
        //{

        //}

        //public static PrefixNumber operator *(PrefixNumber left, PrefixNumber right)
        //{

        //}

        //public static PrefixNumber operator /(PrefixNumber left, PrefixNumber right)
        //{

        //}

        public static PrefixNumber operator ++(PrefixNumber prefixNumber)
        {
            return new(prefixNumber.Number + 1, prefixNumber.UnitPrefix);
        }

        public static PrefixNumber operator --(PrefixNumber prefixNumber)
        {
            return new(prefixNumber.Number - 1, prefixNumber.UnitPrefix);
        }

        public static PrefixNumber operator !(PrefixNumber prefixNumber)
        {
            return new(-prefixNumber.Number, prefixNumber.UnitPrefix);
        }

        public static implicit operator PrefixNumber(long l)
        {
            return new(l);
        }

        public static implicit operator PrefixNumber(ValueTuple<long, UnitPrefix> vT)
        {
            return new(vT.Item1, vT.Item2);
        }

        public static implicit operator PrefixNumber(double d)
        {
            return new(d);
        }

        public static implicit operator PrefixNumber(ValueTuple<double, UnitPrefix> vT)
        {
            return new(vT.Item1, vT.Item2);
        }

        public bool Equals(PrefixNumber prefixNumber)
        {
            return Value == prefixNumber.Value;
        }

        public override string ToString()
        {
            return $"{Number} {UnitPrefix.Symbol}";
        }
    }
}
