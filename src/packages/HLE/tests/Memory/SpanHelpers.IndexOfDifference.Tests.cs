using System;

namespace HLE.UnitTests.Memory;

public sealed class SpanHelpersIndexOfDifferenceUInt8Tests(ITestOutputHelper output) : SpanHelpersIndexOfDifferenceAbstractTests<byte>(output)
{
    private protected override byte GetNonDefaultValue() => 1;
}

public sealed class SpanHelpersIndexOfDifferenceUInt16Tests(ITestOutputHelper output) : SpanHelpersIndexOfDifferenceAbstractTests<ushort>(output)
{
    private protected override ushort GetNonDefaultValue() => 1;
}

public sealed class SpanHelpersIndexOfDifferenceCharTests(ITestOutputHelper output) : SpanHelpersIndexOfDifferenceAbstractTests<char>(output)
{
    private protected override char GetNonDefaultValue() => 'a';
}

public sealed class SpanHelpersIndexOfDifferenceUInt32Tests(ITestOutputHelper output) : SpanHelpersIndexOfDifferenceAbstractTests<uint>(output)
{
    private protected override uint GetNonDefaultValue() => 1;
}

public sealed class SpanHelpersIndexOfDifferenceUInt64Tests(ITestOutputHelper output) : SpanHelpersIndexOfDifferenceAbstractTests<ulong>(output)
{
    private protected override ulong GetNonDefaultValue() => 1;
}

public sealed class SpanHelpersIndexOfDifferenceGuidTests(ITestOutputHelper output) : SpanHelpersIndexOfDifferenceAbstractTests<Guid>(output)
{
    private protected override Guid GetNonDefaultValue() => Guid.NewGuid();
}

public sealed class SpanHelpersIndexOfDifferenceStringTests(ITestOutputHelper output) : SpanHelpersIndexOfDifferenceAbstractTests<string>(output)
{
    private protected override string GetNonDefaultValue() => "Hello, World!";
}
