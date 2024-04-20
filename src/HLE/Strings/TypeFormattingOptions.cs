using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Strings;

public sealed class TypeFormattingOptions : IEquatable<TypeFormattingOptions>
{
    public required char NamespaceSeparator { get; init; }

    public required string GenericTypesSeparator { get; init; }

    public required GenericTypeDelimiters GenericDelimiters { get; init; }

    [Pure]
    public bool Equals([NotNullWhen(true)] TypeFormattingOptions? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(TypeFormattingOptions? left, TypeFormattingOptions? right) => Equals(left, right);

    public static bool operator !=(TypeFormattingOptions? left, TypeFormattingOptions? right) => !(left == right);
}
