using HLE.Time.Interfaces;

namespace HLE.Time
{
    /// <summary>
    /// A class to do calcutations with the time unit Day.
    /// </summary>
    public class Day : ITimeUnit
    {
        /// <summary>
        /// The amount of days the calculations will be done with.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The given amount of days in milliseconds.
        /// </summary>
        public long Milliseconds => Count * _inMilliseconds;

        /// <summary>
        /// The given amount of days in seconds.
        /// </summary>
        public long Seconds => Milliseconds / 1000;

        /// <summary>
        /// The given amount of days in minutes.
        /// </summary>
        public double Minutes => Seconds / 60;

        /// <summary>
        /// The given amount of days in hours.
        /// </summary>
        public double Hours => Minutes / 60;

        /// <summary>
        /// The given amount of days in days.
        /// </summary>
        public double Days => Hours / 24;

        /// <summary>
        /// The given amount of days in years.
        /// </summary>
        public double Years => Days / 365;

        /// <summary>
        /// A pattern that will match an expression of days in a <see cref="string"/>.
        /// </summary>
        public string Pattern => @"\d+d(ay)?s?";

        /// <summary>
        /// A day in milliseconds.
        /// </summary>
        private const long _inMilliseconds = 86400000;

        /// <summary>
        /// The basic constructor for <see cref="Day"/>.
        /// </summary>
        /// <param name="count">The amount of days.</param>
        public Day(int count = 1)
        {
            Count = count;
        }
    }
}
