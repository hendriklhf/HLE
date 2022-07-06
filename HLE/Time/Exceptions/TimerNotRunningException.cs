using System;

namespace HLE.Time.Exceptions;

public class TimerNotRunningException : Exception
{
    public override string Message => "The timer has to be running to retrieve the remaining time.";
}
