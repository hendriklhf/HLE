using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using HLE.Collections;
using Xunit;

namespace HLE.UnitTests;

public sealed class ThrowHelperTest
{
    [Fact]
    public void ThrowObjectDisposedException_Generic_Test()
        => Assert.Throws<ObjectDisposedException>(ThrowHelper.ThrowObjectDisposedException<Stream>);

    [Fact]
    [SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known")]
    public void ThrowObjectDisposedException_NonGeneric_Test()
        => Assert.Throws<ObjectDisposedException>(static () => ThrowHelper.ThrowObjectDisposedException(typeof(Stream)));

    [Fact]
    public void ThrowUnreachableExceptionTest()
        => Assert.Throws<UnreachableException>(ThrowHelper.ThrowUnreachableException);

    [Fact]
    public void ThrowInvalidEnumValueTest()
    {
        const StringComparison InvalidEnumValue = (StringComparison)(-1);
        Assert.Throws<InvalidEnumArgumentException>(static () => ThrowHelper.ThrowInvalidEnumValue(InvalidEnumValue));
    }

    [Fact]
    public void ThrowCalledCollectionBuilderConstructorTest()
        => Assert.Throws<NotSupportedException>(ThrowHelper.ThrowCalledCollectionBuilderConstructor<PooledListBuilder>);

    [Fact]
    public void ThrowOperationCanceledExceptionTest()
        => Assert.Throws<OperationCanceledException>(static () => ThrowHelper.ThrowOperationCanceledException(CancellationToken.None));

    [Fact]
    public void ThrowOperatingSystemNotSupportedTest()
        => Assert.Throws<NotSupportedException>(ThrowHelper.ThrowOperatingSystemNotSupported);

    [Fact]
    public void ThrowNotSupportedExceptionTest()
    {
        string message = Random.Shared.NextString(16);
        NotSupportedException exception = Assert.Throws<NotSupportedException>(() => ThrowHelper.ThrowNotSupportedException(message));
        Assert.Same(message, exception.Message);
    }

    [Fact]
    public void ThrowInvalidOperationExceptionTest()
    {
        string message = Random.Shared.NextString(16);
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => ThrowHelper.ThrowInvalidOperationException(message));
        Assert.Same(message, exception.Message);
    }
}
