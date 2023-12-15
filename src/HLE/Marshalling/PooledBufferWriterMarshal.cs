using HLE.Memory;

namespace HLE.Marshalling;

public static class PooledBufferWriterMarshal<T>
{
    public static T[] GetBuffer(PooledBufferWriter<T> writer) => writer._buffer.Array;
}
