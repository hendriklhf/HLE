#pragma warning disable CA1069

namespace Sterbehilfe.Time.Enums
{
    /// <summary>
    /// An enum to use different conversion types in <see cref="TimeHelper.ConvertMillisecondsToPassedTime(long, string, ConversionType)"/>.<br />
    /// To, for example, dispose the minutes and seconds, in order to shorten the result <see cref="string"/> of the method.
    /// </summary>
    public enum ConversionType
    {
        /// <summary>
        /// Displays the result in years, days and hours, but will still use smaller units if the passed time is less than an hour.
        /// </summary>
        YearDayHour,
        /// <summary>
        /// Displays the result in years, days, hours, and minuts but will still use smaller units if the passed time is less than an hour.
        /// </summary>
        YearDayHourMin,
        /// <summary>
        /// Displays the result in years, days, hours, minutes and seconds, using every available time unit. A copy of <see cref="All"/>.
        /// </summary>
        YearDayHourMinSec,
        /// <summary>
        /// Displays the result in years, days, hours, minutes and seconds, using every available time unit. A copy of <see cref="YearDayHourMinSec"/>.
        /// </summary>
        All = 2,
    }
}
