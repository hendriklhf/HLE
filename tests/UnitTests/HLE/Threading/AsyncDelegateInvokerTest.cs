using System;
using System.Threading.Tasks;
using HLE.Threading;
using Xunit;

namespace HLE.UnitTests.Threading;

public sealed class AsyncDelegateInvokerTest
{
    private uint _counter;

    [Fact]
    public async Task InvokeAsync_Action()
    {
        uint counter = _counter;
        await AsyncDelegateInvoker.InvokeAsync(void () => _counter++);
        Assert.Equal(counter + 1, _counter);
    }

    [Fact]
    public async Task InvokeAsync_Action_Exception()
    {
        const string Message = "hello";

        Task t = AsyncDelegateInvoker.InvokeAsync(static void () => ThrowHelper.ThrowInvalidOperationException(Message));
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => t);
        Assert.Same(Message, exception.Message);
    }

    [Fact]
    public async Task InvokeAsync_Action_State()
    {
        uint counter = _counter;
        await AsyncDelegateInvoker.InvokeAsync(void (i) => _counter += i, 5U);
        Assert.Equal(counter + 5, _counter);
    }

    [Fact]
    public async Task InvokeAsync_Action_State_Exception()
    {
        const string Message = "hello";

        Task t = AsyncDelegateInvoker.InvokeAsync(static void (message) => ThrowHelper.ThrowInvalidOperationException(message), Message);
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => t);
        Assert.Same(Message, exception.Message);
    }

    [Fact]
    public async Task InvokeAsync_Func()
    {
        uint counter = _counter;
        uint c = await AsyncDelegateInvoker.InvokeAsync(() => ++_counter);
        Assert.Equal(counter + 1, _counter);
        Assert.Equal(c, _counter);
    }

    [Fact]
    public async Task InvokeAsync_Func_Exception()
    {
        const string Message = "hello";

        Task<uint> t = AsyncDelegateInvoker.InvokeAsync(static () =>
        {
            ThrowHelper.ThrowInvalidOperationException(Message);
            return 5U;
        });

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => t);
        Assert.Same(Message, exception.Message);
    }

    [Fact]
    public async Task InvokeAsync_Func_State()
    {
        uint counter = _counter;
        uint c = await AsyncDelegateInvoker.InvokeAsync(i => _counter += i, 5U);
        Assert.Equal(counter + 5, _counter);
        Assert.Equal(c, _counter);
    }

    [Fact]
    public async Task InvokeAsync_Func_State_Exception()
    {
        const string Message = "hello";

        Task<uint> t = AsyncDelegateInvoker.InvokeAsync(static message =>
        {
            ThrowHelper.ThrowInvalidOperationException(message);
            return 5U;
        }, Message);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => t);
        Assert.Same(Message, exception.Message);
    }
}
