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
        /// The amount of minutes converted to milliseconds.
        /// </summary>
        public long Milliseconds => Count * _inMilliseconds;

        /// <summary>
        /// The amount of days converted to seconds.
        /// </summary>
        public long Seconds => Milliseconds / 1000;

        /// <summary>
        /// A pattern that will match an expression of minutes in a <see cref="string"/>.
        /// </summary>
        public string Pattern => @"\d+m(in(ute)?)?s?";

        public double Minutes => Seconds / 60;

        public double Hours => Minutes / 60;

        public double Days => Hours / 24;

        public double Years => Days / 365;

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