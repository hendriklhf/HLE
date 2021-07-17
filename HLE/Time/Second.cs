using HLE.Time.Interfaces;

namespace HLE.Time
{
    /// <summary>
    /// A class to do calcutations with the time unit Second.
    /// </summary>
    public class Second : ITimeUnit
    {
        /// <summary>
        /// The amount of seconds the calculations will be done with.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The given amount of seconds in milliseconds.
        /// </summary>
        public long Milliseconds => Count * _inMilliseconds;

        /// <summary>
        /// The given amount of days in seconds.
        /// </summary>
        public long Seconds => Milliseconds / 1000;

        /// <summary>
        /// The given amount of seconds in minutes.
        /// </summary>
        public double Minutes => Seconds / 60;

        /// <summary>
        /// The given amount of seconds in hours.
        /// </summary>
        public double Hours => Minutes / 60;

        /// <summary>
        /// The given amount of seconds in days.
        /// </summary>
        public double Days => Hours / 24;

        /// <summary>
        /// The given amount of seconds in years.
        /// </summary>
        public double Years => Days / 365;

        /// <summary>
        /// A second in milliseconds.
        /// </summary>
        private const long _inMilliseconds = 1000;

        /// <summary>
        /// A pattern that will match an expression of seconds in a <see cref="string"/>.
        /// </summary>
        public string Pattern => @"\d+s(ec(ond)?)?s?";

        /// <summary>
        /// The basic constructor for <see cref="Second"/>.
        /// </summary>
        /// <param name="count">The amount of seconds.</param>
        public Second(int count = 1)
        {
            Count = count;
        }
    }
}