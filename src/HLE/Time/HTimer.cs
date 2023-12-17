using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        get
        {
            ObjectDisposedException.ThrowIf(_timer is null, typeof(HTimer));
            return _timer.AutoReset;
        }
        set
        {
            ObjectDisposedException.ThrowIf(_timer is null, typeof(HTimer));
            _timer.AutoReset = value;
        }
    }

    public bool Enabled
    {
        get
        {
            ObjectDisposedException.ThrowIf(_timer is null, typeof(HTimer));
            return _timer.Enabled;
        }
    }

    public TimeSpan Interval
    {
        get
        {
            ObjectDisposedException.ThrowIf(_timer is null, typeof(HTimer));
            return TimeSpan.FromMilliseconds(_timer.Interval);
        }
    }

    public TimeSpan RemainingTime
    {
        get
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (_end == default || now >= _end)
            {
                return TimeSpan.Zero;
            }

            return _end - now;
        }
    }

    public event EventHandler? OnElapsed;

    private Timer? _timer;
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
        _timer?.Dispose();
        _timer = null;
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_timer is null, typeof(HTimer));

        if (Enabled)
        {
            return;
        }

        _end = DateTimeOffset.UtcNow + Interval;
        _timer.Start();
    }

    public void Stop()
    {
        ObjectDisposedException.ThrowIf(_timer is null, typeof(HTimer));

        if (!Enabled)
        {
            return;
        }

        _end = default;
        _timer.Stop();
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] HTimer? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(HTimer? left, HTimer? right) => Equals(left, right);

    public static bool operator !=(HTimer? left, HTimer? right) => !(left == right);
}
