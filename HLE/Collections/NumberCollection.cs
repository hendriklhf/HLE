using System.Collections.Generic;

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

        public static IEnumerable<byte> Create(byte min = byte.MinValue, byte max = byte.MaxValue)
        {
            List<byte> result = new();
            if (min > max)
            {
                byte tmp = min;
                max = min;
                min = tmp;
            }

            for (byte i = min; i <= max; i++)
            {
                result.Add(i);
            }

            return result;
        }

        public static IEnumerable<sbyte> Create(sbyte min = sbyte.MinValue, sbyte max = sbyte.MaxValue)
        {
            List<sbyte> result = new();
            if (min > max)
            {
                sbyte tmp = min;
                max = min;
                min = tmp;
            }

            for (sbyte i = min; i <= max; i++)
            {
                result.Add(i);
            }

            return result;
        }

        public static IEnumerable<short> Create(short min = short.MinValue, short max = short.MaxValue)
        {
            List<short> result = new();
            if (min > max)
            {
                short tmp = min;
                max = min;
                min = tmp;
            }

            for (short i = min; i <= max; i++)
            {
                result.Add(i);
            }

            return result;
        }

        public static IEnumerable<ushort> Create(ushort min = ushort.MinValue, ushort max = ushort.MaxValue)
        {
            List<ushort> result = new();
            if (min > max)
            {
                ushort tmp = min;
                max = min;
                min = tmp;
            }

            for (ushort i = min; i <= max; i++)
            {
                result.Add(i);
            }

            return result;
        }

        public static IEnumerable<int> Create(int min = int.MinValue, int max = int.MaxValue)
        {
            List<int> result = new();
            if (min > max)
            {
                int tmp = min;
                max = min;
                min = tmp;
            }

            for (int i = min; i <= max; i++)
            {
                result.Add(i);
            }

            return result;
        }

        public static IEnumerable<uint> Create(uint min = uint.MinValue, uint max = uint.MaxValue)
        {
            List<uint> result = new();
            if (min > max)
            {
                uint tmp = min;
                max = min;
                min = tmp;
            }

            for (uint i = min; i <= max; i++)
            {
                result.Add(i);
            }

            return result;
        }

        /// <summary>
        /// Creates a <see cref="IEnumerable{Int64}"/> of type <see cref="long"/> that contains every number from <paramref name="min"/> to <paramref name="max"/>.
        /// </summary>
        /// <param name="min">The lower boundary.</param>
        /// <param name="max">The upper boundary</param>
        /// <returns>A <see cref="IEnumerable{Int64}"/> containing the numbers.</returns>
        public static IEnumerable<long> Create(long min = long.MinValue, long max = long.MaxValue)
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

        public static IEnumerable<ulong> Create(ulong min = ulong.MinValue, ulong max = ulong.MaxValue)
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
