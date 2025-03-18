using System;
using System.Collections.Generic;

namespace HLE.Text;

public interface IInterpolatedStringHandler
{
    void AppendLiteral(string str);

    void AppendFormatted(string? str);

    void AppendFormatted(LazyString? str);

    void AppendFormatted(List<char>? chars);

    void AppendFormatted(char[]? chars);

    void AppendFormatted(ReadOnlyMemory<char> chars);

    void AppendFormatted(ReadOnlySpan<char> chars);

    void AppendFormatted(char value);

    void AppendFormatted<T>(T value);

    void AppendFormatted<T>(T value, string? format);
}
