using HLE.Time.Interfaces;

namespace HLE.Time
{
    /// <summary>
    /// A class that represents the time unit "Week".
    /// </summary>
    public class Week : ITimeUnit
    {
        /// <summary>
        /// The amount of weeks passed to the constructor.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The given amount of weeks in milloseconds.
        /// </summary>
        public long Milliseconds => new Day(7 * Count).Milliseconds;

        /// <summary>
        /// The given amount of weeks in seconds.
        /// </summary>
        public long Seconds => Milliseconds / 1000;

        /// <summary>
        /// A Regex pattern that matches an amount of weeks in a <see cref="string"/>.
        /// </summary>
        public string Pattern => @"\d+w(eek)?s?";

        /// <summary>
        /// The basic constructor for <see cref="Week"/>.
        /// </summary>
        /// <param name="count">The amount of weeks.</param>
        public Week(int count = 1)
        {
            Count = count;
        }
    }
}
