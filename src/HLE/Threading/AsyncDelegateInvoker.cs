using System;
using System.Threading;
using System.Threading.Tasks;

namespace HLE.Threading;

public static partial class AsyncDelegateInvoker
{
    private static readonly Action<ActionState> s_invokeAsyncAction = static state =>
    {
        try
        {
            state.Action();
        }
        catch (Exception ex)
        {
            state.TaskCompletionSource.SetException(ex);
            return;
        }

        state.TaskCompletionSource.SetResult();
    };

    public static Task InvokeAsync(Action action)
    {
        TaskCompletionSource taskCompletionSource = new();
        ActionState actionState = new(taskCompletionSource, action);
        ThreadPool.QueueUserWorkItem(s_invokeAsyncAction, actionState, true);
        return taskCompletionSource.Task;
    }

    public static Task InvokeAsync<TState>(Action<TState> action, TState state)
    {
        TaskCompletionSource taskCompletionSource = new();
        ActionState<TState> actionState = new(taskCompletionSource, action, state);
        ThreadPool.QueueUserWorkItem(InvokeAsyncCallbacks<TState>.InvokeAsyncAction, actionState, true);
        return taskCompletionSource.Task;
    }

    public static Task<TResult> InvokeAsync<TResult>(Func<TResult> func)
    {
        TaskCompletionSource<TResult> taskCompletionSource = new();
        FuncState<TResult> delegateState = new(taskCompletionSource, func);
        ThreadPool.QueueUserWorkItem(InvokeAsyncCallbacks<TResult>.InvokeAsyncFunc, delegateState, true);
        return taskCompletionSource.Task;
    }

    public static Task<TResult> InvokeAsync<TState, TResult>(Func<TState, TResult> func, TState state)
    {
        TaskCompletionSource<TResult> taskCompletionSource = new();
        FuncState<TState, TResult> delegateState = new(taskCompletionSource, func, state);
        ThreadPool.QueueUserWorkItem(InvokeAsyncCallbacks<TState, TResult>.InvokeAsyncFunc, delegateState, true);
        return taskCompletionSource.Task;
    }
}
