using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.Strings;

namespace HLE.Twitch.Models;

[DebuggerDisplay("{ToString()}")]
public readonly partial struct OAuthToken : IIndexAccessible<char>, ICountable, IEquatable<OAuthToken>
{
    public char this[int index] => _token[index];

    public char this[Index index] => _token[index];

    public ReadOnlySpan<char> this[Range range] => _token.AsSpan(range);

    int ICountable.Count => _token.Length;

    private readonly string _token = string.Empty;

    public static OAuthToken Empty => new();

    [GeneratedRegex("^(oauth:)?[a-z0-9]{30}?", RegexOptions.Compiled | RegexOptions.IgnoreCase, 250)]
    private static partial Regex GetTokenPattern();

    private const string _tokenPrefix = "oauth:";
    private const int _tokenLength = 36;

    public OAuthToken()
    {
    }

    public OAuthToken(string token) : this(token.AsSpan())
    {
    }

    [SkipLocalsInit]
    public OAuthToken(ReadOnlySpan<char> token)
    {
        if (!GetTokenPattern().IsMatch(token))
        {
            throw new FormatException($"The OAuthToken is in an invalid format. It needs to match this pattern: {GetTokenPattern()}");
        }

        ValueStringBuilder builder = new(stackalloc char[_tokenLength]);
        if (!token.StartsWith(_tokenPrefix))
        {
            builder.Append(_tokenPrefix);
        }

        token.ToLowerInvariant(builder.FreeBuffer);
        builder.Advance(token.Length);
        _token = StringPool.Shared.GetOrAdd(builder.WrittenSpan);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan() => _token;

    public bool Equals(OAuthToken other)
    {
        return _token == other._token;
    }

    public override bool Equals(object? obj)
    {
        return obj is OAuthToken other && Equals(other);
    }

    public override int GetHashCode()
    {
        return string.GetHashCode(_token);
    }

    public override string ToString()
    {
        return _token;
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
