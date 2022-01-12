using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HLE.Numbers;
using HLE.Strings;
using HLE.Time.Enums;

namespace HLE.Time
{
    /// <summary>
    /// A class to help with any kind of time.
    /// </summary>
    public static class TimeHelper
    {
        /// <summary>
        /// Converts <see cref="string"/>s that match the <see cref="Interfaces.ITimeUnit.Pattern"/> in a <see cref="List{String}"/> to seconds.
        /// </summary>
        /// <param name="input">The <see cref="List{String}"/> in which the <see cref="string"/>s will be converted.</param>
        /// <returns>The amount of seconds of all time <see cref="string"/>s.</returns>
        public static long ConvertStringToSeconds(List<string> input)
        {
            return ConvertTimeToMilliseconds(input) / 1000;
        }

        /// <summary>
        /// Converts <see cref="string"/>s that match the <see cref="Time.Interfaces.ITimeUnit.Pattern"/> in a <see cref="List{String}"/> to milliseconds.
        /// </summary>
        /// <param name="input">The <see cref="List{String}"/> in which the <see cref="string"/>s will be converted.</param>
        /// <returns>The amount of milliseconds of all time <see cref="string"/>s.</returns>
        public static long ConvertTimeToMilliseconds(List<string> input)
        {
            long result = 0;
            input.ForEach(str =>
            {
                if (str.IsMatch(new Year().Pattern))
                {
                    result += new Year(str.Match(@"\d+").ToInt()).Milliseconds;
                }
                else if (str.IsMatch(new Day().Pattern))
                {
                    result += new Day(str.Match(@"\d+").ToInt()).Milliseconds;
                }
                else if (str.IsMatch(new Week().Pattern))
                {
                    result += new Week(str.Match(@"\d+").ToInt()).Milliseconds;
                }
                else if (str.IsMatch(new Hour().Pattern))
                {
                    result += new Hour(str.Match(@"\d+").ToInt()).Milliseconds;
                }
                else if (str.IsMatch(new Minute().Pattern))
                {
                    result += new Minute(str.Match(@"\d+").ToInt()).Milliseconds;
                }
                else if (str.IsMatch(new Second().Pattern))
                {
                    result += new Second(str.Match(@"\d+").ToInt()).Milliseconds;
                }
            });
            return result;
        }

        /// <summary>
        /// Converts the a Unix time stamp into a time stamp with time units. The Unix time stamp can be in the future or past.<br />
        /// For example: "1h, 24min, 56s"
        /// </summary>
        /// <param name="unixTime">The point of time as a Unix time stamp.</param>
        /// <param name="conversionType">A <see cref="ConversionType"/> to shorten the <see cref="string"/> of time to, for example, only show the hours and minutes.</param>
        /// <returns>The time stamp result as a <see cref="string"/>.</returns>
        public static string ConvertUnixTimeToTimeStamp(long unixTime, ConversionType conversionType = ConversionType.All)
        {
            StringBuilder builder = new();
            if (unixTime > Now())
            {
                unixTime -= (unixTime - Now()) * 2;
            }

            if (!(Now() - unixTime >= 1000))
            {
                return $"{Now() - unixTime}ms";
            }

            unixTime = Now() - unixTime;
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
}
