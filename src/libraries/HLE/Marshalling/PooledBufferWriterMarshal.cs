using HLE.Memory;

namespace HLE.Marshalling;

public static class PooledBufferWriterMarshal
{
    public static T[] GetBuffer<T>(PooledBufferWriter<T> writer) => writer.GetBuffer();
}
