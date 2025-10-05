using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Text;

namespace HLE.Twitch.Tmi;

[DebuggerDisplay("{ToString()}")]
public readonly partial struct OAuthToken : IEquatable<OAuthToken>
{
    private readonly string _token;

    public static OAuthToken Empty => new();

    [GeneratedRegex("^(oauth:)?[a-z0-9]{30}$", RegexOptions.Compiled | RegexOptions.IgnoreCase, 250)]
#if NET9_0_OR_GREATER
    private static partial Regex TokenPattern { get; }
#else
    private static partial Regex GetTokenPattern();
#endif

    private const string TokenPrefix = "oauth:";
    private const int TokenLength = 30;
    private const int TotalTokenLength = 36;

    public OAuthToken() => _token = string.Empty;

    public OAuthToken(string token)
        => _token = CtorCore(token, true);

    public OAuthToken(ReadOnlySpan<char> token)
        => _token = CtorCore(token, false);

    private static string CtorCore(ReadOnlySpan<char> token, [ConstantExpected] bool wasString)
    {
#if NET9_0_OR_GREATER
        if (!TokenPattern.IsMatch(token))
#else
        if (!GetTokenPattern().IsMatch(token))
#endif
        {
            ThrowInvalidOAuthTokenFormat();
        }

        if (token.Length == TotalTokenLength)
        {
            return wasString ? StringMarshal.AsString(token) : new(token);
        }

        UnsafeBufferWriter<char> builder = new(stackalloc char[TotalTokenLength]);
        builder.Write(TokenPrefix);
        token.ToLowerInvariant(builder.GetSpan(TokenLength));
        builder.Advance(token.Length);
        return StringPool.Shared.GetOrAdd(builder.WrittenSpan);
    }

    [DoesNotReturn]
    private static void ThrowInvalidOAuthTokenFormat()
    {
#if NET9_0_OR_GREATER
        Regex regex = TokenPattern;
#else
        Regex regex = GetTokenPattern();
#endif
        throw new FormatException($"The OAuthToken is in an invalid format. It needs to match this pattern: {regex}");
    }

    [Pure]
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
