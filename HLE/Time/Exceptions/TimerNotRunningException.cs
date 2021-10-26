using System;

namespace HLE.Time.Exceptions;

public class TimerNotRunningException : Exception
{
    public override string Message => "The timer has to be runnning to retrieve the remaining time.";

    public TimerNotRunningException() : base()
    {
    }

    public TimerNotRunningException(string message) : base(message)
    {
    }
}
