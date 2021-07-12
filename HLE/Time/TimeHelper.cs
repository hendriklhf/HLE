using HLE.Numbers;
using HLE.Strings;
using HLE.Time.Enums;
using System;
using System.Collections.Generic;

namespace HLE.Time
{
    /// <summary>
    /// A class to help with any kind of time.
    /// </summary>
    public static class TimeHelper
    {
        /// <summary>
        /// Return the Unix time stamp in milliseconds for the computer's time zone.
        /// </summary>
        /// <returns>The Unix timestamp.</returns>
        public static long Now()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Converts the Unix timestamp into passed time since the given <paramref name="time"/>.<br />
        /// For example: "1h, 24min, 56s ago"
        /// </summary>
        /// <param name="time">The point of time in the past.</param>
        /// <param name="addition">A <see cref="string"/> that will be placed behind the return <see cref="string"/>, like: "ago".</param>
        /// <param name="conversionType">A <see cref="ConversionType"/> to shorten the <see cref="string"/> of passed time to, for example, only show the hours and minutes.</param>
        /// <returns>The passed time as a <see cref="string"/>.</returns>
        public static string ConvertUnixTimeToPassedTime(long time, string addition = "", ConversionType conversionType = ConversionType.All)
        {
            if (Now() - time >= 1000)
            {
                string result = "";
                time = Now() - time;
                if (Math.Truncate((time / new Year().Milliseconds).ToDouble()) > 0)
                {
                    result += Math.Truncate((time / new Year().Milliseconds).ToDouble()).ToString() + "y, ";
                    time -= (Math.Truncate((time / new Year().Milliseconds).ToDouble()) * new Year().Milliseconds).ToLong();
                    if (Math.Truncate((time / new Day().Milliseconds).ToDouble()) > 0)
                    {
                        result += Math.Truncate((time / new Day().Milliseconds).ToDouble()).ToString() + "d, ";
                        time -= (Math.Truncate((time / new Day().Milliseconds).ToDouble()) * new Day().Milliseconds).ToLong();
                        if (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) > 0)
                        {
                            result += Math.Truncate((time / new Hour().Milliseconds).ToDouble()).ToString() + "h, ";
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                            {
                                time -= (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
                                if (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) > 0)
                                {
                                    result += Math.Truncate((time / new Minute().Milliseconds).ToDouble()).ToString() + "min, ";
                                    if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                    {
                                        time -= (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                        if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                        {
                                            result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                        }
                                    }
                                }
                                else if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                {
                                    if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                    {
                                        result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                    }
                                }
                            }
                        }
                        else if (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) > 0)
                        {
                            result += Math.Truncate((time / new Hour().Milliseconds).ToDouble()).ToString() + "h, ";
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                            {
                                time -= (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
                                if (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) > 0)
                                {
                                    result += Math.Truncate((time / new Minute().Milliseconds).ToDouble()).ToString() + "min, ";
                                    if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                    {
                                        time -= (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                        if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                        {
                                            result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                        }
                                    }
                                }
                                else if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                {
                                    if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                    {
                                        result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                    }
                                }
                            }
                        }
                        else if (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) > 0)
                        {
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                            {
                                result += Math.Truncate((time / new Minute().Milliseconds).ToDouble()).ToString() + "min, ";
                                if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                {
                                    time -= (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                    if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                    {
                                        result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                    }
                                }
                            }
                        }
                        else if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                        {
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                            {
                                result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                            }
                        }
                    }
                    else if (Math.Truncate((time / new Day().Milliseconds).ToDouble()) > 0)
                    {
                        result += Math.Truncate((time / new Day().Milliseconds).ToDouble()).ToString() + "d, ";
                        time -= (Math.Truncate((time / new Day().Milliseconds).ToDouble()) * new Day().Milliseconds).ToLong();
                        if (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) > 0)
                        {
                            result += Math.Truncate((time / new Hour().Milliseconds).ToDouble()).ToString() + "h, ";
                            time -= (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
                            if (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) > 0)
                            {
                                if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                                {
                                    result += Math.Truncate((time / new Minute().Milliseconds).ToDouble()).ToString() + "min, ";
                                    if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                    {
                                        time -= (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                        if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                        {
                                            result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                        }
                                    }
                                }
                            }
                            else if (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) > 0)
                            {
                                if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                                {
                                    result += Math.Truncate((time / new Minute().Milliseconds).ToDouble()).ToString() + "min, ";
                                    if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                    {
                                        time -= (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                        if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                        {
                                            result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                        }
                                    }
                                }
                            }
                            else if (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) > 0)
                            {
                                result += Math.Truncate((time / new Hour().Milliseconds).ToDouble()).ToString() + "h, ";
                                time -= (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
                                if (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) > 0)
                                {
                                    if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                                    {
                                        result += Math.Truncate((time / new Minute().Milliseconds).ToDouble()).ToString() + "min, ";
                                        if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                        {
                                            time -= (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                            if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                            {
                                                result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                            }
                                        }
                                    }
                                }
                                else if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                {
                                    if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                    {
                                        result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                    }
                                }
                            }
                        }
                        else if (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) > 0)
                        {
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                            {
                                result += Math.Truncate((time / new Minute().Milliseconds).ToDouble()).ToString() + "min, ";
                                if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                {
                                    time -= (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                    if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                    {
                                        result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                    }
                                }
                            }
                        }
                        else if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                        {
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                            {
                                result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                            }
                        }
                    }
                    else if (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) > 0)
                    {
                        result += Math.Truncate((time / new Hour().Milliseconds).ToDouble()).ToString() + "h, ";
                        time -= (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
                        if (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) > 0)
                        {
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                            {
                                result += Math.Truncate((time / new Minute().Milliseconds).ToDouble()).ToString() + "min, ";
                                if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                {
                                    time -= (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                    if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                    {
                                        result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                    }
                                }
                            }
                        }
                        else if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                        {
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                            {
                                result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                            }
                        }
                    }
                    else if (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) > 0)
                    {
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                        {
                            result += Math.Truncate((time / new Minute().Milliseconds).ToDouble()).ToString() + "min, ";
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                            {
                                time -= (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                {
                                    result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                }
                            }
                        }
                    }
                    else if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                    {
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                        {
                            result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                        }
                    }
                }
                else if (Math.Truncate((time / new Day().Milliseconds).ToDouble()) > 0)
                {
                    result += Math.Truncate((time / new Day().Milliseconds).ToDouble()).ToString() + "d, ";
                    time -= (Math.Truncate((time / new Day().Milliseconds).ToDouble()) * new Day().Milliseconds).ToLong();
                    if (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) > 0)
                    {
                        result += Math.Truncate((time / new Hour().Milliseconds).ToDouble()).ToString() + "h, ";
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                        {
                            time -= (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
                            if (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) > 0)
                            {
                                result += Math.Truncate((time / new Minute().Milliseconds).ToDouble()).ToString() + "min, ";
                                if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                {
                                    time -= (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                    if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                    {
                                        result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                    }
                                }
                            }
                            else if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                            {
                                if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                                {
                                    result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                }
                            }
                        }
                    }
                    else if (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) > 0)
                    {
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMin)
                        {
                            result += Math.Truncate((time / new Minute().Milliseconds).ToDouble()).ToString() + "min, ";
                            if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                            {
                                time -= (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                                if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                                {
                                    result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                                }
                            }
                        }
                    }
                    else if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                    {
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                        {
                            result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                        }
                    }
                }
                else if (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) > 0)
                {
                    result += Math.Truncate((time / new Hour().Milliseconds).ToDouble()).ToString() + "h, ";
                    time -= (Math.Truncate((time / new Hour().Milliseconds).ToDouble()) * new Hour().Milliseconds).ToLong();
                    if (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) > 0)
                    {
                        result += Math.Truncate((time / new Minute().Milliseconds).ToDouble()).ToString() + "min, ";
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                        {
                            time -= (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                            if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                            {
                                result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                            }
                        }
                    }
                    else if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                    {
                        if ((int)conversionType >= (int)ConversionType.YearDayHourMinSec)
                        {
                            result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                        }
                    }
                }
                else if (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) > 0)
                {
                    result += Math.Truncate((time / new Minute().Milliseconds).ToDouble()).ToString() + "min, ";
                    time -= (Math.Truncate((time / new Minute().Milliseconds).ToDouble()) * new Minute().Milliseconds).ToLong();
                    if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                    {
                        result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                    }
                }
                else if (Math.Truncate((time / new Second().Milliseconds).ToDouble()) > 0)
                {
                    result += Math.Truncate((time / new Second().Milliseconds).ToDouble()).ToString() + "s";
                }

                result = result.Trim();
                if (result[^1] == ',')
                {
                    result = result[0..^1];
                }

                return $"{result} {addition}".Trim();
            }
            else
            {
                return $"{Now() - time}ms {addition}".Trim();
            }
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
        /// Converts <see cref="string"/>s that match the <see cref="Time.Interfaces.ITimeUnit.Pattern"/> in a <see cref="List{String}"/> to seconds.
        /// </summary>
        /// <param name="input">The <see cref="List{String}"/> in which the <see cref="string"/>s will be converted.</param>
        /// <returns>The amount of seconds of all time <see cref="string"/>s.</returns>
        public static long ConvertStringToSeconds(List<string> input)
        {
            return ConvertTimeToMilliseconds(input) / 1000;
        }
    }
}