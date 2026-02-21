using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using HLE.Marshalling;
using HLE.Text;

namespace HLE.Memory;

#pragma warning disable CA1708, S2325
[SuppressMessage("Performance", "CA1822:Mark members as static")] // TODO: remove soon?
public static class BufferWriterExtensions
{
    extension<TBufferWriter>(TBufferWriter writer)
#if NET9_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>, allows ref struct
#else
        where TBufferWriter : IBufferWriter<byte>
#endif
    {
        public void WriteUtf8(ref DefaultInterpolatedStringHandler chars)
        {
            writer.WriteUtf8(chars.Text);
            chars.Clear();
        }

        public void WriteUtf8(ReadOnlySpan<char> chars)
        {
            Encoding utf8 = Encoding.UTF8;
            int maxByteCount = utf8.GetMaxByteCount(chars.Length);
            Span<byte> destination = writer.GetSpan(maxByteCount);
            int bytesWritten = utf8.GetBytes(chars, destination);
            writer.Advance(bytesWritten);
        }
    }

    extension<TBufferWriter>(TBufferWriter writer)
#if NET9_0_OR_GREATER
        where TBufferWriter : IBufferWriter<char>, allows ref struct
#else
        where TBufferWriter : IBufferWriter<char>
#endif
    {
        public void Write(string str)
        {
            Span<char> destination = writer.GetSpan(str.Length);
            SpanHelpers.Memmove(ref MemoryMarshal.GetReference(destination), ref StringMarshal.GetReference(str), str.Length);
            writer.Advance(str.Length);
        }

        public void Write(ref DefaultInterpolatedStringHandler chars)
        {
            int length = chars.Text.Length;
            Span<char> destination = writer.GetSpan(length);
            SpanHelpers.Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(chars.Text), length);
            writer.Advance(length);
            chars.Clear();
        }
    }

    extension(PooledBufferWriter<char> writer)
    {
        public void Write(string str)
        {
            SpanHelpers.Memmove(ref writer.GetReference(str.Length), ref StringMarshal.GetReference(str), str.Length);
            writer.Advance(str.Length);
        }

        public void Write(ref DefaultInterpolatedStringHandler chars)
        {
            writer.Write(chars.Text);
            chars.Clear();
        }
    }

    extension(ValueBufferWriter<char> writer)
    {
        public void Write(string str)
        {
            SpanHelpers.Memmove(ref writer.GetReference(str.Length), ref StringMarshal.GetReference(str), str.Length);
            writer.Advance(str.Length);
        }

        public void Write(ref DefaultInterpolatedStringHandler chars)
        {
            writer.Write(chars.Text);
            chars.Clear();
        }
    }

    extension(UnsafeBufferWriter<char> writer)
    {
        public void Write(string str)
        {
            SpanHelpers.Memmove(ref writer.GetReference(), ref StringMarshal.GetReference(str), str.Length);
            writer.Advance(str.Length);
        }

        public void Write(ref DefaultInterpolatedStringHandler chars)
        {
            writer.Write(chars.Text);
            chars.Clear();
        }
    }
}
