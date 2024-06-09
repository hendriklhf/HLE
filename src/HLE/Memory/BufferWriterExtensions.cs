using System;
using System.Buffers;
using System.Text;
using HLE.Text;

namespace HLE.Memory;

public static class BufferWriterExtensions
{
    public static void WriteUtf8<TBufferWriter>(this TBufferWriter writer, ref PooledInterpolatedStringHandler chars) where TBufferWriter : IBufferWriter<byte>
    {
        try
        {
            writer.WriteUtf8(chars.Text);
        }
        finally
        {
            chars.Dispose();
        }
    }

    public static void WriteUtf8<TBufferWriter>(this TBufferWriter writer, ReadOnlySpan<char> chars) where TBufferWriter : IBufferWriter<byte>
    {
        Encoding utf8 = Encoding.UTF8;
        int maxByteCount = utf8.GetMaxByteCount(chars.Length);
        Span<byte> destination = writer.GetSpan(maxByteCount);
        int bytesWritten = utf8.GetBytes(chars, destination);
        writer.Advance(bytesWritten);
    }
}
