using System;
using System.Diagnostics;

namespace HLE.Twitch.Models;

[DebuggerDisplay("{Type}: \"{Message}\"")]
public readonly struct Notice : IEquatable<Notice>
{
    public NoticeType Type { get; }

    public string Message { get; }

    public string Channel { get; }

    public Notice(NoticeType type, string message, string channel)
    {
        Type = type;
        Message = message;
        Channel = channel;
    }

    public bool Equals(Notice other)
    {
        return Type == other.Type && Channel == other.Channel && Message == other.Message;
    }

    public override bool Equals(object? obj)
    {
        return obj is Notice other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Message);
    }

    public static bool operator ==(Notice left, Notice right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Notice left, Notice right)
    {
        return !(left == right);
    }
}
