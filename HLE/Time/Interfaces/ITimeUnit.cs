namespace HLE.Time.Interfaces
{
    /// <summary>
    /// An Interface to declare the structure of time units.
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
        /// 
        /// </summary>
        public string Pattern { get; }
    }
}
