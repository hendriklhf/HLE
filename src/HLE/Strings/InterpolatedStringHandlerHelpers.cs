using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Strings;

internal static class InterpolatedStringHandlerHelpers
{
    // TODO: move method into *StringBuilder classes as Append<T>(T)
    public static bool TryFormat<T>(T value, Span<char> destination, out int charsWritten, string? format)
    {
        if (typeof(T).IsEnum)
        {
            return TryFormatEnum(null, value, destination, out charsWritten, format);
        }

        string? str;
        if (value is IFormattable)
        {
            if (value is ISpanFormattable)
            {
                // constrained call to avoid boxing for value types
                return ((ISpanFormattable)value).TryFormat(destination, out charsWritten, format, null);
            }

            // constrained call to avoid boxing for value types
            str = ((IFormattable)value).ToString(format, null);
            return TryCopyTo(str, destination, out charsWritten);
        }

        str = value?.ToString() ?? string.Empty;
        return TryCopyTo(str, destination, out charsWritten);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryCopyTo(string str, Span<char> destination, out int charsWritten)
    {
        if (str.TryCopyTo(destination))
        {
            charsWritten = str.Length;
            return true;
        }

        charsWritten = 0;
        return true;
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "TryFormatUnconstrained")]
    private static extern bool TryFormatEnum<TEnum>(Enum? c, TEnum value, Span<char> destination, out int charsWritten,
        [StringSyntax(StringSyntaxAttribute.EnumFormat)] ReadOnlySpan<char> format = default);
}
