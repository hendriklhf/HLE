using System;
using System.Collections.Generic;
using System.Linq;

namespace HLE.Enums
{
    /// <summary>
    /// A class to help with any kind of <see cref="Enum"/>.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Creates an <see cref="Array"/> of all enum values.
        /// </summary>
        /// <returns>Returns an <see cref="Array"/> of all enum values.</returns>
        public static T[] ToArray<T>(this Type enumType) where T : Enum
        {
            return (T[])Enum.GetValues(enumType);
        }

        /// <summary>
        /// Creates a <see cref="List{T}"/> of all enum values.
        /// </summary>
        /// <returns>Returns a <see cref="List{T}"/> of all enum values.</returns>
        public static List<T> ToList<T>(this Type enumType) where T : Enum
        {
            return ((T[])Enum.GetValues(enumType)).ToList();
        }
    }
}
