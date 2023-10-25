﻿using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Strings;

internal readonly unsafe ref struct StringRawDataWriter
{
    private readonly ref byte _buffer;

    public StringRawDataWriter(Span<byte> buffer) => _buffer = ref MemoryMarshal.GetReference(buffer);

    public StringRawDataWriter(byte* buffer) => _buffer = ref Unsafe.AsRef<byte>(buffer);

    public StringRawDataWriter(ref byte buffer) => _buffer = ref buffer;

    public void Write(int length) => WriteMethodTableAndLength(length);

    public void Write(ReadOnlySpan<char> chars)
    {
        WriteMethodTableAndLength(chars.Length);
        WriteChars(chars);
    }

    private void WriteMethodTableAndLength(int length)
    {
        ref nuint typeHandleReference = ref Unsafe.As<byte, nuint>(ref Unsafe.Add(ref _buffer, sizeof(nuint)));
        typeHandleReference = (nuint)typeof(string).TypeHandle.Value;
        ref int lengthReference = ref Unsafe.As<nuint, int>(ref Unsafe.Add(ref typeHandleReference, 1));
        lengthReference = length;
        ref char charsReference = ref Unsafe.As<int, char>(ref Unsafe.Add(ref lengthReference, 1));
        Unsafe.Add(ref charsReference, length) = '\0';
    }

    private void WriteChars(ReadOnlySpan<char> chars)
    {
        ref char charsReference = ref Unsafe.As<byte, char>(ref Unsafe.Add(ref _buffer, sizeof(nuint) * 2 + sizeof(int)));
        CopyWorker<char> copyWorker = new(chars);
        copyWorker.CopyTo(ref charsReference);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNeededBufferSize(int stringLength) =>
        sizeof(nuint) /* object header */ +
        sizeof(nuint) /* method table pointer */ +
        sizeof(int) /* string length */ +
        stringLength * sizeof(char) /* chars */ +
        sizeof(char) /* zero-char */;
}
