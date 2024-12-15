using System;
using System.Diagnostics;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Memmove<T>(ref T destination, ref T source, nuint elementCount)
    {
        if (elementCount <= int.MaxValue)
        {
            MemoryMarshal.CreateReadOnlySpan(ref source, (int)elementCount)
                .CopyTo(MemoryMarshal.CreateSpan(ref destination, int.MaxValue));
            return;
        }

        MemmoveCore(ref destination, ref source, elementCount);

        return;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void MemmoveCore(ref T destination, ref T source, nuint elementCount)
        {
            Debug.Assert(elementCount > int.MaxValue);

            do
            {
                MemoryMarshal.CreateReadOnlySpan(ref source, int.MaxValue)
                    .CopyTo(MemoryMarshal.CreateSpan(ref destination, int.MaxValue));

                source = ref Unsafe.Add(ref source, int.MaxValue);
                destination = ref Unsafe.Add(ref destination, int.MaxValue);
                elementCount -= int.MaxValue;
            }
            while (elementCount >= int.MaxValue);

            if (elementCount != 0)
            {
                MemoryMarshal.CreateReadOnlySpan(ref source, (int)elementCount)
                    .CopyTo(MemoryMarshal.CreateSpan(ref destination, int.MaxValue));
            }
        }
    }
}
