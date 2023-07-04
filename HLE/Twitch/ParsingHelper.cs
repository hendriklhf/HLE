using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE.Twitch;

internal static class ParsingHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetIndicesOfWhitespaces(ReadOnlySpan<char> ircMessage, ref int indicesBufferReference, int maximumOfIndicesNeeded)
    {
        int indicesLength = 0;
        ReadOnlySpan<ushort> ircMessageAsShort = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(ircMessage)), ircMessage.Length);

        int vector512Count = Vector512<ushort>.Count;
        if (Vector512.IsHardwareAccelerated && ircMessageAsShort.Length >= vector512Count)
        {
            Vector512<ushort> whitespaceVector = Vector512.Create((ushort)' ');
            int startIndex = 0;
            while (ircMessageAsShort.Length - startIndex >= vector512Count)
            {
                Vector512<ushort> charsVector = Unsafe.As<ushort, Vector512<ushort>>(ref Unsafe.Add(ref MemoryMarshal.GetReference(ircMessageAsShort), startIndex));
                uint equals = (uint)Vector512.Equals(charsVector, whitespaceVector).ExtractMostSignificantBits();
                while (equals > 0)
                {
                    int index = BitOperations.TrailingZeroCount(equals);
                    Unsafe.Add(ref indicesBufferReference, indicesLength++) = startIndex + index;
                    if (indicesLength == maximumOfIndicesNeeded)
                    {
                        return maximumOfIndicesNeeded;
                    }

                    equals &= equals - 1;
                }

                startIndex += vector512Count;
            }

            ref char remainingCharsReference = ref Unsafe.Add(ref MemoryMarshal.GetReference(ircMessage), startIndex);
            int remainingLength = ircMessage.Length - startIndex;
            for (int i = 0; i < remainingLength; i++)
            {
                if (Unsafe.Add(ref remainingCharsReference, i) != ' ')
                {
                    continue;
                }

                Unsafe.Add(ref indicesBufferReference, indicesLength++) = startIndex + i;
                if (indicesLength == maximumOfIndicesNeeded)
                {
                    return maximumOfIndicesNeeded;
                }
            }

            return indicesLength;
        }

        int vector256Count = Vector256<ushort>.Count;
        if (Vector256.IsHardwareAccelerated && ircMessageAsShort.Length >= vector256Count)
        {
            Vector256<ushort> whitespaceVector = Vector256.Create((ushort)' ');
            int startIndex = 0;
            while (ircMessageAsShort.Length - startIndex >= vector256Count)
            {
                Vector256<ushort> charsVector = Unsafe.As<ushort, Vector256<ushort>>(ref Unsafe.Add(ref MemoryMarshal.GetReference(ircMessageAsShort), startIndex));
                ushort equals = (ushort)Vector256.Equals(charsVector, whitespaceVector).ExtractMostSignificantBits();
                while (equals > 0)
                {
                    int index = BitOperations.TrailingZeroCount(equals);
                    Unsafe.Add(ref indicesBufferReference, indicesLength++) = startIndex + index;
                    if (indicesLength == maximumOfIndicesNeeded)
                    {
                        return maximumOfIndicesNeeded;
                    }

                    equals &= (ushort)(equals - 1);
                }

                startIndex += vector256Count;
            }

            ref char remainingCharsReference = ref Unsafe.Add(ref MemoryMarshal.GetReference(ircMessage), startIndex);
            int remainingLength = ircMessage.Length - startIndex;
            for (int i = 0; i < remainingLength; i++)
            {
                if (Unsafe.Add(ref remainingCharsReference, i) != ' ')
                {
                    continue;
                }

                Unsafe.Add(ref indicesBufferReference, indicesLength++) = startIndex + i;
                if (indicesLength == maximumOfIndicesNeeded)
                {
                    return maximumOfIndicesNeeded;
                }
            }

            return indicesLength;
        }

        return IndicesOfWhitespacesNonOptimizedFallback(ircMessage, ref indicesBufferReference, maximumOfIndicesNeeded);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int IndicesOfWhitespacesNonOptimizedFallback(ReadOnlySpan<char> ircMessage, ref int indicesBufferReference, int maximumOfIndicesNeeded)
    {
        int indicesLength = 0;
        int indexOfWhitespace = ircMessage.IndexOf(' ');
        int spanStartIndex = indexOfWhitespace;
        while (indexOfWhitespace >= 0)
        {
            Unsafe.Add(ref indicesBufferReference, indicesLength++) = spanStartIndex;
            if (indicesLength == maximumOfIndicesNeeded)
            {
                return maximumOfIndicesNeeded;
            }

            indexOfWhitespace = ircMessage[++spanStartIndex..].IndexOf(' ');
            spanStartIndex += indexOfWhitespace;
        }

        return indicesLength;
    }
}
