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
        /// The amount of hours converted to milliseconds.
        /// </summary>
        public long Milliseconds => Count * _inMilliseconds;

        /// <summary>
        /// The amount of days converted to seconds.
        /// </summary>
        public long Seconds => Milliseconds / 1000;

        /// <summary>
        /// A pattern that will match an expression of hours in a <see cref="string"/>.
        /// </summary>
        public string Pattern => @"\d+h(our)?s?";

        public double Minutes => Seconds / 60;

        public double Hours => Minutes / 60;

        public double Days => Hours / 24;

        public double Years => Days / 365;

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