using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Timers;

namespace HLE.Time;

// ReSharper disable UseNameofExpressionForPartOfTheString
[DebuggerDisplay(nameof(Interval) + " = {Interval} " + nameof(RemainingTime) + " = {RemainingTime}")]
public sealed class HTimer : IEquatable<HTimer>, IDisposable
{
    public bool AutoReset
    {
        get => _timer.AutoReset;
        set => _timer.AutoReset = value;
    }

    public bool Enabled => _timer.Enabled;

    public TimeSpan Interval => TimeSpan.FromMilliseconds(_timer.Interval);

    public TimeSpan RemainingTime => GetRemainingTime();

    public event EventHandler? OnElapsed;

    private readonly Timer _timer;
    private DateTimeOffset _end;

    public HTimer(TimeSpan interval)
    {
        _timer = new(interval);
        _timer.Elapsed += (sender, _) =>
        {
            OnElapsed?.Invoke(this, EventArgs.Empty);
            Timer timer = Unsafe.As<object?, Timer>(ref sender);
            if (timer.AutoReset)
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
        if (Enabled)
        {
            return;
        }

        _end = DateTimeOffset.UtcNow + Interval;
        _timer.Start();
    }

    public void Stop()
    {
        if (!Enabled)
        {
            return;
        }

        _end = default;
        _timer.Stop();
    }

    private TimeSpan GetRemainingTime()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (_end == default || now >= _end)
        {
            return TimeSpan.Zero;
        }

        return _end - now;
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

    public static bool operator ==(HTimer? left, HTimer? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(HTimer? left, HTimer? right)
    {
        return !(left == right);
    }
}
