using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using HLE.Memory;

namespace HLE.Strings;

/// <summary>
/// Provides methods for <see cref="string"/> manipulation.
/// ⚠️ Only use this class if you completely know what you are doing and how strings work in C#. ⚠️
/// </summary>
public static class StringManipulations
{
    /// <summary>
    /// Creates a mutable <see cref="Span{Char}"/> over a <see cref="string"/>.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that you will be able to mutate.</param>
    /// <returns>A <see cref="Span{Char}"/> representation of the passed-in <see cref="string"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<char> AsMutableSpan(string? str)
    {
        return ((ReadOnlySpan<char>)str).AsMutableSpan();
    }

#if NET8_0_OR_GREATER
    public static void Replace(string? str, char oldChar, char newChar)
    {
        Replace((ReadOnlySpan<char>)str, oldChar, newChar);
    }

    public static void Replace(ReadOnlySpan<char> span, char oldChar, char newChar)
    {
        Replace(span.AsMutableSpan(), oldChar, newChar);
    }

    private static void Replace(Span<char> span, char oldChar, char newChar)
    {
        if (span.Length == 0)
        {
            return;
        }

        span.Replace(oldChar, newChar);
    }
#endif

    public static void ToLower(string? str, CultureInfo? cultureInfo = null)
    {
        ToLower((ReadOnlySpan<char>)str, cultureInfo);
    }

    public static void ToLower(ReadOnlySpan<char> span, CultureInfo? cultureInfo = null)
    {
        ToLower(span.AsMutableSpan(), cultureInfo);
    }

    private static void ToLower(Span<char> span, CultureInfo? cultureInfo = null)
    {
        if (span.Length == 0)
        {
            return;
        }

        if (!MemoryHelper.UseStackAlloc<char>(span.Length))
        {
            using RentedArray<char> copyArrayBuffer = new(span.Length);
            span.CopyTo(copyArrayBuffer);
            MemoryExtensions.ToLower(copyArrayBuffer[..span.Length], span, cultureInfo);
            return;
        }

        Span<char> copyBuffer = stackalloc char[span.Length];
        span.CopyTo(copyBuffer);
        MemoryExtensions.ToLower(copyBuffer, span, cultureInfo);
    }

    public static void ToUpper(string? str, CultureInfo? cultureInfo = null)
    {
        ToUpper((ReadOnlySpan<char>)str, cultureInfo);
    }

    public static void ToUpper(ReadOnlySpan<char> span, CultureInfo? cultureInfo = null)
    {
        ToUpper(span.AsMutableSpan(), cultureInfo);
    }

    private static void ToUpper(Span<char> span, CultureInfo? cultureInfo = null)
    {
        if (!MemoryHelper.UseStackAlloc<char>(span.Length))
        {
            using RentedArray<char> copyArrayBuffer = new(span.Length);
            span.CopyTo(copyArrayBuffer);
            MemoryExtensions.ToUpper(copyArrayBuffer[..span.Length], span, cultureInfo);
            return;
        }

        Span<char> copyBuffer = stackalloc char[span.Length];
        span.CopyTo(copyBuffer);
        MemoryExtensions.ToUpper(copyBuffer, span, cultureInfo);
    }
}
