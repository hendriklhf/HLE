using System;
using System.Timers;
using HLE.Time.Exceptions;

namespace HLE.Time;

public class HTimer
{
    public bool AutoReset
    {
        get => _timer.AutoReset;
        set => _timer.AutoReset = value;
    }

    public bool Enabled
    {
        get => _timer.Enabled;
        set => _timer.Enabled = value;
    }

    public double Interval
    {
        get => _timer.Interval;
        set => _timer.Interval = value;
    }

    public double? RemainingTime => GetRemainingTime();

    public event EventHandler Elapsed;

    private readonly Timer _timer;

    private double? _end;

    public HTimer(double interval)
    {
        _timer = new(interval);
        _timer.Elapsed += (sender, e) =>
        {
            Elapsed?.Invoke(this, new());
            if (AutoReset)
            {
                Start();
            }
        };
    }

    public HTimer(double interval, Action<object, EventArgs> onElapsed)
        : this(interval)
    {
        Elapsed += (sender, e) => onElapsed(sender, e);
    }

    public void Start()
    {
        _end = TimeHelper.Now() + Interval;
        _timer.Start();
    }

    public void Stop()
    {
        _end = null;
        _timer.Stop();
    }

    private double? GetRemainingTime()
    {
        if (_end is not null && Enabled)
        {
            double? result = _end - TimeHelper.Now();
            return result >= 0 ? result : 0;
        }
        else
        {
            throw new TimerNotRunningException();
        }
    }
}
