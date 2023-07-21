using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public static unsafe class MemoryHelper
{
    public static int MaxStackAllocSize { get; set; } = sizeof(nuint) << (Environment.Is64BitProcess ? 10 : 9);

    /// <summary>
    /// Determines whether to use a stack or a heap allocation by passing a generic type and the element count.
    /// The default maximum stack allocation size is set to 10.000 bytes for 64-bit processes and to 2500 bytes for 32-bit processes.
    /// The default maximum can also be changed with the <see cref="MaxStackAllocSize"/> property.
    /// </summary>
    /// <param name="elementCount">The amount of elements wanted to be stack allocated.</param>
    /// <typeparam name="T">The type of the <see langword="stackalloc"/>.</typeparam>
    /// <returns>True, if a stackalloc can be used, otherwise false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool UseStackAlloc<T>(int elementCount)
    {
        int totalByteSize = sizeof(T) * elementCount;
        return totalByteSize <= MaxStackAllocSize;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsMutableSpan<T>(this ReadOnlySpan<T> span)
    {
        return *(Span<T>*)&span;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<T> AsMutableMemory<T>(this ReadOnlyMemory<T> memory)
    {
        return *(Memory<T>*)&memory;
    }

    /// <inheritdoc cref="AsMemoryUnsafe{T}(ReadOnlySpan{T})"/>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<T> AsMemoryUnsafe<T>(this Span<T> span)
    {
        return AsMemoryUnsafe((ReadOnlySpan<T>)span).AsMutableMemory();
    }

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{T}"/> to a <see cref="ReadOnlyMemory{T}"/>. Does not allocate any memory. <br/>
    /// ⚠️ Only works if the span's reference points to the first element of an <see cref="Array"/> of type <typeparamref name="T"/>. Otherwise this method is potentially dangerous. ⚠️
    /// </summary>
    /// <param name="span">The span that will be converted.</param>
    /// <returns>A memory view over the span.</returns>
    [Pure]
    public static ReadOnlyMemory<T> AsMemoryUnsafe<T>(this ReadOnlySpan<T> span)
    {
        Unsafe.SkipInit(out ReadOnlyMemory<T> result);
        byte* spanPointerAsBytePointer = (byte*)&span;
        byte* memoryPointerAsBytePointer = (byte*)&result;

        // pointers to the three fields Memory<T> consists off
        nuint* memoryReferenceField = (nuint*)memoryPointerAsBytePointer;
        int* memoryIndexField = (int*)(memoryPointerAsBytePointer + sizeof(nuint));
        int* memoryLengthField = (int*)(memoryPointerAsBytePointer + sizeof(nuint) + sizeof(int));

        nuint spanReferenceFieldValue = *(nuint*)spanPointerAsBytePointer - (nuint)(sizeof(nuint) << 1);
        *memoryReferenceField = spanReferenceFieldValue;

        *memoryIndexField = 0;

        int memoryLength = *(int*)(spanPointerAsBytePointer + sizeof(nuint));
        *memoryLengthField = memoryLength;

        return result;
    }

    /// <inheritdoc cref="AsStringUnsafe(System.ReadOnlySpan{char})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsStringUnsafe(this Span<char> span)
    {
        return AsStringUnsafe((ReadOnlySpan<char>)span);
    }

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{T}"/> back to a <see cref="string"/>.
    /// The caller has to be sure that the <see cref="ReadOnlySpan{T}"/> was definitely a <see cref="string"/>,
    /// otherwise this method is potentially dangerous.
    /// </summary>
    /// <param name="span">The <see cref="ReadOnlySpan{T}"/> that will be converted to a <see cref="string"/>.</param>
    /// <returns>The <see cref="ReadOnlySpan{T}"/> as a <see cref="string"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsStringUnsafe(this ReadOnlySpan<char> span)
    {
        ref char charsReference = ref MemoryMarshal.GetReference(span);
        ref byte charsAsBytesReference = ref Unsafe.As<char, byte>(ref charsReference);
        ref byte stringDataReference = ref Unsafe.Subtract(ref charsAsBytesReference, sizeof(nuint) + sizeof(int));
        return GetReferenceFromRawDataPointer<byte, string>(ref stringDataReference)!;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetRawDataPointer<T>(T reference) where T : class?
    {
        return *(nuint*)&reference;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue? GetReferenceFromRawDataPointer<TRef, TValue>(ref TRef reference) where TValue : class?
    {
        TValue* pointer = (TValue*)Unsafe.AsPointer(ref reference);
        return GetReferenceFromRawDataPointer<TValue>((nuint)pointer);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetReferenceFromRawDataPointer<T>(void* pointer) where T : class?
    {
        return GetReferenceFromRawDataPointer<T>((nuint)pointer);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetReferenceFromRawDataPointer<T>(nuint pointer) where T : class?
    {
        return *(T*)&pointer;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> GetStructBytes<T>(ref T item) where T : struct
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref item), sizeof(T));
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> GetStructBytes<T>(T item) where T : struct
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref item), sizeof(T));
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsBitwise<TLeft, TRight>(this TLeft left, TRight right) where TLeft : struct where TRight : struct
    {
        return EqualsBitwise(ref left, ref right);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsBitwise<TLeft, TRight>(this ref TLeft left, ref TRight right) where TLeft : struct where TRight : struct
    {
        if (sizeof(TLeft) != sizeof(TRight))
        {
            return false;
        }

        switch (sizeof(TLeft))
        {
            case sizeof(byte):
                return Unsafe.As<TLeft, byte>(ref left) == Unsafe.As<TRight, byte>(ref right);
            case sizeof(short):
                return Unsafe.As<TLeft, short>(ref left) == Unsafe.As<TRight, short>(ref right);
            case sizeof(int):
                return Unsafe.As<TLeft, int>(ref left) == Unsafe.As<TRight, int>(ref right);
            case sizeof(long):
                return Unsafe.As<TLeft, long>(ref left) == Unsafe.As<TRight, long>(ref right);
        }

        if (Unsafe.AreSame(ref Unsafe.As<TLeft, byte>(ref left), ref Unsafe.As<TRight, byte>(ref right)))
        {
            return true;
        }

        ReadOnlySpan<byte> leftBytes = GetStructBytes(ref left);
        ReadOnlySpan<byte> rightBytes = GetStructBytes(ref right);
        return leftBytes.SequenceEqual(rightBytes);
    }

    /// <inheritdoc cref="CopyToUnsafe{T}(ReadOnlySpan{T},Span{T})"/>
    public static void CopyToUnsafe<T>(this Span<T> source, Span<T> destination)
    {
        CopyToUnsafe((ReadOnlySpan<T>)source, destination);
    }

    /// <summary>
    /// Copies the <paramref name="source"/> to the <paramref name="destination"/> without checking if enough space is available, so the caller has to verify that it is safe.
    /// </summary>
    /// <param name="source">The items that will be copied.</param>
    /// <param name="destination">The destination where the items will be copied to.</param>
    /// <typeparam name="T">The type of the items that will be copied.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyToUnsafe<T>(this ReadOnlySpan<T> source, Span<T> destination)
    {
        ref T sourceReference = ref MemoryMarshal.GetReference(source);
        ref byte sourceReferenceAsByte = ref Unsafe.As<T, byte>(ref sourceReference);

        ref T destinationReference = ref MemoryMarshal.GetReference(destination);
        ref byte destinationReferenceAsByte = ref Unsafe.As<T, byte>(ref destinationReference);

        Unsafe.CopyBlock(ref destinationReferenceAsByte, ref sourceReferenceAsByte, (uint)(sizeof(T) * source.Length));
    }

    /// <summary>
    /// Returns the <see cref="Span{T}"/> that has been passed in. The method can be used to avoid a compiler error,
    /// but comes at the cost that the caller has to guarantee the safety of the code by themself.<br/>
    /// </summary>
    /// <example>
    /// For example, this code produces a compiler error:
    /// <code>
    /// Span&lt;T&gt; span = Span&lt;T&gt;.Empty;
    /// if (span.Length == 0)
    /// {
    ///     span = stackalloc int[50]; // CS8353: A result of a 'stackalloc' expression cannot be used in this context because it may be exposed outside of the containing method
    /// }
    /// </code>
    /// The error can be avoided by using this method:
    /// <code>
    /// Span&lt;T&gt; span = Span&lt;T&gt;.Empty;
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
    public static Span<T> ReturnStackAlloced<T>(scoped Span<T> span)
    {
#pragma warning disable CS9080
        return span;
#pragma warning restore CS9080
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> SliceUnsafe<T>(this Span<T> span, int start, int length)
    {
        ref T reference = ref MemoryMarshal.GetReference(span);
        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref reference, start), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> SliceUnsafe<T>(this Span<T> span, int start)
    {
        return span.SliceUnsafe(start, span.Length - start);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> SliceUnsafe<T>(this Span<T> span, Range range)
    {
        int start = range.Start.GetOffset(span.Length);
        int length = range.End.GetOffset(span.Length) - start;
        return span.SliceUnsafe(start, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> SliceUnsafe<T>(this ReadOnlySpan<T> span, int start, int length)
    {
        ref T reference = ref MemoryMarshal.GetReference(span);
        return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref reference, start), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> SliceUnsafe<T>(this ReadOnlySpan<T> span, int start)
    {
        return span.SliceUnsafe(start, span.Length - start);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> SliceUnsafe<T>(this ReadOnlySpan<T> span, Range range)
    {
        int start = range.Start.GetOffset(span.Length);
        int length = range.End.GetOffset(span.Length) - start;
        return span.SliceUnsafe(start, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpanUnsafe<T>(this T[] array, int start, int length)
    {
        ref T reference = ref MemoryMarshal.GetArrayDataReference(array);
        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref reference, start), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpanUnsafe<T>(this T[] array, int start)
    {
        return array.AsSpanUnsafe(start, array.Length - start);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpanUnsafe<T>(this T[] array, Range range)
    {
        int arrayLength = array.Length;
        int start = range.Start.GetOffset(arrayLength);
        int length = range.End.GetOffset(arrayLength) - start;
        return array.AsSpanUnsafe(start, length);
    }
}
