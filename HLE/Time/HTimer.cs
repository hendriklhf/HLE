using System;
using System.Timers;

namespace HLE.Time;

public sealed class HTimer
{
    public bool AutoReset
    {
        get => _timer.AutoReset;
        set => _timer.AutoReset = value;
    }

    public bool Enabled => _timer.Enabled;

    public double Interval
    {
        get => _timer.Interval;
        set => _timer.Interval = value;
    }

    public double RemainingTime => GetRemainingTime();

    public event EventHandler? OnElapsed;

    private readonly Timer _timer;

    private double _end;

    public HTimer(double interval)
    {
        _timer = new(interval);
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
        _end = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Interval;
        _timer.Start();
    }

    public void Stop()
    {
        _end = -1;
        _timer.Stop();
    }

    private double GetRemainingTime()
    {
        if (Math.Abs(_end + 1) < 0 || !Enabled)
        {
            return -1;
        }

        double result = _end - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return result >= 0 ? result : 0;

    }
}
