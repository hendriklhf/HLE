using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

[InterpolatedStringHandler]
[SuppressMessage("Usage", "CA2213:Disposable fields should be disposed")]
[SuppressMessage("ReSharper", "NotDisposedResourceIsReturned")]
[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
[SuppressMessage("ReSharper", "NotDisposedResource")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable")]
public ref struct PooledInterpolatedStringHandler
{
    public readonly ReadOnlySpan<char> Text => _builder.WrittenSpan;

    private ValueStringBuilder _builder;

    public PooledInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        int capacity = literalLength + formattedCount * AssumedAverageFormattingLength;
        _builder = new(capacity);
    }

    private const int AssumedAverageFormattingLength = 16;

    public void Dispose() => _builder.Dispose();

    public void AppendLiteral(string str) => _builder.Append(str);

    public void AppendFormatted(List<char> chars) => _builder.Append(chars);

    public void AppendFormatted(char[] chars) => _builder.Append(chars);

    public void AppendFormatted(scoped ReadOnlySpan<char> chars) => _builder.Append(chars);

    public void AppendFormatted(char value) => _builder.Append(value);

    public void AppendFormatted<T>(T value)
    {
        if (value is IReadOnlySpanProvider<char>)
        {
            // constrained call to avoid boxing for value types
            ReadOnlySpan<char> span = ((IReadOnlySpanProvider<char>)value).GetReadOnlySpan();
            _builder.EnsureCapacity(_builder.Length + span.Length);
            SpanHelpers<char>.Copy(span, _builder.FreeBufferSpan);
            _builder.Advance(span.Length);
        }

        AppendFormatted(value, null);
    }

    public void AppendFormatted<T>(T value, string? format)
    {
        const int BufferGrowth = 256;

        int charsWritten;
        while (!InterpolatedStringHandlerHelpers.TryFormat(value, _builder.FreeBufferSpan, out charsWritten, format))
        {
            _builder.EnsureCapacity(_builder.Length + BufferGrowth);
        }

        _builder.Advance(charsWritten);
    }

    public override readonly string ToString() => new(Text);

    public string ToStringAndDispose()
    {
        try
        {
            return ToString();
        }
        finally
        {
            Dispose();
        }
    }

    [Pure]
    public readonly bool Equals(PooledInterpolatedStringHandler other) => _builder.Equals(other._builder);

    [Pure]
    public override readonly bool Equals(object? obj) => false;

    [Pure]
    public override readonly int GetHashCode() => _builder.GetHashCode();

    public static bool operator ==(PooledInterpolatedStringHandler left, PooledInterpolatedStringHandler right) => left.Equals(right);

    public static bool operator !=(PooledInterpolatedStringHandler left, PooledInterpolatedStringHandler right) => !(left == right);
}
