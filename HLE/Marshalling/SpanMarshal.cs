using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

public static unsafe class SpanMarshal
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsMutableSpan<T>(ReadOnlySpan<T> span)
    {
        ref T reference = ref MemoryMarshal.GetReference(span);
        return MemoryMarshal.CreateSpan(ref reference, span.Length);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<T> AsMutableMemory<T>(ReadOnlyMemory<T> memory)
        => Unsafe.As<ReadOnlyMemory<T>, Memory<T>>(ref memory);

    /// <inheritdoc cref="AsMemory{T}(ReadOnlySpan{T})"/>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<T> AsMemory<T>(Span<T> span)
        => AsMutableMemory(AsMemory((ReadOnlySpan<T>)span));

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{T}"/> to a <see cref="ReadOnlyMemory{T}"/>. Does not allocate any memory. <br/>
    /// ⚠️ Only works if the span's reference points to the first element of an <see cref="Array"/> of type <typeparamref name="T"/>. Otherwise this method is potentially dangerous. ⚠️
    /// </summary>
    /// <param name="span">The span that will be converted.</param>
    /// <returns>A memory view over the span.</returns>
    [Pure]
    [SkipLocalsInit]
    public static ReadOnlyMemory<T> AsMemory<T>(ReadOnlySpan<T> span)
    {
        Unsafe.SkipInit(out ReadOnlyMemory<T> result);

        RawMemoryData* rawMemoryData = (RawMemoryData*)&result;

        ref byte reference = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span));
        reference = ref Unsafe.Subtract(ref reference, sizeof(nuint) << 1);
        nuint arrayMethodTablePointer = (nuint)Unsafe.AsPointer(ref reference);

        Debug.Assert(RawDataMarshal.ReadObject<T[]>(arrayMethodTablePointer) is { } array && array.Length >= span.Length);

        rawMemoryData->Reference = arrayMethodTablePointer;
        rawMemoryData->Index = 0;
        rawMemoryData->Length = span.Length;

        return result;
    }

    /// <summary>
    /// Returns the <see cref="Span{T}"/> that has been passed in. The method can be used to avoid a compiler error,
    /// but comes at the cost that the caller has to guarantee the safety of the code by themself.<br/>
    /// </summary>
    /// <example>
    /// This code produces a compiler error:
    /// <br/><code>
    /// Span&lt;T&gt; span = [];
    /// if (span.Length == 0)
    /// {
    ///     span = stackalloc int[50]; // CS8353: A result of a 'stackalloc' expression cannot be used in this context because it may be exposed outside of the containing method
    /// }
    /// </code><br/>
    /// The error can be avoided by using this method:
    /// <br/><code>
    /// Span&lt;T&gt; span = [];
    /// if (span.Length == 0)
    /// {
    ///     span = MemoryHelper.ReturnStackAlloced(stackalloc int[50]);
    /// }
    /// </code>
    /// </example>
    /// <param name="span">The <see cref="Span{T}"/> that will be returned.</param>
    /// <typeparam name="T">The type of elements in the <see cref="Span{T}"/>.</typeparam>
    /// <returns>The <see cref="Span{T}"/> that has been passed in.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope
    public static Span<T> ReturnStackAlloced<T>(scoped Span<T> span) => span;
#pragma warning restore CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope
}
