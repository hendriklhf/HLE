#pragma warning disable CS0661, CS0659, CS1591

using System;
using System.Linq;

namespace HLE.Numbers
{
    /// <summary>
    /// Represents a number with a unit prefix (<see cref="Numbers.UnitPrefix"/>).
    /// </summary>
    public struct PrefixNumber
    {
        /// <summary>
        /// The original number passed to the constructor.
        /// </summary>
        public double Number { get; private set; }

        /// <summary>
        /// The unit prefix that can be assigned manually or automatically.
        /// </summary>
        public UnitPrefix UnitPrefix { get; private set; }

        /// <summary>
        /// The value calculated with <see cref="Number"/> and <see cref="UnitPrefix"/>.
        /// </summary>
        public double Value => Number * UnitPrefix.Value;

        /// <summary>
        /// The constructor to set the prefix manually with.
        /// </summary>
        /// <param name="number">The original number.</param>
        /// <param name="unitPrefix">The unit prefix.</param>
        public PrefixNumber(double number, UnitPrefix unitPrefix)
        {
            Number = number;
            UnitPrefix = unitPrefix;
        }

        /// <summary>
        /// The constructor to set the prefix automatically with.<br />
        /// If <paramref name="setPrefixAutomatically"/> is false, 
        /// the default <see cref="Numbers.UnitPrefix"/> (<see cref="Numbers.UnitPrefix.Null"/>) will be assigned.
        /// </summary>
        /// <param name="number">The original number.</param>
        /// <param name="setPrefixAutomatically">Decides whether the <see cref="UnitPrefix"/> will be assigned automatically or not.</param>
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

        /// <summary>
        /// Will set <see cref="UnitPrefix"/> to the best fitting one.
        /// </summary>
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

        public static bool operator ==(PrefixNumber left, double right)
        {
            return left.Value == right;
        }

        public static bool operator !=(PrefixNumber left, PrefixNumber right)
        {
            return !(left == right);
        }

        public static bool operator !=(PrefixNumber left, double right)
        {
            return !(left.Value == right);
        }

        public static bool operator >(PrefixNumber left, PrefixNumber right)
        {
            return left.Value > right.Value;
        }

        public static bool operator >(PrefixNumber left, double right)
        {
            return left.Value > right;
        }

        public static bool operator <(PrefixNumber left, PrefixNumber right)
        {
            return left.Value < right.Value;
        }

        public static bool operator <(PrefixNumber left, double right)
        {
            return left.Value < right;
        }

        public static bool operator >=(PrefixNumber left, PrefixNumber right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator >=(PrefixNumber left, double right)
        {
            return left.Value >= right;
        }

        public static bool operator <=(PrefixNumber left, PrefixNumber right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator <=(PrefixNumber left, double right)
        {
            return left.Value <= right;
        }

        public static PrefixNumber operator +(PrefixNumber left, PrefixNumber right)
        {
            return new(left.Value + right.Value);
        }

        public static PrefixNumber operator +(PrefixNumber left, double right)
        {
            return new(left.Value + right);
        }

        public static PrefixNumber operator -(PrefixNumber left, PrefixNumber right)
        {
            return new(left.Value - right.Value);
        }

        public static PrefixNumber operator -(PrefixNumber left, double right)
        {
            return new(left.Value - right);
        }

        public static PrefixNumber operator *(PrefixNumber left, PrefixNumber right)
        {
            return new(left.Value * right.Value);
        }

        public static PrefixNumber operator *(PrefixNumber left, double right)
        {
            return new(left.Value * right);
        }

        public static PrefixNumber operator /(PrefixNumber left, PrefixNumber right)
        {
            return new(left.Value / right.Value);
        }

        public static PrefixNumber operator /(PrefixNumber left, double right)
        {
            return new(left.Value / right);
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

        public override bool Equals(object obj)
        {
            return obj is PrefixNumber p && this == p;
        }

        public override string ToString()
        {
            return $"{Number}{UnitPrefix.Symbol}";
        }
    }
}
