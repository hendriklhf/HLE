using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.Strings;

namespace HLE.Twitch.Models;

[DebuggerDisplay("{ToString()}")]
public readonly struct OAuthToken : IIndexAccessible<char>, IEquatable<OAuthToken>
{
    public char this[int index] => AsSpan()[index];

    public char this[Index index] => AsSpan()[index];

    public ReadOnlySpan<char> this[Range range] => AsSpan()[range];

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

    [SkipLocalsInit]
    public OAuthToken(ReadOnlyMemory<char> token)
    {
        if (!_tokenPattern.IsMatch(token.Span))
        {
            throw new FormatException($"The OAuthToken is in an invalid format. It needs to match this pattern: {_tokenPattern}");
        }

        ValueStringBuilder builder = stackalloc char[_tokenLength];
        if (!token.Span.StartsWith(_tokenPrefix))
        {
            builder.Append(_tokenPrefix);
        }

        token.Span.ToLowerInvariant(builder.FreeBuffer);
        builder.Advance(token.Length);
        _token = builder.ToString().AsMemory();
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan() => _token.Span;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<char> AsMemory() => _token;

    public bool Equals(OAuthToken other)
    {
        return _token.Equals(other._token) || _token.Span.SequenceEqual(other._token.Span);
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
