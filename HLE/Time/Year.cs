using HLE.Time.Interfaces;

namespace HLE.Time
{
    /// <summary>
    /// A class that represents the time unit "Year".
    /// </summary>
    public class Year : ITimeUnit
    {
        /// <summary>
        /// The amount of years passed to the constructor.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The given amount of years in milliseconds.
        /// </summary>
        public long Milliseconds => Count * _inMilliseconds;

        /// <summary>
        /// The given amount of years in seconds.
        /// </summary>
        public long Seconds => Milliseconds / 1000;


        /// <summary>
        /// The given amount of years in minutes.
        /// </summary>
        public double Minutes => Seconds / 60;

        /// <summary>
        /// The given amount of years in hours.
        /// </summary>
        public double Hours => Minutes / 60;

        /// <summary>
        /// The given amount of years in days.
        /// </summary>
        public double Days => Hours / 24;

        /// <summary>
        /// The given amount of years in years.
        /// </summary>
        public double Years => Days / 365;

        /// <summary>
        /// A Regex pattern that matches an amount of years in a <see cref="string"/>.
        /// </summary>
        public const string Pattern = @"\d+y(ear)?s?";

        /// <summary>
        /// A year in milliseconds.
        /// </summary>
        private const long _inMilliseconds = 31556952000;

        /// <summary>
        /// The basic constructor of <see cref="Year"/>.
        /// </summary>
        /// <param name="count">The amount of years.</param>
        public Year(int count = 1)
        {
            Count = count;
        }
    }
}
