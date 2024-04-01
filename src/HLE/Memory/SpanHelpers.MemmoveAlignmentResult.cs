using System.Runtime.CompilerServices;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    private readonly ref struct MemmoveAlignmentResult
    {
        public ref byte Source => ref _source;

        public ref byte Destination => ref _destination;

        public nuint ByteCount { get; }

        private readonly ref byte _destination;
        private readonly ref byte _source;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemmoveAlignmentResult(ref byte source, ref byte destination, nuint byteCount)
        {
            _source = ref source;
            _destination = ref destination;
            ByteCount = byteCount;
        }
    }
}
