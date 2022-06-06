using System;

namespace HLE.Time;

/// <summary>
/// A class to help with any kind of time.
/// </summary>
public static class TimeHelper
{
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

        long yearMs = (long)TimeSpan.FromDays(365).TotalMilliseconds;
        long dayMs = (long)TimeSpan.FromDays(1).TotalMilliseconds;
        long hourMs = (long)TimeSpan.FromHours(1).TotalMilliseconds;
        long minuteMs = (long)TimeSpan.FromMinutes(1).TotalMilliseconds;
        long secondMs = (long)TimeSpan.FromSeconds(1).TotalMilliseconds;

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
                if (unixTime >= secondMs >> 1)
                {
                    result.Seconds++;
                    if (result.Seconds == 60)
                    {
                        result.Minutes++;
                        result.Seconds = 0;
                        if (result.Minutes == 60)
                        {
                            result.Hours++;
                            result.Minutes = 0;
                            if (result.Hours == 24)
                            {
                                result.Days++;
                                result.Hours = 0;
                                if (result.Days == 365)
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
            result += (long)TimeSpan.FromSeconds(60 - now.Seconds + second).TotalMilliseconds;
        }
        else
        {
            result += (long)TimeSpan.FromSeconds(second - now.Seconds).TotalMilliseconds;
        }

        now.Minutes++;
        if (now.Minutes > minute)
        {
            result += (long)TimeSpan.FromMinutes(60 - now.Minutes + minute).TotalMilliseconds;
            now.Hours++;
        }
        else
        {
            result += (long)TimeSpan.FromMinutes(minute - now.Minutes).TotalMilliseconds;
        }

        if (now.Hours > hour)
        {
            result += (long)TimeSpan.FromHours(24 - now.Hours + hour).TotalMilliseconds;
        }
        else
        {
            result += (long)TimeSpan.FromHours(hour - now.Hours).TotalMilliseconds;
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
    /// Return the Unix time stamp in milliseconds for the UTC time zone.
    /// </summary>
    /// <returns>The Unix time stamp.</returns>
    public static long Now()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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
