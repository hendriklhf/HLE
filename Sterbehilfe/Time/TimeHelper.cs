using Sterbehilfe.Numbers;
using Sterbehilfe.Strings;
using Sterbehilfe.Time.Enums;
using System;
using System.Collections.Generic;

namespace Sterbehilfe.Time
{
    public static class TimeHelper
    {
        public static long Now()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public static string ConvertMillisecondsToPassedTime(long time, string addition = "", ConversionType conversionType = ConversionType.All)
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

        public static long ConvertStringToMilliseconds(List<string> input)
        {
            long result = 0;
            input.ForEach(str =>
            {
                if (str.IsMatch(Year.Pattern))
                {
                    result += new Year(Convert.ToInt32(str.Match(@"\d+"))).Milliseconds;
                }
                else if (str.IsMatch(Day.Pattern))
                {
                    result += new Day(Convert.ToInt32(str.Match(@"\d+"))).Milliseconds;
                }
                else if (str.IsMatch(Hour.Pattern))
                {
                    result += new Hour(Convert.ToInt32(str.Match(@"\d+"))).Milliseconds;
                }
                else if (str.IsMatch(Minute.Pattern))
                {
                    result += new Minute(Convert.ToInt32(str.Match(@"\d+"))).Milliseconds;
                }
                else if (str.IsMatch(Second.Pattern))
                {
                    result += new Second(Convert.ToInt32(str.Match(@"\d+"))).Milliseconds;
                }
            });
            return result;
        }

        public static long ConvertStringToSeconds(List<string> input)
        {
            return ConvertStringToMilliseconds(input) / 1000;
        }
    }
}