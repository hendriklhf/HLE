using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using HLE.Strings;

namespace HLE.Twitch.Helix;

[DebuggerDisplay("{ToString()}")]
internal struct UrlBuilder : IDisposable
{
    private PoolBufferStringBuilder _stringBuilder;

    public int ParameterCount { get; private set; }

    public UrlBuilder(ReadOnlySpan<char> baseUrl, ReadOnlySpan<char> endpoint, int initialBufferLength = 100)
    {
        _stringBuilder = new(initialBufferLength);
        _stringBuilder.Append(baseUrl);
        if (baseUrl[^1] != '/' && endpoint[0] != '/')
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

    public void AppendParameter<T>(ReadOnlySpan<char> key, T value) where T : ISpanFormattable
    {
        _stringBuilder.Append(ParameterCount == 0 ? '?' : '&');
        _stringBuilder.Append(key);
        _stringBuilder.Append('=');
        _stringBuilder.Append<T, IFormatProvider>(value);
        ParameterCount++;
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly string ToString()
    {
        return ParameterCount == 0 ? StringPool.Shared.GetOrAdd(_stringBuilder.WrittenSpan) : _stringBuilder.ToString();
    }
}
