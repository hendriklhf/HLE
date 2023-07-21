using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using HLE.Memory;

namespace HLE.Marshals;

/// <summary>
/// Provides methods for <see cref="string"/> manipulation.<br/>
/// </summary>
public static class StringMarshal
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

    public static void Replace(string? str, char oldChar, char newChar)
    {
        Replace((ReadOnlySpan<char>)str, oldChar, newChar);
    }

    public static void Replace(ReadOnlySpan<char> span, char oldChar, char newChar)
    {
        Replace(span.AsMutableSpan(), oldChar, newChar);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Replace(Span<char> span, char oldChar, char newChar)
    {
        if (span.Length == 0)
        {
            return;
        }

        span.Replace(oldChar, newChar);
    }

    public static void ToLower(string? str)
    {
        ToLower((ReadOnlySpan<char>)str);
    }

    public static void ToLower(ReadOnlySpan<char> span)
    {
        ToLower(span.AsMutableSpan());
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ToLower(Span<char> span)
    {
        if (span.Length == 0)
        {
            return;
        }

        if (!MemoryHelper.UseStackAlloc<char>(span.Length))
        {
            using RentedArray<char> rentedCopyBuffer = new(span.Length);
            span.CopyTo(rentedCopyBuffer);
            MemoryExtensions.ToLower(rentedCopyBuffer[..span.Length], span, CultureInfo.InvariantCulture);
            return;
        }

        Span<char> copyBuffer = stackalloc char[span.Length];
        span.CopyTo(copyBuffer);
        MemoryExtensions.ToLower(copyBuffer, span, CultureInfo.InvariantCulture);
    }

    public static void ToUpper(string? str)
    {
        ToUpper((ReadOnlySpan<char>)str);
    }

    public static void ToUpper(ReadOnlySpan<char> span)
    {
        ToUpper(span.AsMutableSpan());
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ToUpper(Span<char> span)
    {
        if (!MemoryHelper.UseStackAlloc<char>(span.Length))
        {
            using RentedArray<char> rentedCopyBuffer = new(span.Length);
            span.CopyTo(rentedCopyBuffer);
            MemoryExtensions.ToUpper(rentedCopyBuffer[..span.Length], span, CultureInfo.InvariantCulture);
            return;
        }

        Span<char> copyBuffer = stackalloc char[span.Length];
        span.CopyTo(copyBuffer);
        MemoryExtensions.ToUpper(copyBuffer, span, CultureInfo.InvariantCulture);
    }
}
