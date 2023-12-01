using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

    private const string TokenPrefix = "oauth:";
    private const int TokenLength = 36;

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
            ThrowInvalidOAuthTokenFormat();
        }

        ValueStringBuilder builder = new(stackalloc char[TokenLength]);
        if (!token.StartsWith(TokenPrefix))
        {
            builder.Append(TokenPrefix);
        }

        token.ToLowerInvariant(builder.FreeBuffer);
        builder.Advance(token.Length);
        _token = StringPool.Shared.GetOrAdd(builder.WrittenSpan);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowInvalidOAuthTokenFormat()
        => throw new FormatException($"The OAuthToken is in an invalid format. It needs to match this pattern: {GetTokenPattern()}");

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan() => _token;

    [Pure]
    public bool Equals(OAuthToken other) => _token == other._token;

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is OAuthToken other && Equals(other);

    [Pure]
    public override int GetHashCode() => string.GetHashCode(_token);

    [Pure]
    public override string ToString() => _token;

    public static bool operator ==(OAuthToken left, OAuthToken right) => left.Equals(right);

    public static bool operator !=(OAuthToken left, OAuthToken right) => !(left == right);
}
