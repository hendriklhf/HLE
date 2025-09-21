using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public static unsafe partial class SpanHelpers
{
    public static void Copy<T>(T[] source, T[] destination)
        => Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), source.Length);

    public static void Copy<T>(T[] source, Span<T> destination)
        => Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetArrayDataReference(source), source.Length);

    public static void Copy<T>(T[] source, T* destination)
        => Memmove(ref Unsafe.AsRef<T>(destination), ref MemoryMarshal.GetArrayDataReference(source), source.Length);

    public static void Copy<T>(T[] source, ref T destination)
        => Memmove(ref destination, ref MemoryMarshal.GetArrayDataReference(source), source.Length);

    public static void Copy<T>(Span<T> source, T[] destination)
        => Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetReference(source), source.Length);

    public static void Copy<T>(Span<T> source, Span<T> destination)
        => Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), source.Length);

    public static void Copy<T>(Span<T> source, ref T destination)
        => Memmove(ref destination, ref MemoryMarshal.GetReference(source), source.Length);

    public static void Copy<T>(Span<T> source, T* destination)
        => Memmove(ref Unsafe.AsRef<T>(destination), ref MemoryMarshal.GetReference(source), source.Length);

    public static void Copy<T>(ReadOnlySpan<T> source, T[] destination)
        => Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetReference(source), source.Length);

    public static void Copy<T>(ReadOnlySpan<T> source, Span<T> destination)
        => Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), source.Length);

    public static void Copy<T>(ReadOnlySpan<T> source, ref T destination)
        => Memmove(ref destination, ref MemoryMarshal.GetReference(source), source.Length);

    public static void Copy<T>(ReadOnlySpan<T> source, T* destination)
        => Memmove(ref Unsafe.AsRef<T>(destination), ref MemoryMarshal.GetReference(source), source.Length);
}
