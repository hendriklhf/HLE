using System;

namespace HLE.UnitTests.Memory;

public sealed class SpanHelpersIndicesOfUInt8Tests : SpanHelpersIndicesOfTests<byte>;

public sealed class SpanHelpersIndicesOfUInt16Tests : SpanHelpersIndicesOfTests<ushort>;

public sealed class SpanHelpersIndicesOfCharTests : SpanHelpersIndicesOfTests<char>;

public sealed class SpanHelpersIndicesOfUInt32Tests : SpanHelpersIndicesOfTests<uint>;

public sealed class SpanHelpersIndicesOfUInt64Tests : SpanHelpersIndicesOfTests<ulong>;

public sealed class SpanHelpersIndicesOfGuidTests : SpanHelpersIndicesOfTests<Guid>;

public sealed class SpanHelpersIndicesOfStringTests : SpanHelpersIndicesOfTests<string>;
