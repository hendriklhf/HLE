using System.Runtime.CompilerServices;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    private readonly ref struct MemcpyAlignmentResult
    {
        public ref byte Source => ref _source;

        public ref byte Destination => ref _destination;

        public nuint ByteCount { get; }

#pragma warning disable IDE0032 // Use auto property (are assigned by ref in ctor, doesn't work currently)
        private readonly ref byte _destination;
        private readonly ref byte _source;
#pragma warning restore IDE0032 // Use auto property

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemcpyAlignmentResult(ref byte source, ref byte destination, nuint byteCount)
        {
            _source = ref source;
            _destination = ref destination;
            ByteCount = byteCount;
        }
    }
}
