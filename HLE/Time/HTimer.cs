﻿using System;
using System.Diagnostics.Contracts;
using System.Timers;
using HLE.Memory;

namespace HLE.Time;

public sealed class HTimer : IEquatable<HTimer>
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
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return MemoryHelper.GetRawDataPointer(this).GetHashCode();
    }
}
