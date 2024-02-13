using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Marshalling;

public static class StringMarshal
{
    internal static readonly unsafe delegate*<int, string> s_fastAllocateString = (delegate*<int, string>)typeof(string)
        .GetMethod("FastAllocateString", BindingFlags.NonPublic | BindingFlags.Static)!
        .MethodHandle
        .GetFunctionPointer();

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string FastAllocateString(int length, out Span<char> chars)
    {
        if (length == 0)
        {
            chars = [];
            return string.Empty;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length); // otherwise an OutOfMemoryException will be thrown

        string str = s_fastAllocateString(length);
        chars = MemoryMarshal.CreateSpan(ref GetReference(str), length);
        return str;
    }

    /// <summary>
    /// Creates a mutable <see cref="Span{Char}"/> over a <see cref="string"/>.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that will be mutable.</param>
    /// <returns>A <see cref="Span{Char}"/> representation of the passed-in <see cref="string"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<char> AsMutableSpan(string str)
        => MemoryMarshal.CreateSpan(ref GetReference(str), str.Length);

    public static void Replace(string? str, char oldChar, char newChar) => Replace(str.AsSpan(), oldChar, newChar);

    public static void Replace(ReadOnlySpan<char> span, char oldChar, char newChar) => Replace(SpanMarshal.AsMutableSpan(span), oldChar, newChar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Replace(Span<char> span, char oldChar, char newChar)
    {
        if (span.Length == 0)
        {
            return;
        }

        span.Replace(oldChar, newChar);
    }

    public static void ToLower(string? str) => ToLower(str.AsSpan());

    public static void ToLower(ReadOnlySpan<char> span) => ToLower(SpanMarshal.AsMutableSpan(span));

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ToLower(Span<char> span)
    {
        if (span.Length == 0)
        {
            return;
        }

        if (!MemoryHelpers.UseStackalloc<char>(span.Length))
        {
            using RentedArray<char> rentedCopyBuffer = ArrayPool<char>.Shared.RentAsRentedArray(span.Length);
            span.CopyTo(rentedCopyBuffer.AsSpan());
            MemoryExtensions.ToLowerInvariant(rentedCopyBuffer[..span.Length], span);
            return;
        }

        Span<char> copyBuffer = stackalloc char[span.Length];
        span.CopyTo(copyBuffer);
        MemoryExtensions.ToLowerInvariant(copyBuffer, span);
    }

    public static void ToUpper(string? str) => ToUpper(str.AsSpan());

    public static void ToUpper(ReadOnlySpan<char> span) => ToUpper(SpanMarshal.AsMutableSpan(span));

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ToUpper(Span<char> span)
    {
        if (span.Length == 0)
        {
            return;
        }

        if (!MemoryHelpers.UseStackalloc<char>(span.Length))
        {
            using RentedArray<char> rentedCopyBuffer = ArrayPool<char>.Shared.RentAsRentedArray(span.Length);
            span.CopyTo(rentedCopyBuffer.AsSpan());
            MemoryExtensions.ToUpperInvariant(rentedCopyBuffer[..span.Length], span);
            return;
        }

        Span<char> copyBuffer = stackalloc char[span.Length];
        span.CopyTo(copyBuffer);
        MemoryExtensions.ToUpperInvariant(copyBuffer, span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear(string? str)
    {
        if (str is null)
        {
            return;
        }

        Unsafe.InitBlock(ref Unsafe.As<char, byte>(ref GetReference(str)), 0, (uint)(str.Length << 1));
    }

    /// <inheritdoc cref="AsString(System.ReadOnlySpan{char})"/>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsString(Span<char> span) => AsString((ReadOnlySpan<char>)span);

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{Char}"/> back to a <see cref="string"/>.
    /// ⚠️ The caller has to ensure that the <see cref="ReadOnlySpan{Char}"/> was definitely a <see cref="string"/>,
    /// otherwise this method is potentially dangerous. ⚠️
    /// </summary>
    /// <param name="span">The <see cref="ReadOnlySpan{Char}"/> that will be converted to a <see cref="string"/>.</param>
    /// <returns>The <see cref="ReadOnlySpan{Char}"/> as a <see cref="string"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string AsString(ReadOnlySpan<char> span)
    {
        if (span.Length == 0)
        {
            return string.Empty;
        }

        ref byte charsReference = ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(span));
        charsReference = ref Unsafe.Subtract(ref charsReference, sizeof(int) + sizeof(nuint));
        return ObjectMarshal.ReadObject<string, byte>(ref charsReference);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref char GetReference(string str)
    {
        byte* ptr = *(byte**)&str;
        return ref Unsafe.AsRef<char>(ptr + sizeof(nuint) + sizeof(int));
    }
}
