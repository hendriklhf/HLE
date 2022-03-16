using System.Collections.Generic;
using System.Linq;
using System.Text;
using HLE.Collections;

namespace HLE.Random
{
    /// <summary>
    /// A static class that contains all sorts of random methods.
    /// </summary>
    public static class Random
    {
        /// <summary>
        /// Returns a random <see cref="char"/> out of all basic latin characters.
        /// </summary>
        /// <returns>A basic Latin character.</returns>
        public static char Char()
        {
            return CharCollection.BasicLatinChars.Random();
        }

        public static char Char(ushort min, ushort max)
        {
            List<long> numbers = NumberCollection.Create(min, max).ToList();
            return (char)numbers.Random();
        }

        /// <summary>
        /// Returns a random <see cref="int"/> between the given borders.<br />
        /// Default values are <see cref="int.MinValue"/> and <see cref="int.MaxValue"/>.<br />
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value</param>
        /// <returns>A random <see cref="int"/>.</returns>
        public static int Int(int min = int.MinValue, int max = int.MaxValue)
        {
            if (min > max)
            {
                (max, min) = (min, max);
            }

            if (max < int.MaxValue)
            {
                max++;
            }

            return new System.Random().Next(min, max);
        }

        /// <summary>
        /// Returns a <see cref="string"/> of the given <paramref name="length"/> filled with basic Latin characters.<br />
        /// Calls <see cref="Char()"/> to fill the result string.
        /// </summary>
        /// <param name="length">The <paramref name="length"/> of the <see cref="string"/>.</param>
        /// <returns>A string of the given <paramref name="length"/>.</returns>
        public static string String(ulong length = 10)
        {
            if (length <= 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new();
            for (ulong i = 0; i < length; i++)
            {
                builder.Append(Char());
            }

            return builder.ToString();
        }

        public static bool Bool()
        {
            return Int(0, 1) switch
            {
                0 => true,
                1 => false,
                _ => true
            };
        }
    }
}
