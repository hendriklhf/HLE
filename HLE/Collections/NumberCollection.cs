using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HLE.Collections
{
    /// <summary>
    /// A class containing collections of numbers.
    /// </summary>
    public static class NumberCollection
    {
        private static readonly List<int> _everyNumber = new() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        /// <summary>
        /// A <see cref="ReadOnlyCollection{Int32}"/> of type <see cref="int"/> that contains every number from 0 to 9.
        /// </summary>
        public static readonly ReadOnlyCollection<int> Numbers = new(_everyNumber);
    }

}
