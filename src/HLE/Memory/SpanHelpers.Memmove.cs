using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public static unsafe partial class SpanHelpers
{
    public static void Copy<T>(T[] source, T* destination)
        => Memmove(ref Unsafe.AsRef<T>(destination), ref MemoryMarshal.GetArrayDataReference(source), (uint)source.Length);

    public static void Copy<T>(T[] source, ref T destination)
        => Memmove(ref destination, ref MemoryMarshal.GetArrayDataReference(source), (uint)source.Length);

    public static void Copy<T>(T[] source, T[] destination)
        => Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), (uint)source.Length);

    public static void Copy<T>(Span<T> source, T[] destination)
        => Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    public static void Copy<T>(Span<T> source, Span<T> destination)
        => Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    public static void Copy<T>(Span<T> source, ref T destination)
        => Memmove(ref destination, ref MemoryMarshal.GetReference(source), (uint)source.Length);

    public static void Copy<T>(Span<T> source, T* destination)
        => Memmove(ref Unsafe.AsRef<T>(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    public static void Copy<T>(ReadOnlySpan<T> source, T[] destination)
        => Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    public static void Copy<T>(ReadOnlySpan<T> source, Span<T> destination)
        => Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    public static void Copy<T>(ReadOnlySpan<T> source, ref T destination)
        => Memmove(ref destination, ref MemoryMarshal.GetReference(source), (uint)source.Length);

    public static void Copy<T>(ReadOnlySpan<T> source, T* destination)
        => Memmove(ref Unsafe.AsRef<T>(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    /// <inheritdoc cref="Memmove{T}(ref T,ref T,nuint)"/>
    public static void Memmove<T>(T* destination, T* source, nuint elementCount)
        => Memmove(ref Unsafe.AsRef<T>(destination), ref Unsafe.AsRef<T>(source), elementCount);

    /// <summary>
    /// Copies the given amount of elements from the source into the destination.
    /// </summary>
    /// <typeparam name="T">The element type that will be copied.</typeparam>
    /// <param name="destination">The destination of the elements.</param>
    /// <param name="source">The source of the elements.</param>
    /// <param name="elementCount">The amount of elements that will be copied from source to destination.</param>
    public static void Memmove<T>(ref T destination, ref T source, nuint elementCount)
    {
        if (elementCount == 0 || Unsafe.AreSame(ref destination, ref source))
        {
            return;
        }

        nuint byteCount = checked(elementCount * (uint)sizeof(T));
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>() || Overlaps(ref source, ref destination, byteCount))
        {
            MemmoveImpl<T>.s_memmove(ref destination, ref source, elementCount);
            return;
        }

        Memcpy(ref Unsafe.As<T, byte>(ref destination), ref Unsafe.As<T, byte>(ref source), byteCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Overlaps<T>(ref T source, ref T destination, nuint byteCount) =>
        (nuint)Unsafe.ByteOffset(ref source, ref destination) < byteCount ||
        (nuint)Unsafe.ByteOffset(ref destination, ref source) < byteCount;
}
