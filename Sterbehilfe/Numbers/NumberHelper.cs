namespace Sterbehilfe.Numbers
{
    /// <summary>
    /// A class to help with any kind of number.
    /// </summary>
    public static class NumberHelper
    {
        /// <summary>
        /// Converts an <see cref="int"/> to a <see cref="double"/>.
        /// </summary>
        /// <param name="i">The <see cref="int"/> that will be converted.</param>
        /// <returns>Returns <paramref name="i"/> converted to a <see cref="double"/>.</returns>
        public static double ToDouble(this int i)
        {
            return i;
        }

        /// <summary>
        /// Converts a <see cref="long"/> to a <see cref="double"/>.
        /// </summary>
        /// <param name="l">The <see cref="long"/> that will be converted.</param>
        /// <returns>Returns <paramref name="l"/> converted to a <see cref="double"/>.</returns>
        public static double ToDouble(this long l)
        {
            return l;
        }

        /// <summary>
        /// Converts a <see cref="double"/> to a <see cref="long"/>.<br />
        /// The decimal places will be disposed.
        /// </summary>
        /// <param name="d">The <see cref="double"/> that will be converted.</param>
        /// <returns>Returns <paramref name="d"/> converted to a <see cref="long"/>.</returns>
        public static long ToLong(this double d)
        {
            return (long)d;
        }

        /// <summary>
        /// Converts a <see cref="long"/> to a <see cref="DottedNumber"/>.
        /// </summary>
        /// <param name="number">The <see cref="long"/> that wil be converted.</param>
        /// <returns>Returns <paramref name="number"/> converted to a <see cref="DottedNumber"/>.</returns>
        public static string ToDottedNumber(this long number)
        {
            return new DottedNumber(number).Number;
        }
    }
}