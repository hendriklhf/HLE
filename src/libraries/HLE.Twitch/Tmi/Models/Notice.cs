using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Twitch.Tmi.Models;

[DebuggerDisplay("{Type}: \"{Message}\"")]
public readonly struct Notice(NoticeType type, string message, string channel)
    : IEquatable<Notice>
{
    public string Message { get; } = message;

    public string Channel { get; } = channel;

    public NoticeType Type { get; } = type;

    [Pure]
    public bool Equals(Notice other) => Type == other.Type && Channel == other.Channel && Message == other.Message;

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Notice other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Type, Message);

    public static bool operator ==(Notice left, Notice right) => left.Equals(right);

    public static bool operator !=(Notice left, Notice right) => !(left == right);
}
