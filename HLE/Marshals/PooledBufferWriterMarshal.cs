using HLE.Memory;

namespace HLE.Marshals;

public static class PooledBufferWriterMarshal<T>
{
    public static T[] GetBuffer(PooledBufferWriter<T> writer)
    {
        return writer._buffer._array;
    }

    public static void SetCount(PooledBufferWriter<T> writer, int count)
    {
        writer.EnsureCapacity(count);
        writer.Count = count;
    }
}
