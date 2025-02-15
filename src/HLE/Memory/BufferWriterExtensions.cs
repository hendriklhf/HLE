using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using HLE.Marshalling;
using HLE.Text;

namespace HLE.Memory;

public static class BufferWriterExtensions
{
    public static void WriteUtf8<TBufferWriter>(this TBufferWriter writer, ref PooledInterpolatedStringHandler chars)
        where TBufferWriter : IBufferWriter<byte>, allows ref struct
    {
        writer.WriteUtf8(chars.Text);
        chars.Dispose();
    }

    public static void WriteUtf8<TBufferWriter>(this TBufferWriter writer, ReadOnlySpan<char> chars)
        where TBufferWriter : IBufferWriter<byte>, allows ref struct
    {
        Encoding utf8 = Encoding.UTF8;
        int maxByteCount = utf8.GetMaxByteCount(chars.Length);
        Span<byte> destination = writer.GetSpan(maxByteCount);
        int bytesWritten = utf8.GetBytes(chars, destination);
        writer.Advance(bytesWritten);
    }

    public static void Write<TBufferWriter>(this TBufferWriter writer, string str)
        where TBufferWriter : IBufferWriter<char>, allows ref struct
    {
        Span<char> destination = writer.GetSpan(str.Length);
        SpanHelpers.Memmove(ref MemoryMarshal.GetReference(destination), ref StringMarshal.GetReference(str), str.Length);
        writer.Advance(str.Length);
    }

    public static void Write(this PooledBufferWriter<char> writer, string str)
    {
        SpanHelpers.Memmove(ref writer.GetReference(str.Length), ref StringMarshal.GetReference(str), str.Length);
        writer.Advance(str.Length);
    }

    public static void Write(this ValueBufferWriter<char> writer, string str)
    {
        SpanHelpers.Memmove(ref writer.GetReference(str.Length), ref StringMarshal.GetReference(str), str.Length);
        writer.Advance(str.Length);
    }

    public static void Write(this UnsafeBufferWriter<char> writer, string str)
    {
        SpanHelpers.Memmove(ref writer.GetReference(), ref StringMarshal.GetReference(str), str.Length);
        writer.Advance(str.Length);
    }
}
