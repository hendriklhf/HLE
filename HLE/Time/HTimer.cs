using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Timers;

namespace HLE.Time;

public sealed class HTimer : IEquatable<HTimer>, IDisposable
{
    public bool AutoReset { get; set; }

    public bool Enabled => _timer.Enabled;

    public TimeSpan Interval => TimeSpan.FromMilliseconds(_timer.Interval);

    public TimeSpan RemainingTime => GetRemainingTime();

    public event EventHandler? OnElapsed;

    private readonly Timer _timer;
    private DateTimeOffset _end;

    public HTimer(TimeSpan interval)
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

    public void Dispose()
    {
        _timer.Dispose();
    }

    public void Start()
    {
        _end = DateTimeOffset.UtcNow + Interval;
        _timer.Start();
    }

    public void Stop()
    {
        _end = default;
        _timer.Stop();
    }

    private TimeSpan GetRemainingTime()
    {
        if (_end == default || DateTimeOffset.UtcNow > _end)
        {
            return TimeSpan.Zero;
        }

        return _end - DateTimeOffset.UtcNow;
    }

    [Pure]
    public bool Equals(HTimer? other)
    {
        return ReferenceEquals(this, other);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is HTimer other && Equals(other);
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }
}
