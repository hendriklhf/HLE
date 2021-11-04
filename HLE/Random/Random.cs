using System.Collections.Generic;
using System.Linq;
using System.Text;
using HLE.Collections;
using HLE.HttpRequests;

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
                int tmp = min;
                min = max;
                max = tmp;
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

        /// <summary>
        /// Returns the given <paramref name="amount"/> of random words retrieved from two online word lists of over 100k words.<br />
        /// The parameter will be fitted to be working with the amount of words in the two list.
        /// </summary>
        /// <param name="amount">The amount of words</param>
        /// <returns>A <see cref="List{String}"/> containing the given <paramref name="amount"/> of random words</returns>
        public static IEnumerable<string> Word(int amount = 1)
        {
            List<string> words = new();
            HttpGet request1 = new("https://thewordcounter.com/wp-content/themes/sage/resources/inc/base_dictionary2.txt");
            HttpGet request2 = new("https://capitalizemytitle.com/wp-content/tools/random-word/en/words%20alpha.txt");
            List<string> result = new();
            words = words
                .Concat(request1.Result.Split("\n"))
                .Concat(request2.Result.Split(","))
                .Distinct()
                .ToList();
            if (amount < 1)
            {
                amount = 1;
            }
            if (amount > words.Count - 1)
            {
                amount = words.Count - 1;
            }
            for (int i = 0; i <= amount; i++)
            {
                result.Add(words.Random());
            }
            return result;
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
