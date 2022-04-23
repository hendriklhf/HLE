using System;
using System.Text;
using HLE.Numbers;
using HLE.Time.Enums;

namespace HLE.Time;

/// <summary>
/// A class to help with any kind of time.
/// </summary>
public static class TimeHelper
{
    /// <summary>
    /// Converts the a Unix time stamp into a time stamp with time units. The Unix time stamp can be in the future or past.<br />
    /// For example: "1h, 24min, 56s"
    /// </summary>
    /// <param name="unixTime">The point of time as a Unix time stamp.</param>
    /// <param name="conversionType">A <see cref="ConversionType"/> to shorten the <see cref="string"/> of time to, for example, only show the hours and minutes.</param>
    /// <returns>The time stamp result as a <see cref="string"/>.</returns>
    [Obsolete("Use GetUnixDifference(long) instead.")]
    public static string ConvertUnixTimeToTimeStamp(long unixTime, ConversionType conversionType = ConversionType.All)
    {
        long now = Now();
        if (unixTime > now)
        {
            unixTime -= (unixTime - now) * 2;
        }

        if (now - unixTime < 1000)
        {
            return $"{now - unixTime}ms";
        }

        StringBuilder builder = new();
        unixTime = now - unixTime;
        if (Math.Truncate((unixTime / new Year().Milliseconds).ToDouble()) > 0)
        {
            builder.Append(Math.Truncate((unixTime / new Year().Milliseconds).ToDouble()).ToString(), "y, ");
            unixTime -= (Math.Truncate((unixTime / new Year().Milliseconds).ToDouble()) * new Year().Milliseconds).ToLong();
            if (Math.Truncate((unixTime / new Day().Milliseconds).ToDouble()) > 0)
            {
                builder.Append(Math.Truncate((unixTime / new Day().Milliseconds).ToDouble()).ToString(), "d, ");
                unixTime -= (Math.Truncate((unixTime / new Day().Milliseconds).ToDouble()) * new Day().Milliseconds).ToLong();
                if (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) > 0)
                {
                    builder.Append(Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()).ToString(), "h, ");
                    if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                    {
                        unixTime -= (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
                        if (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) > 0)
                        {
                            builder.Append(Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()).ToString(), "min, ");
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                            {
                                unixTime -= (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                                {
                                    builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                                }
                            }
                        }
                        else if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                        {
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                            {
                                builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                            }
                        }
                    }
                }
                else if (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) > 0)
                {
                    builder.Append(Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()).ToString(), "h, ");
                    if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                    {
                        unixTime -= (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
                        if (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) > 0)
                        {
                            builder.Append(Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()).ToString(), "min, ");
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                            {
                                unixTime -= (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                                {
                                    builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                                }
                            }
                        }
                        else if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                        {
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                            {
                                builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                            }
                        }
                    }
                }
                else if (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) > 0)
                {
                    if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                    {
                        builder.Append(Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()).ToString(), "min, ");
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                        {
                            unixTime -= (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                            if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                            {
                                builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                            }
                        }
                    }
                }
                else if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                {
                    if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                    {
                        builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                    }
                }
            }
            else if (Math.Truncate((unixTime / new Day().Milliseconds).ToDouble()) > 0)
            {
                builder.Append(Math.Truncate((unixTime / new Day().Milliseconds).ToDouble()).ToString(), "d, ");
                unixTime -= (Math.Truncate((unixTime / new Day().Milliseconds).ToDouble()) * new Day().Milliseconds).ToLong();
                if (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) > 0)
                {
                    builder.Append(Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()).ToString(), "h, ");
                    unixTime -= (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
                    if (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) > 0)
                    {
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                        {
                            builder.Append(Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()).ToString(), "min, ");
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                            {
                                unixTime -= (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                                {
                                    builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                                }
                            }
                        }
                    }
                    else if (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) > 0)
                    {
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                        {
                            builder.Append(Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()).ToString(), "min, ");
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                            {
                                unixTime -= (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                                {
                                    builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                                }
                            }
                        }
                    }
                    else if (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) > 0)
                    {
                        builder.Append(Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()).ToString(), "h, ");
                        unixTime -= (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
                        if (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) > 0)
                        {
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                            {
                                builder.Append(Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()).ToString(), "min, ");
                                if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                {
                                    unixTime -= (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                    if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                                    {
                                        builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                                    }
                                }
                            }
                        }
                        else if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                        {
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                            {
                                builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                            }
                        }
                    }
                }
                else if (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) > 0)
                {
                    if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                    {
                        builder.Append(Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()).ToString(), "min, ");
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                        {
                            unixTime -= (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                            if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                            {
                                builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                            }
                        }
                    }
                }
                else if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                {
                    if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                    {
                        builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                    }
                }
            }
            else if (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) > 0)
            {
                builder.Append(Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()).ToString(), "h, ");
                unixTime -= (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
                if (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) > 0)
                {
                    if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                    {
                        builder.Append(Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()).ToString(), "min, ");
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                        {
                            unixTime -= (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                            if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                            {
                                builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                            }
                        }
                    }
                }
                else if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                {
                    if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                    {
                        builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                    }
                }
            }
            else if (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) > 0)
            {
                if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                {
                    builder.Append(Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()).ToString(), "min, ");
                    if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                    {
                        unixTime -= (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                        if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                        {
                            builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                        }
                    }
                }
            }
            else if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
            {
                if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                {
                    builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                }
            }
        }
        else if (Math.Truncate((unixTime / new Day().Milliseconds).ToDouble()) > 0)
        {
            builder.Append(Math.Truncate((unixTime / new Day().Milliseconds).ToDouble()).ToString(), "d, ");
            unixTime -= (Math.Truncate((unixTime / new Day().Milliseconds).ToDouble()) * new Day().Milliseconds).ToLong();
            if (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) > 0)
            {
                builder.Append(Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()).ToString(), "h, ");
                if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                {
                    unixTime -= (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
                    if (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) > 0)
                    {
                        builder.Append(Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()).ToString(), "min, ");
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                        {
                            unixTime -= (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                            if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                            {
                                builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                            }
                        }
                    }
                    else if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                    {
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                        {
                            builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                        }
                    }
                }
            }
            else if (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) > 0)
            {
                if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                {
                    builder.Append(Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()).ToString(), "min, ");
                    if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                    {
                        unixTime -= (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                        if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                        {
                            builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                        }
                    }
                }
            }
            else if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
            {
                if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                {
                    builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                }
            }
        }
        else if (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) > 0)
        {
            builder.Append(Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()).ToString(), "h, ");
            unixTime -= (Math.Truncate((unixTime / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
            if (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) > 0)
            {
                builder.Append(Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()).ToString(), "min, ");
                if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                {
                    unixTime -= (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                    if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
                    {
                        builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                    }
                }
            }
            else if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
            {
                if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                {
                    builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
                }
            }
        }
        else if (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) > 0)
        {
            builder.Append(Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()).ToString(), "min, ");
            unixTime -= (Math.Truncate((unixTime / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
            if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
            {
                builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
            }
        }
        else if (Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()) > 0)
        {
            builder.Append(Math.Truncate((unixTime / new Second().Milliseconds).ToDouble()).ToString(), "s");
        }

        string result = builder.ToString().TrimAll();
        if (result[^1] == ',')
        {
            result = result[..^1];
        }

        return result;
    }

    public static UnixDiffSpan GetUnixDifference(long unixTime)
    {
        UnixDiffSpan result = new();
        long now = Now();
        if (unixTime > now)
        {
            unixTime -= now;
        }
        else if (unixTime < now)
        {
            unixTime = now - unixTime;
        }
        else
        {
            return result;
        }

        long yearMs = new Year().Milliseconds;
        long dayMs = new Day().Milliseconds;
        long hourMs = new Hour().Milliseconds;
        long minuteMs = new Minute().Milliseconds;
        long secondMs = new Second().Milliseconds;

        bool returnNow = false;

        CheckMilliseconds();
        if (returnNow)
        {
            return result;
        }

        CheckSeconds();
        if (returnNow)
        {
            return result;
        }

        CheckMinutes();
        if (returnNow)
        {
            return result;
        }

        CheckHours();
        if (returnNow)
        {
            return result;
        }

        CheckDays();
        if (returnNow)
        {
            return result;
        }

        CheckYears();
        return result;

        #region Local methods

        void CheckMilliseconds()
        {
            if (unixTime > 0 && unixTime < secondMs)
            {
                result.Milliseconds = (ushort)unixTime;
                returnNow = true;
            }
        }

        void CheckSeconds()
        {
            if (unixTime > secondMs && unixTime < minuteMs)
            {
                double seconds = unixTime / secondMs;
                double roundedSeconds = Math.Round(seconds);
                unixTime -= (long)roundedSeconds * secondMs;
                result.Seconds = (byte)roundedSeconds;
                if (unixTime >= new Second().Milliseconds >> 1)
                {
                    result.Seconds++;
                    if (result.Seconds == new Minute().Seconds)
                    {
                        result.Minutes++;
                        result.Seconds = 0;
                        if (result.Minutes == new Hour().Minutes)
                        {
                            result.Hours++;
                            result.Minutes = 0;
                            if (result.Hours == new Day().Hours)
                            {
                                result.Days++;
                                result.Hours = 0;
                                if (result.Days == new Year().Days)
                                {
                                    result.Years++;
                                    result.Days = 0;
                                }
                            }
                        }
                    }
                }

                returnNow = true;
            }
        }

        void CheckMinutes()
        {
            if (unixTime > minuteMs && unixTime < hourMs)
            {
                double minutes = unixTime / minuteMs;
                double roundedMinutes = Math.Round(minutes);
                unixTime -= (long)roundedMinutes * minuteMs;
                result.Minutes = (byte)roundedMinutes;

                CheckSeconds();
                returnNow = true;
            }
        }

        void CheckHours()
        {
            if (unixTime > hourMs && unixTime < dayMs)
            {
                double hours = unixTime / hourMs;
                double roundedHours = Math.Round(hours);
                unixTime -= (long)roundedHours * hourMs;
                result.Hours = (byte)roundedHours;

                CheckMinutes();
                returnNow = true;
            }
        }

        void CheckDays()
        {
            if (unixTime > dayMs && unixTime < yearMs)
            {
                double days = unixTime / dayMs;
                double roundedDays = Math.Round(days);
                unixTime -= (long)days * dayMs;
                result.Days = (ushort)roundedDays;

                CheckHours();
                returnNow = true;
            }
        }

        void CheckYears()
        {
            double years = unixTime / yearMs;
            double roundedYears = Math.Round(years);
            unixTime -= (long)roundedYears * yearMs;
            result.Years = (uint)roundedYears;

            CheckDays();
        }

        #endregion Local functions
    }

    /// <summary>
    /// Return the hours that remain until the given day time.
    /// </summary>
    /// <param name="hour">The hour in 24h format.</param>
    /// <param name="minute">The minute.</param>
    /// <param name="second">The second.</param>
    /// <param name="millisecond">The millisecond.</param>
    /// <returns>The hours that remain until the given day time.</returns>
    public static double HoursUntil(int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
    {
        return MinutesUntil(hour, minute, second, millisecond) / 60;
    }

    /// <summary>
    /// Return the milliseconds that remain until the given day time.
    /// </summary>
    /// <param name="hour">The hour in 24h format.</param>
    /// <param name="minute">The minute.</param>
    /// <param name="second">The second.</param>
    /// <param name="millisecond">The millisecond.</param>
    /// <returns>The milliseconds that remain until the given day time.</returns>
    public static long MillisecondsUntil(int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
    {
        long result = 0;
        (int Hours, int Minutes, int Seconds, int Milliseconds) now = new(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
        if (now.Milliseconds > millisecond)
        {
            result += 1000 - now.Milliseconds + millisecond;
        }
        else
        {
            result += millisecond - now.Milliseconds;
        }

        now.Seconds++;
        if (now.Seconds > second)
        {
            result += new Second(60 - now.Seconds + second).Milliseconds;
        }
        else
        {
            result += new Second(second - now.Seconds).Milliseconds;
        }

        now.Minutes++;
        if (now.Minutes > minute)
        {
            result += new Minute(60 - now.Minutes + minute).Milliseconds;
            now.Hours++;
        }
        else
        {
            result += new Minute(minute - now.Minutes).Milliseconds;
        }

        if (now.Hours > hour)
        {
            result += new Hour(24 - now.Hours + hour).Milliseconds;
        }
        else
        {
            result += new Hour(hour - now.Hours).Milliseconds;
        }

        return result;
    }

    /// <summary>
    /// Return the minutes that remain until the given day time.
    /// </summary>
    /// <param name="hour">The hour in 24h format.</param>
    /// <param name="minute">The minute.</param>
    /// <param name="second">The second.</param>
    /// <param name="millisecond">The millisecond.</param>
    /// <returns>The minutes that remain until the given day time.</returns>
    public static double MinutesUntil(int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
    {
        return SecondsUntil(hour, minute, second, millisecond) / 60;
    }

    /// <summary>
    /// Return the Unix time stamp in milliseconds for the computer's time zone.
    /// </summary>
    /// <returns>The Unix time stamp.</returns>
    public static long Now()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Return the seconds that remain until the given day time.
    /// </summary>
    /// <param name="hour">The hour in 24h format.</param>
    /// <param name="minute">The minute.</param>
    /// <param name="second">The second.</param>
    /// <param name="millisecond">The millisecond.</param>
    /// <returns>The seconds that remain until the given day time.</returns>
    public static double SecondsUntil(int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
    {
        return MillisecondsUntil(hour, minute, second, millisecond) / 1000;
    }
}
