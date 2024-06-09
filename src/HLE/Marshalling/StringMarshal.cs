using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.IL;
using HLE.Memory;
using HLE.Text;

namespace HLE.Marshalling;

public static unsafe class StringMarshal
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FastAllocateString(int length, out Span<char> chars)
    {
        if (length == 0)
        {
            chars = [];
            return string.Empty;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length); // otherwise an OutOfMemoryException will be thrown

        string str = FastAllocateStringCore(null, length);
        chars = MemoryMarshal.CreateSpan(ref GetReference(str), length);
        return str;
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "FastAllocateString")]
    private static extern string FastAllocateStringCore(string? s, int length);

    /// <summary>
    /// Creates a mutable <see cref="Span{Char}"/> over a <see cref="string"/>.
    /// </summary>
    /// <param name="str">The <see cref="string"/> that will be mutable.</param>
    /// <returns>A <see cref="Span{Char}"/> representation of the passed-in <see cref="string"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<char> AsMutableSpan(string str) => MemoryMarshal.CreateSpan(ref GetReference(str), str.Length);

    public static void Replace(string? str, char oldChar, char newChar) => Replace(str.AsSpan(), oldChar, newChar);

    public static void Replace(ref PooledInterpolatedStringHandler span, char oldChar, char newChar) => Replace(span.Text, oldChar, newChar);

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

    public static void ToLower(ref PooledInterpolatedStringHandler span) => ToLower(span.Text);

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
            SpanHelpers<char>.Copy(span, rentedCopyBuffer.AsSpan());
            MemoryExtensions.ToLowerInvariant(rentedCopyBuffer[..span.Length], span);
            return;
        }

        Span<char> copyBuffer = stackalloc char[span.Length];
        SpanHelpers<char>.Copy(span, copyBuffer);
        MemoryExtensions.ToLowerInvariant(copyBuffer, span);
    }

    public static void ToUpper(string? str) => ToUpper(str.AsSpan());

    public static void ToUpper(ref PooledInterpolatedStringHandler span) => ToUpper(span.Text);

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
            SpanHelpers<char>.Copy(span, rentedCopyBuffer.AsSpan());
            MemoryExtensions.ToUpperInvariant(rentedCopyBuffer[..span.Length], span);
            return;
        }

        Span<char> copyBuffer = stackalloc char[span.Length];
        SpanHelpers<char>.Copy(span, copyBuffer);
        MemoryExtensions.ToUpperInvariant(copyBuffer, span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear(string str) => Unsafe.InitBlock(ref Unsafe.As<char, byte>(ref GetReference(str)), 0, (uint)(str.Length << 1));

    /// <inheritdoc cref="AsString(ReadOnlySpan{char})"/>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsString(Span<char> span)
        => span.Length == 0 ? string.Empty : AsString(ref MemoryMarshal.GetReference(span));

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{Char}"/> back to a <see cref="string"/>.
    /// ⚠️ The caller has to ensure that the <see cref="ReadOnlySpan{Char}"/> was definitely a <see cref="string"/>,
    /// otherwise this method is potentially dangerous. ⚠️
    /// </summary>
    /// <param name="span">The <see cref="ReadOnlySpan{Char}"/> that will be converted to a <see cref="string"/>.</param>
    /// <returns>The <see cref="ReadOnlySpan{Char}"/> as a <see cref="string"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsString(ReadOnlySpan<char> span)
        => span.Length == 0 ? string.Empty : AsString(ref MemoryMarshal.GetReference(span));

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string AsString(ref char chars) => UnsafeIL.AsString(ref chars);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref char GetReference(string str) => ref GetReferenceCore(str);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_firstChar")]
    private static extern ref char GetReferenceCore(string str);
}
