using System;
using System.Collections.Generic;
using System.Linq;
using Rand = HLE.Randoms.Random;

namespace HLE.Collections
{
    /// <summary>
    /// A class to help with any kind of collections.
    /// </summary>
    public static class CollectionHelper
    {
        /// <summary>
        /// Return a random element from the <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="T">The type of the <paramref name="collection"/>.</typeparam>
        /// <param name="collection">The collection the random element will be take from.</param>
        /// <returns>A random element.</returns>
        public static T Random<T>(this IEnumerable<T> collection)
        {
            return collection.ElementAt(Rand.Int(0, collection.Count() - 1));
        }

        /// <summary>
        /// Will loop through an <see cref="IEnumerable{T}"/> and performs the given <paramref name="action"/> on each element.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="collection">The <see cref="IEnumerable{T}"/> that will be loop through.</param>
        /// <param name="action">The action that will be performed.</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T item in collection)
            {
                action(item);
            }
        }

        /// <summary>
        /// Checks if the <see cref="IEnumerable{T}"/> is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="collection">The checked collection.</param>
        /// <returns>True, if null or empty, false otherwise.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection is null || !collection.Any();
        }
    }
}
