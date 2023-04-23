using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using HLE.Strings;

namespace HLE.Twitch.Api;

[DebuggerDisplay("{ToString()}")]
internal struct UrlBuilder : IDisposable
{
    private PoolBufferStringBuilder _stringBuilder;

    public int ParameterCount { get; private set; }

    public UrlBuilder(ReadOnlySpan<char> baseUrl, ReadOnlySpan<char> endpoint, int initialBufferLength = 100)
    {
        _stringBuilder = new(initialBufferLength);
        _stringBuilder.Append(baseUrl);
        if (endpoint[0] != '/')
        {
            _stringBuilder.Append('/');
        }

        _stringBuilder.Append(endpoint);
    }

    public readonly void Dispose()
    {
        _stringBuilder.Dispose();
    }

    public void AppendParameter(ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        _stringBuilder.Append(ParameterCount == 0 ? '?' : '&');
        _stringBuilder.Append(key);
        _stringBuilder.Append('=');
        _stringBuilder.Append(value);
        ParameterCount++;
    }

    public void AppendParameter(ReadOnlySpan<char> key, long value)
    {
        _stringBuilder.Append(ParameterCount == 0 ? '?' : '&');
        _stringBuilder.Append(key);
        _stringBuilder.Append('=');
        _stringBuilder.Append(value);
        ParameterCount++;
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly string ToString()
    {
        return _stringBuilder.ToString();
    }
}
