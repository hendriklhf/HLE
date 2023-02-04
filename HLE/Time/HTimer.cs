using System;
using System.Timers;

namespace HLE.Time;

public sealed class HTimer
{
    public bool AutoReset { get; set; }

    public bool Enabled => _timer.Enabled;

    public double Interval => _timer.Interval;

    public TimeSpan RemainingTime => GetRemainingTime();

    public event EventHandler? OnElapsed;

    private readonly Timer _timer;
    private DateTimeOffset _end;

    public HTimer(double interval)
    {
        _timer = new(interval)
        {
            AutoReset = false
        };

        _timer.Elapsed += (_, _) =>
        {
            OnElapsed?.Invoke(this, EventArgs.Empty);
            if (AutoReset)
            {
                Start();
            }
        };
    }

    public void Start()
    {
        _end = DateTimeOffset.UtcNow.AddMilliseconds(Interval);
        _timer.Start();
    }

    public void Stop()
    {
        _end = default;
        _timer.Stop();
    }

    private TimeSpan GetRemainingTime()
    {
        if (_end == default)
        {
            return TimeSpan.Zero;
        }

        return _end - DateTimeOffset.UtcNow;
    }
}
