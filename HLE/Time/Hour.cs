using HLE.Time.Interfaces;

namespace HLE.Time
{
    /// <summary>
    /// A class to do calculations with the time unit Hour.
    /// </summary>
    public class Hour : ITimeUnit
    {
        /// <summary>
        /// The amount of hours the calculations will be done with.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The given amount of hours in milliseconds.
        /// </summary>
        public long Milliseconds => Count * _inMilliseconds;

        /// <summary>
        /// The given amount of hours in seconds.
        /// </summary>
        public long Seconds => Milliseconds / 1000;

        /// <summary>
        /// The given amount of hours in minutes.
        /// </summary>
        public double Minutes => Seconds / 60;

        /// <summary>
        /// The given amount of hours in hours.
        /// </summary>
        public double Hours => Minutes / 60;

        /// <summary>
        /// The given amount of hours in days.
        /// </summary>
        public double Days => Hours / 24;

        /// <summary>
        /// The given amount of hours in years.
        /// </summary>
        public double Years => Days / 365;

        /// <summary>
        /// A pattern that will match an expression of hours in a <see cref="string"/>.
        /// </summary>
        public string Pattern => @"\d+h(our)?s?";

        /// <summary>
        /// An hour in milliseconds.
        /// </summary>
        private const long _inMilliseconds = 3600000;

        /// <summary>
        /// The basic constructor for <see cref="Hour"/>.
        /// </summary>
        /// <param name="count">The amount of hours.</param>
        public Hour(int count = 1)
        {
            Count = count;
        }
    }
}
