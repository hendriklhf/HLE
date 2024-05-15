using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Strings;

public readonly struct GenericTypeDelimiters(string openingDelimiter, string closingDelimiter) : IEquatable<GenericTypeDelimiters>
{
    public string Opening { get; } = openingDelimiter;

    public string Closing { get; } = closingDelimiter;

    private const string DefaultOpeningDelimiter = "<";
    private const string DefaultClosingDelimiter = ">";

    public GenericTypeDelimiters() : this(DefaultOpeningDelimiter, DefaultClosingDelimiter)
    {
    }

    [Pure]
    public bool Equals(GenericTypeDelimiters other) => Opening == other.Opening && Closing == other.Closing;

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is GenericTypeDelimiters other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Opening, Closing);

    public static bool operator ==(GenericTypeDelimiters left, GenericTypeDelimiters right) => left.Equals(right);

    public static bool operator !=(GenericTypeDelimiters left, GenericTypeDelimiters right) => !(left == right);
}
