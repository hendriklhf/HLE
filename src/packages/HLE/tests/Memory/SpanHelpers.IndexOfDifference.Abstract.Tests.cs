using System;
using System.Threading.Tasks;
using HLE.Memory;
using HLE.RemoteExecution;
using HLE.TestUtilities;

namespace HLE.UnitTests.Memory;

public abstract class SpanHelpersIndexOfDifferenceAbstractTests<T>(ITestOutputHelper output)
    where T : IEquatable<T>
{
    private readonly ITestOutputHelper _output = output;

    private protected abstract T GetNonDefaultValue();

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_OneDifference_AtIndex0_SameLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_OneDifference_AtFirstIndex_SameLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_OneDifference_AtFirstIndex_SameLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[1000];

        a[0] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(0, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_OneDifference_AtSomeIndex_SameLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_OneDifference_AtSomeIndex_SameLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_OneDifference_AtSomeIndex_SameLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[1000];

        a[518] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(518, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_OneDifference_AtLastIndex_SameLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_OneDifference_AtLastIndex_SameLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_OneDifference_AtLastIndex_SameLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[1000];

        a[999] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(999, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_TwoDifferences_AtFirstIndex_SameLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_TwoDifferences_AtFirstIndex_SameLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_TwoDifferences_AtFirstIndex_SameLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[1000];

        a[518] = GetNonDefaultValue();
        b[0] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(0, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_TwoDifferences_AtSomeIndex_SameLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_TwoDifferences_AtSomeIndex_SameLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_TwoDifferences_AtSomeIndex_SameLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[1000];

        a[518] = GetNonDefaultValue();
        b[517] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(517, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_TwoDifferences_AtLastIndex_SameLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_TwoDifferences_AtLastIndex_SameLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_TwoDifferences_AtLastIndex_SameLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[1000];

        a[999] = GetNonDefaultValue();
        b[998] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(998, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_NoDifference_SameLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_NoDifference_SameLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private static void Remote_IndexOfDifference_NoDifference_SameLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[1000];

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(-1, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_OneDifference_AtIndex0_DifferentLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_OneDifference_AtFirstIndex_DifferentLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_OneDifference_AtFirstIndex_DifferentLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[600];

        a[0] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(0, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_OneDifference_AtSomeIndex_DifferentLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_OneDifference_AtSomeIndex_DifferentLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_OneDifference_AtSomeIndex_DifferentLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[600];

        a[599] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(599, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_OneDifference_AtLastIndex_DifferentLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_OneDifference_AtLastIndex_DifferentLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_OneDifference_AtLastIndex_DifferentLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[600];

        a[599] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(599, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_TwoDifferences_AtFirstIndex_DifferentLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_TwoDifferences_AtFirstIndex_DifferentLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_TwoDifferences_AtFirstIndex_DifferentLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[600];

        a[518] = GetNonDefaultValue();
        b[0] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(0, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_TwoDifferences_AtSomeIndex_DifferentLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_TwoDifferences_AtSomeIndex_DifferentLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_TwoDifferences_AtSomeIndex_DifferentLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[600];

        a[518] = GetNonDefaultValue();
        b[517] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(517, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_TwoDifferences_AtLastIndex_DifferentLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_TwoDifferences_AtLastIndex_DifferentLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_TwoDifferences_AtLastIndex_DifferentLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[600];

        a[999] = GetNonDefaultValue();
        b[599] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(599, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_NoDifference_DifferentLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_NoDifference_DifferentLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private static void Remote_IndexOfDifference_NoDifference_DifferentLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[600];

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(-1, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_OneDifference_BeyondB_DifferentLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_OneDifference_BeyondB_DifferentLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_OneDifference_BeyondB_DifferentLength()
    {
        Span<T> a = new T[1000];
        Span<T> b = new T[600];

        a[600] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(-1, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_NoDifference_BothZeroLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_NoDifference_BothZeroLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private static void Remote_IndexOfDifference_NoDifference_BothZeroLength()
    {
        Span<T> a = [];
        Span<T> b = [];

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(-1, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_NoDifference_FirstZeroLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_NoDifference_FirstZeroLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_NoDifference_FirstZeroLength()
    {
        Span<T> a = [];
        Span<T> b = new T[10];

        b[0] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(-1, index);
    }

    [Theory]
    [MemberData(nameof(TheoryDataHelpers.VectorExecutionOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IndexOfDifference_NoDifference_SecondZeroLength(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IndexOfDifference_NoDifference_SecondZeroLength, options);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private void Remote_IndexOfDifference_NoDifference_SecondZeroLength()
    {
        Span<T> a = new T[10];
        Span<T> b = [];

        a[0] = GetNonDefaultValue();

        int index = SpanHelpers.IndexOfDifference(a, b);
        Assert.Equal(-1, index);
    }
}
