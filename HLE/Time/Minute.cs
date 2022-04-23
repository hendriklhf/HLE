using HLE.Time.Interfaces;

namespace HLE.Time
{
    /// <summary>
    /// A class to do calcutations with the time unit Minute.
    /// </summary>
    public class Minute : ITimeUnit
    {
        /// <summary>
        /// The amount of minutes the calculations will be done with.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The given amount of minutes in milliseconds.
        /// </summary>
        public long Milliseconds => Count * _inMilliseconds;

        /// <summary>
        /// The given amount of minutes in seconds.
        /// </summary>
        public long Seconds => Milliseconds / 1000;

        /// <summary>
        /// The given amount of minutes in minutes.
        /// </summary>
        public double Minutes => Seconds / 60;

        /// <summary>
        /// The given amount of minutes in hours.
        /// </summary>
        public double Hours => Minutes / 60;

        /// <summary>
        /// The given amount of minutes in days.
        /// </summary>
        public double Days => Hours / 24;

        /// <summary>
        /// The given amount of minutes in years.
        /// </summary>
        public double Years => Days / 365;

        /// <summary>
        /// A pattern that will match an expression of minutes in a <see cref="string"/>.
        /// </summary>
        public const string Pattern = @"\d+m(in(ute)?)?s?";

        /// <summary>
        /// A minute in milliseconds.
        /// </summary>
        private const long _inMilliseconds = 60000;

        /// <summary>
        /// The basic constructor for <see cref="Minute"/>.
        /// </summary>
        /// <param name="count">The amount of minutes.</param>
        public Minute(int count = 1)
        {
            Count = count;
        }
    }
}
