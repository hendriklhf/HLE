using HLE.Memory;

namespace HLE.Marshals;

public static class PooledBufferWriterMarshal<T>
{
    public static T[] GetBuffer(PooledBufferWriter<T> writer)
    {
        return writer._buffer._array;
    }
}
