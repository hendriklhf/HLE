using System;
using System.Diagnostics.CodeAnalysis;
using HLE.TestUtilities;

namespace HLE.UnitTests.HLE;

public sealed class DelegateExtensionsTest
{
    private int _counter;

    public static TheoryData<int> MultiDelegateCounts { get; } = TheoryDataHelpers.CreateRange(1, 16);

    [Fact]
    public void EnumerateInvocationList_NullDelegate_ReturnsEmptyEnumerator()
    {
        Action? action = null;
        foreach (Action? _ in Delegate.EnumerateInvocationList(action))
        {
            Assert.Fail("Expected no delegates, but found one.");
        }
    }

    [Fact]
    [SuppressMessage("Style", "IDE0039:Use local function")]
    public void EnumerateInvocationList_SingleDelegate_Enumerates()
    {
        bool executed = false;
        Action action = () => executed = true;
        foreach (Action a in Delegate.EnumerateInvocationList(action))
        {
            a();
        }

        Assert.True(executed);
    }

    [Theory]
    [MemberData(nameof(MultiDelegateCounts))]
    public void EnumerateInvocationList_MultiDelegate_Enumerates(int count)
    {
        Action? action = null;
        for (int i = 0; i < count; i++)
        {
            action += Increment;
        }

        foreach (Action? a in Delegate.EnumerateInvocationList(action))
        {
            a!();
        }

        Assert.Equal(count, _counter);

        return;

        void Increment() => _counter++;
    }
}
