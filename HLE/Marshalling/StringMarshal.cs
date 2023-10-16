using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Marshalling;

public static class StringMarshal
{
    private static readonly unsafe delegate*<int, string> _fastAllocateString = (delegate*<int, string>)typeof(string).GetMethod("FastAllocateString", BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string FastAllocateString(int length, out Span<char> chars)
    {
        string str = _fastAllocateString(length);
        chars = AsMutableSpan(str);
        return str;
    }

    /// <summary>
    /// Creates a mutable <see cref="Span{Char}"/> over a <see cref="string"/>.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that you will be able to mutate.</param>
    /// <returns>A <see cref="Span{Char}"/> representation of the passed-in <see cref="string"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<char> AsMutableSpan(string? str) => str.AsSpan().AsMutableSpan();

    public static void Replace(string? str, char oldChar, char newChar) => Replace(str.AsSpan(), oldChar, newChar);

    public static void Replace(ReadOnlySpan<char> span, char oldChar, char newChar) => Replace(span.AsMutableSpan(), oldChar, newChar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Replace(Span<char> span, char oldChar, char newChar)
    {
        if (span.Length == 0)
        {
            return;
        }

        span.Replace(oldChar, newChar);
    }

    public static void ToLower(string? str) => ToLower((ReadOnlySpan<char>)str);

    public static void ToLower(ReadOnlySpan<char> span) => ToLower(span.AsMutableSpan());

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
            using RentedArray<char> rentedCopyBuffer = ArrayPool<char>.Shared.CreateRentedArray(span.Length);
            span.CopyTo(rentedCopyBuffer.AsSpan());
            MemoryExtensions.ToLower(rentedCopyBuffer[..span.Length], span, CultureInfo.InvariantCulture);
            return;
        }

        Span<char> copyBuffer = stackalloc char[span.Length];
        span.CopyTo(copyBuffer);
        MemoryExtensions.ToLower(copyBuffer, span, CultureInfo.InvariantCulture);
    }

    public static void ToUpper(string? str) => ToUpper((ReadOnlySpan<char>)str);

    public static void ToUpper(ReadOnlySpan<char> span) => ToUpper(span.AsMutableSpan());

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ToUpper(Span<char> span)
    {
        if (!MemoryHelper.UseStackAlloc<char>(span.Length))
        {
            using RentedArray<char> rentedCopyBuffer = ArrayPool<char>.Shared.CreateRentedArray(span.Length);
            span.CopyTo(rentedCopyBuffer.AsSpan());
            MemoryExtensions.ToUpper(rentedCopyBuffer[..span.Length], span, CultureInfo.InvariantCulture);
            return;
        }

        Span<char> copyBuffer = stackalloc char[span.Length];
        span.CopyTo(copyBuffer);
        MemoryExtensions.ToUpper(copyBuffer, span, CultureInfo.InvariantCulture);
    }

    /// <inheritdoc cref="AsString(System.ReadOnlySpan{char})"/>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsString(Span<char> span) => AsString((ReadOnlySpan<char>)span);

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{T}"/> back to a <see cref="string"/>.
    /// The caller has to be sure that the <see cref="ReadOnlySpan{T}"/> was definitely a <see cref="string"/>,
    /// otherwise this method is potentially dangerous.
    /// </summary>
    /// <param name="span">The <see cref="ReadOnlySpan{T}"/> that will be converted to a <see cref="string"/>.</param>
    /// <returns>The <see cref="ReadOnlySpan{T}"/> as a <see cref="string"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string AsString(ReadOnlySpan<char> span)
    {
        ref byte charsReference = ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(span));
        charsReference = ref Unsafe.Subtract(ref charsReference, sizeof(int) + sizeof(nuint));
        return RawDataMarshal.ReadObject<string>(ref charsReference);
    }
}
