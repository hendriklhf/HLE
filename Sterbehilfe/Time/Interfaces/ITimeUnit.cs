namespace Sterbehilfe.Time.Interfaces
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
        /// Converts the given amount of the specific time unit into milliseconds.
        /// </summary>
        /// <returns>Returns the amount of the time unit in milliseconds.</returns>
        public long ToMilliseconds();

        /// <summary>
        /// Converts the given amount of the specific time unit into seconds.
        /// </summary>
        /// <returns>Returns the amount of the time unit in seconds.</returns>
        public long ToSeconds();
    }
}
