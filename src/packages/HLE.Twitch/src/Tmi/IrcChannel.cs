using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Text;
using HLE.Text;

namespace HLE.Twitch.Tmi;

internal sealed class IrcChannel(string name) : IEquatable<IrcChannel>
{
    public string Name { get; } = name;

    public ImmutableArray<byte> NameUtf8 { get; } = ImmutableCollectionsMarshal.AsImmutableArray(Encoding.UTF8.GetBytes(name));

    public IrcChannel(ReadOnlySpan<char> name) : this(StringPool.Shared.GetOrAdd(name))
    {
    }

    public bool Equals(IrcChannel? other) => Name == other?.Name && NameUtf8.AsSpan().SequenceEqual(other.NameUtf8.AsSpan());

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is IrcChannel other && Equals(other);

    public override int GetHashCode() => (int)XxHash32.HashToUInt32(NameUtf8.AsSpan());

    public static bool operator ==(IrcChannel left, IrcChannel right) => left.Equals(right);

    public static bool operator !=(IrcChannel left, IrcChannel right) => !(left == right);
}
