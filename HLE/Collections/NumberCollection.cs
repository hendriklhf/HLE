using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HLE.Collections
{
    /// <summary>
    /// A class containing collections of numbers.
    /// </summary>
    public static class NumberCollection
    {
        private static readonly byte[] _everyNumber =
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9
        };

        /// <summary>
        /// A <see cref="IEnumerable{Byte}"/> of type <see cref="byte"/> that contains every number from 0 to 9.
        /// </summary>
        public static IEnumerable<byte> Numbers => _everyNumber;

        /// <summary>
        /// Creates a <see cref="IEnumerable{Int64}"/> of type <see cref="long"/> that contains every number from <paramref name="min"/> to <paramref name="max"/>.
        /// </summary>
        /// <param name="min">The lower boundary.</param>
        /// <param name="max">The upper boundary</param>
        /// <returns>A <see cref="IEnumerable{Int64}"/> containing the numbers.</returns>
        public static IEnumerable<long> Create(long min, long max)
        {
            List<long> result = new();
            if (min > max)
            {
                long tmp = min;
                max = min;
                min = tmp;
            }
            
            for (long i = min; i <= max; i++)
            {
                result.Add(i);
            }

            return result;
        }

        public static IEnumerable<ulong> Create(ulong min, ulong max)
        {
            List<ulong> result = new();
            if (min > max)
            {
                ulong tmp = min;
                max = min;
                min = tmp;
            }

            for (ulong i = min; i <= max; i++)
            {
                result.Add(i);
            }

            return result;
        }
    }
}
