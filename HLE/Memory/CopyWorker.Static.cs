using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public readonly unsafe ref partial struct CopyWorker<T>
{
    /// <summary>
    /// <c>Memmove(ref T destination, ref T source, nuint elementCount)</c>
    /// </summary>
    [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "exactly what i want")]
    internal static readonly delegate*<ref T, ref T, nuint, void> s_memmove = GetMemmoveFunctionPointer();

    private static delegate*<ref T, ref T, nuint, void> GetMemmoveFunctionPointer() =>
        (delegate*<ref T, ref T, nuint, void>)
        typeof(Buffer).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .FirstOrDefault(static m => m is { Name: "Memmove", IsGenericMethod: true })!
            .MakeGenericMethod(typeof(T)).MethodHandle
            .GetFunctionPointer();

    /// <inheritdoc cref="Copy(ReadOnlySpan{T},Span{T})"/>
    public static void Copy(T[] source, T[] destination)
        => s_memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), (uint)source.Length);

    /// <inheritdoc cref="Copy(ReadOnlySpan{T},Span{T})"/>
    public static void Copy(Span<T> source, Span<T> destination)
        => s_memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    /// <summary>
    /// Copies all elements from the source into the destination without checking if enough space is available.
    /// </summary>
    /// <param name="destination">The destination of the elements.</param>
    /// <param name="source">The source of the elements.</param>
    public static void Copy(ReadOnlySpan<T> source, Span<T> destination)
        => s_memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    public static void Copy(ReadOnlySpan<T> source, ref T destination)
        => s_memmove(ref destination, ref MemoryMarshal.GetReference(source), (uint)source.Length);

    public static void Copy(ReadOnlySpan<T> source, T* destination)
        => s_memmove(ref Unsafe.AsRef<T>(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    /// <inheritdoc cref="Copy(ref T,ref T,nuint)"/>
    public static void Copy(T* source, T* destination, nuint elementCount)
        => s_memmove(ref Unsafe.AsRef<T>(source), ref Unsafe.AsRef<T>(destination), elementCount);

    /// <summary>
    /// Copies the given amount of elements from the source into the destination.
    /// </summary>
    /// <param name="destination">The destination of the elements.</param>
    /// <param name="source">The source of the elements.</param>
    /// <param name="elementCount">The amount of elements that will be copied from source to destination.</param>
    public static void Copy(ref T source, ref T destination, nuint elementCount) => s_memmove(ref destination, ref source, elementCount);
}
