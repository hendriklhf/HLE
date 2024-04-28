using System;
using System.Buffers;
using System.Text;

namespace HLE.Memory;

public static class BufferWriterExtensions
{
    public static void WriteUtf8<TBufferWriter>(this TBufferWriter writer, ReadOnlySpan<char> chars) where TBufferWriter : IBufferWriter<byte>
    {
        Encoding utf8 = Encoding.UTF8;
        int maxByteCount = utf8.GetMaxByteCount(chars.Length);
        Span<byte> destination = writer.GetSpan(maxByteCount);
        int bytesWritten = utf8.GetBytes(chars, destination);
        writer.Advance(bytesWritten);
    }
}
