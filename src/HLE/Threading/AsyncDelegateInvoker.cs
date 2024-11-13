using System;
using System.Threading;
using System.Threading.Tasks;

namespace HLE.Threading;

public static partial class AsyncDelegateInvoker
{
    public static Task InvokeAsync(Action action)
    {
        TaskCompletionSource taskCompletionSource = new();

        ActionState actionState = new(taskCompletionSource, action);
        ThreadPool.QueueUserWorkItem(static state =>
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
        }, actionState, true);

        return taskCompletionSource.Task;
    }

    public static Task InvokeAsync<TState>(Action<TState> action, TState state)
    {
        TaskCompletionSource taskCompletionSource = new();

        ActionState<TState> actionState = new(taskCompletionSource, action, state);
        ThreadPool.QueueUserWorkItem(static state =>
        {
            try
            {
                state.Action(state.State);
            }
            catch (Exception ex)
            {
                state.TaskCompletionSource.SetException(ex);
                return;
            }

            state.TaskCompletionSource.SetResult();
        }, actionState, true);

        return taskCompletionSource.Task;
    }

    public static Task<TResult> InvokeAsync<TResult>(Func<TResult> func)
    {
        TaskCompletionSource<TResult> taskCompletionSource = new();

        FuncState<TResult> delegateState = new(taskCompletionSource, func);
        ThreadPool.QueueUserWorkItem(static state =>
        {
            TResult result;
            try
            {
                result = state.Func();
            }
            catch (Exception ex)
            {
                state.TaskCompletionSource.SetException(ex);
                return;
            }

            state.TaskCompletionSource.SetResult(result);
        }, delegateState, true);

        return taskCompletionSource.Task;
    }

    public static Task<TResult> InvokeAsync<TState, TResult>(Func<TState, TResult> func, TState state)
    {
        TaskCompletionSource<TResult> taskCompletionSource = new();

        FuncState<TState, TResult> delegateState = new(taskCompletionSource, func, state);
        ThreadPool.QueueUserWorkItem(static state =>
        {
            TResult result;
            try
            {
                result = state.Func(state.State);
            }
            catch (Exception ex)
            {
                state.TaskCompletionSource.SetException(ex);
                return;
            }

            state.TaskCompletionSource.SetResult(result);
        }, delegateState, true);

        return taskCompletionSource.Task;
    }
}
