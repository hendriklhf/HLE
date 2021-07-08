using System;
using System.Linq;

namespace Sterbehilfe.Numbers
{
    public struct PrefixNumber
    {
        public double Number { get; private set; }

        public UnitPrefix UnitPrefix { get; private set; }

        public double Value => Number * UnitPrefix.Value;

        public PrefixNumber(double number, UnitPrefix unitPrefix)
        {
            Number = number;
            UnitPrefix = unitPrefix;
        }

        public PrefixNumber(double number, bool setPrefixAutomatically = true)
        {
            if (setPrefixAutomatically)
            {
                UnitPrefix = UnitPrefix.UnitPrefixCollection.OrderBy(up => Math.Abs(1 - (number / up.Value))).FirstOrDefault();
                Number = number / UnitPrefix.Value;
            }
            else
            {
                Number = number;
                UnitPrefix = UnitPrefix.Null;
            }
        }

        public void SetUnitPrefix()
        {
            double n = Number;
            UnitPrefix = UnitPrefix.UnitPrefixCollection.OrderBy(up => Math.Abs(1 - (n / up.Value))).FirstOrDefault();
            Number /= UnitPrefix.Value;
        }

        public static bool operator ==(PrefixNumber left, PrefixNumber right)
        {
            return left.Value == right.Value;
        }

        public static bool operator !=(PrefixNumber left, PrefixNumber right)
        {
            return !(left == right);
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

        public static PrefixNumber operator +(PrefixNumber left, PrefixNumber right)
        {
            return new(left.Value + right.Value);
        }

        public static PrefixNumber operator -(PrefixNumber left, PrefixNumber right)
        {
            return new(left.Value - right.Value);
        }

        public static PrefixNumber operator *(PrefixNumber left, PrefixNumber right)
        {
            return new(left.Value * right.Value);
        }

        public static PrefixNumber operator /(PrefixNumber left, PrefixNumber right)
        {
            return new(left.Value / right.Value);
        }

        public static PrefixNumber operator ++(PrefixNumber prefixNumber)
        {
            return new(prefixNumber.Value + 1);
        }

        public static PrefixNumber operator --(PrefixNumber prefixNumber)
        {
            return new(prefixNumber.Value - 1);
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

        public bool Equals(long number)
        {
            return Value == number;
        }

        public bool Equals(double number)
        {
            return Value == number;
        }

        public override string ToString()
        {
            return $"{Number}{UnitPrefix.Symbol}";
        }
    }
}
