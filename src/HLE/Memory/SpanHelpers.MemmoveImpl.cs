using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public static unsafe partial class SpanHelpers
{
    private static class MemmoveImpl<T>
    {
        /// <summary>
        /// Memmove function pointer used for managed types and overlapping source and destination.
        /// </summary>
        /// <remarks>
        /// <c>Memmove(ref T destination, ref T source, nuint elementCount)</c>
        /// </remarks>
        [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "exactly what i want")]
        internal static readonly delegate*<ref T, ref T, nuint, void> s_memmove = GetMemmoveFunctionPointer();

        [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields")]
        private static delegate*<ref T, ref T, nuint, void> GetMemmoveFunctionPointer()
        {
            MethodInfo? memmove = Array.Find(
                typeof(Buffer).GetMethods(BindingFlags.NonPublic | BindingFlags.Static),
                static m => m is { Name: "Memmove", IsGenericMethod: true }
            );

            if (memmove is not null)
            {
#pragma warning disable HAA0101
                return (delegate*<ref T, ref T, nuint, void>)memmove
                    .MakeGenericMethod(typeof(T)).MethodHandle.GetFunctionPointer();
#pragma warning restore HAA0101
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
    }
}
