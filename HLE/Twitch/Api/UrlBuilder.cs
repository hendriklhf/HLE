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
        if (endpoint[^1] != '?')
        {
            _stringBuilder.Append('?');
        }
    }

    public readonly void Dispose()
    {
        _stringBuilder.Dispose();
    }

    public void AppendParameter(ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        if (_stringBuilder.WrittenSpan[^1] != '?')
        {
            _stringBuilder.Append('&');
        }

        _stringBuilder.Append(key);
        _stringBuilder.Append('=');
        _stringBuilder.Append(value);
        ParameterCount++;
    }

    public void AppendParameter(ReadOnlySpan<char> key, long value)
    {
        if (_stringBuilder.WrittenSpan[^1] != '?')
        {
            _stringBuilder.Append('&');
        }

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
