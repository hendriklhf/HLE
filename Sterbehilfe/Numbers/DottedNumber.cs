using Sterbehilfe.Strings;

namespace Sterbehilfe.Numbers
{
    /// <summary>
    /// Creates a number in which every three digits are devided by a dot.<br />
    /// Works with positive and negative values.<br />
    /// For example: 1.465.564
    /// </summary>
    public struct DottedNumber
    {
        /// <summary>
        /// The number with the dots inserted displayed as a <see cref="string"/>.
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// The original number passed to the constructor.
        /// </summary>
        public long OrigninalNumber { get; private set; }

        /// <summary>
        /// The basic constructor of DottedNumber.
        /// </summary>
        /// <param name="number">A number of type <see cref="long"/> in which the dots will be inserted</param>
        public DottedNumber(long number)
        {
            OrigninalNumber = number;
            bool negative = OrigninalNumber < 0;
            string num = negative ? OrigninalNumber.ToString()[1..] : OrigninalNumber.ToString();

            if (num.Length >= 4)
            {
                for (int i = num.Length - 3; i > 0; i -= 3)
                {
                    num = num.Insert(i, ".");
                }
            }

            Number = negative ? $"-{num}" : num;
        }

        /// <summary>
        /// A constructor that takes in a DottedNumber <see cref="string"/>.
        /// </summary>
        /// <param name="number">The DottedNumber <see cref="string"/>.</param>
        public DottedNumber(string number)
        {
            OrigninalNumber = number.Remove(".").ToLong();
            Number = number;
        }

        public static bool operator >(DottedNumber left, DottedNumber right)
        {
            return left.OrigninalNumber > right.OrigninalNumber;
        }

        public static bool operator <(DottedNumber left, DottedNumber right)
        {
            return left.OrigninalNumber < right.OrigninalNumber;
        }

        public static bool operator >=(DottedNumber left, DottedNumber right)
        {
            return left.OrigninalNumber >= right.OrigninalNumber;
        }

        public static bool operator <=(DottedNumber left, DottedNumber right)
        {
            return left.OrigninalNumber <= right.OrigninalNumber;
        }

        public static DottedNumber operator +(DottedNumber left, DottedNumber right)
        {
            return new(left.OrigninalNumber + right.OrigninalNumber);
        }

        public static DottedNumber operator -(DottedNumber left, DottedNumber right)
        {
            return new(left.OrigninalNumber - right.OrigninalNumber);
        }

        public static DottedNumber operator *(DottedNumber left, DottedNumber right)
        {
            return new(left.OrigninalNumber * right.OrigninalNumber);
        }

        public static DottedNumber operator /(DottedNumber left, DottedNumber right)
        {
            return new(left.OrigninalNumber / right.OrigninalNumber);
        }

        public static DottedNumber operator ++(DottedNumber dottedNumber)
        {
            return new(dottedNumber.OrigninalNumber + 1);
        }

        public static DottedNumber operator --(DottedNumber dottedNumber)
        {
            return new(dottedNumber.OrigninalNumber - 1);
        }

        public static DottedNumber operator !(DottedNumber dottedNumber)
        {
            return new DottedNumber(-dottedNumber.OrigninalNumber);
        }

        public static implicit operator long(DottedNumber dottedNumber)
        {
            return dottedNumber.OrigninalNumber;
        }

        public static implicit operator DottedNumber(long l)
        {
            return new DottedNumber(l);
        }

        public override string ToString()
        {
            return Number;
        }
    }
}
