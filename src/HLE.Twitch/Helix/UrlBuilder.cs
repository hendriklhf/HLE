using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using HLE.Text;

namespace HLE.Twitch.Helix;

[DebuggerDisplay("{ToString()}")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "it implements IDisposable")]
internal struct UrlBuilder : IDisposable, IEquatable<UrlBuilder>
{
    public readonly ReadOnlySpan<char> WrittenSpan => _builder.WrittenSpan;

    public int ParameterCount { get; private set; }

    private readonly PooledStringBuilder _builder;

    public UrlBuilder(ReadOnlySpan<char> baseUrl, ReadOnlySpan<char> endpoint, int capacity = 128)
    {
        _builder = new(capacity);
        _builder.Append(baseUrl);
        if (baseUrl[^1] != '/' && endpoint[0] != '/')
        {
            _builder.Append('/');
        }

        _builder.Append(endpoint);
    }

    public readonly void Dispose() => _builder.Dispose();

    public void AppendParameter(ReadOnlySpan<char> key, ReadOnlySpan<char> value)
        => _builder.Append($"{(ParameterCount++ == 0 ? '?' : '&')}{key}={value}");

    public void AppendParameter<T>(ReadOnlySpan<char> key, T value)
        => _builder.Append($"{(ParameterCount++ == 0 ? '?' : '&')}{key}={value}");

    [Pure]
    public override readonly string ToString() => _builder.ToString();

    public readonly bool Equals(UrlBuilder other) => ParameterCount == other.ParameterCount && _builder.Equals(other._builder);

    public override readonly bool Equals(object? obj) => obj is UrlBuilder other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(_builder, ParameterCount);

    public static bool operator ==(UrlBuilder left, UrlBuilder right) => left.Equals(right);

    public static bool operator !=(UrlBuilder left, UrlBuilder right) => !left.Equals(right);
}
