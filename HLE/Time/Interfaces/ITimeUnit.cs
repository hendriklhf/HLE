namespace HLE.Time.Interfaces
{
    /// <summary>
    /// An interface to declare the structure of time units.
    /// </summary>
    public interface ITimeUnit
    {
        /// <summary>
        /// The amount of the specific time unit that will converted into other units.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The amount of the specific time unit in milliseconds.
        /// </summary>
        public long Milliseconds { get; }

        /// <summary>
        /// The amount of the specific time unit in seconds.
        /// </summary>
        public long Seconds { get; }

        /// <summary>
        /// The amount of the specific time unit in minutes.
        /// </summary>
        public double Minutes { get; }

        /// <summary>
        /// The amount of the specific time unit in hours.
        /// </summary>
        public double Hours { get; }

        /// <summary>
        /// The amount of the specific time unit in days.
        /// </summary>
        public double Days { get; }

        /// <summary>
        /// The amount of the specific time unit in years.
        /// </summary>
        public double Years { get; }

        /// <summary>
        /// A pattern that will match an expression of the specific time unit in a <see cref="string"/>.
        /// </summary>
        public string Pattern { get; }
    }
}
