using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.Globalization;
using HLE.Memory;

namespace HLE;

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
    public static Span<char> AsMutableSpan(string? str)
    {
        return ((ReadOnlySpan<char>)str).AsMutableSpan();
    }

    public static void Replace(string? str, char oldChar, char newChar)
    {
        Replace((ReadOnlySpan<char>)str, oldChar, newChar);
    }

    public static void Replace(ReadOnlySpan<char> span, char oldChar, char newChar)
    {
        Replace(span.AsMutableSpan(), oldChar, newChar);
    }

    internal static void Replace(Span<char> span, char oldChar, char newChar)
    {
        span.Replace(oldChar, newChar);
    }

    public static void ToLower(string? str, CultureInfo? cultureInfo = null)
    {
        ToLower((ReadOnlySpan<char>)str, cultureInfo);
    }

    public static void ToLower(ReadOnlySpan<char> span, CultureInfo? cultureInfo = null)
    {
        ToLower(span.AsMutableSpan(), cultureInfo);
    }

    internal static void ToLower(Span<char> span, CultureInfo? cultureInfo = null)
    {
        if (!MemoryHelper.UseStackAlloc<char>(span.Length))
        {
            using RentedArray<char> copyArrayBuffer = ArrayPool<char>.Shared.Rent(span.Length);
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

    internal static void ToUpper(Span<char> span, CultureInfo? cultureInfo = null)
    {
        if (!MemoryHelper.UseStackAlloc<char>(span.Length))
        {
            using RentedArray<char> copyArrayBuffer = ArrayPool<char>.Shared.Rent(span.Length);
            span.CopyTo(copyArrayBuffer);
            MemoryExtensions.ToUpper(copyArrayBuffer[..span.Length], span, cultureInfo);
            return;
        }

        Span<char> copyBuffer = stackalloc char[span.Length];
        span.CopyTo(copyBuffer);
        MemoryExtensions.ToUpper(copyBuffer, span, cultureInfo);
    }
}
