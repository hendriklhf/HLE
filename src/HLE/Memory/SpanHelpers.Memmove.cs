using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public static unsafe class SpanHelpers<T>
{
    /// <summary>
    /// <c>Memmove(ref T destination, ref T source, nuint elementCount)</c>
    /// </summary>
    [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "exactly what i want")]
    private static readonly delegate*<ref T, ref T, nuint, void> s_memmove = GetMemmoveFunctionPointer();

    private static delegate*<ref T, ref T, nuint, void> GetMemmoveFunctionPointer()
    {
        MethodInfo? memmove = Array.Find(
            typeof(Buffer).GetMethods(BindingFlags.NonPublic | BindingFlags.Static),
            static m => m is { Name: "Memmove", IsGenericMethod: true }
        );

        if (memmove is not null)
        {
            return (delegate*<ref T, ref T, nuint, void>)memmove
                .MakeGenericMethod(typeof(T)).MethodHandle.GetFunctionPointer();
        }

        Debug.Fail($"Using {nameof(MemmoveFallback)} method.");
        return &MemmoveFallback;
    }

    private static void MemmoveFallback(ref T destination, ref T source, nuint elementCount)
    {
        if (elementCount == 0)
        {
            return;
        }

        ReadOnlySpan<T> sourceSpan;
        Span<T> destinationSpan;
        while (elementCount >= int.MaxValue)
        {
            sourceSpan = MemoryMarshal.CreateReadOnlySpan(ref source, int.MaxValue);
            destinationSpan = MemoryMarshal.CreateSpan(ref destination, int.MaxValue);
            sourceSpan.CopyTo(destinationSpan);

            elementCount -= int.MaxValue;
            source = ref Unsafe.Add(ref source, int.MaxValue);
            destination = ref Unsafe.Add(ref destination, int.MaxValue);
        }

        sourceSpan = MemoryMarshal.CreateReadOnlySpan(ref source, (int)elementCount);
        destinationSpan = MemoryMarshal.CreateSpan(ref destination, (int)elementCount);
        sourceSpan.CopyTo(destinationSpan);
    }

    public static void Copy(T[] source, T* destination)
        => Memmove(ref Unsafe.AsRef<T>(destination), ref MemoryMarshal.GetArrayDataReference(source), (uint)source.Length);

    public static void Copy(T[] source, ref T destination)
        => Memmove(ref destination, ref MemoryMarshal.GetArrayDataReference(source), (uint)source.Length);

    /// <inheritdoc cref="Copy(ReadOnlySpan{T},Span{T})"/>
    public static void Copy(T[] source, T[] destination)
        => Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), (uint)source.Length);

    /// <inheritdoc cref="Copy(ReadOnlySpan{T},Span{T})"/>
    public static void Copy(Span<T> source, T[] destination)
        => Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    /// <inheritdoc cref="Copy(ReadOnlySpan{T},Span{T})"/>
    public static void Copy(Span<T> source, Span<T> destination)
        => Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    /// <inheritdoc cref="Copy(ReadOnlySpan{T},Span{T})"/>
    public static void Copy(ReadOnlySpan<T> source, T[] destination)
        => Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    /// <summary>
    /// Copies all elements from the source into the destination without checking if enough space is available.
    /// </summary>
    /// <param name="source">The source of the elements.</param>
    /// <param name="destination">The destination of the elements.</param>
    public static void Copy(ReadOnlySpan<T> source, Span<T> destination)
        => Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    public static void Copy(ReadOnlySpan<T> source, ref T destination)
        => Memmove(ref destination, ref MemoryMarshal.GetReference(source), (uint)source.Length);

    public static void Copy(ReadOnlySpan<T> source, T* destination)
        => Memmove(ref Unsafe.AsRef<T>(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    /// <inheritdoc cref="Memmove(ref T,ref T,nuint)"/>
    public static void Memmove(T* destination, T* source, nuint elementCount)
        => Memmove(ref Unsafe.AsRef<T>(destination), ref Unsafe.AsRef<T>(source), elementCount);

    /// <summary>
    /// Copies the given amount of elements from the source into the destination.
    /// </summary>
    /// <param name="destination">The destination of the elements.</param>
    /// <param name="source">The source of the elements.</param>
    /// <param name="elementCount">The amount of elements that will be copied from source to destination.</param>
    public static void Memmove(ref T destination, ref T source, nuint elementCount)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>() || Overlaps(ref source, ref destination, elementCount))
        {
            s_memmove(ref destination, ref source, elementCount);
            return;
        }

        SpanHelpers.Memmove(ref Unsafe.As<T, byte>(ref destination), ref Unsafe.As<T, byte>(ref source), checked(elementCount * (uint)sizeof(T)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Overlaps(ref T source, ref T destination, nuint elementCount) =>
        (nuint)Unsafe.ByteOffset(ref source, ref destination) < elementCount ||
        (nuint)Unsafe.ByteOffset(ref destination, ref source) < elementCount;
}
