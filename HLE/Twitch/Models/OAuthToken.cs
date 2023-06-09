using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HLE.Strings;

namespace HLE.Twitch.Models;

[DebuggerDisplay("{ToString()}")]
public readonly struct OAuthToken : IEquatable<OAuthToken>
{
    private readonly ReadOnlyMemory<char> _token = ReadOnlyMemory<char>.Empty;

    public static OAuthToken Empty => new();

    private static readonly Regex _tokenPattern = RegexPool.Shared.GetOrAdd(@"^(oauth:)?[a-z0-9]{30}$", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

    private const string _tokenPrefix = "oauth:";
    private const int _tokenLength = 36;

    public OAuthToken()
    {
    }

    public OAuthToken(string token) : this(token.AsMemory())
    {
    }

    public OAuthToken(ReadOnlyMemory<char> token)
    {
        if (!_tokenPattern.IsMatch(token.Span))
        {
            throw new FormatException($"The OAuthToken is in an invalid format. It needs to match this pattern: {_tokenPattern}");
        }

        if (!token.Span.StartsWith(_tokenPrefix))
        {
            ValueStringBuilder builder = stackalloc char[_tokenLength];
            builder.Append(_tokenPrefix, token.Span);
            token = builder.ToString().AsMemory();
        }

        _token = token;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan() => _token.Span;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<char> AsMemory() => _token;

    public bool Equals(OAuthToken other)
    {
        return _token.Span.SequenceEqual(other._token.Span);
    }

    public override bool Equals(object? obj)
    {
        return obj is OAuthToken other && Equals(other);
    }

    public override int GetHashCode()
    {
        return string.GetHashCode(_token.Span);
    }

    public override string ToString()
    {
        return new(_token.Span);
    }

    public static bool operator ==(OAuthToken left, OAuthToken right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(OAuthToken left, OAuthToken right)
    {
        return !(left == right);
    }
}
