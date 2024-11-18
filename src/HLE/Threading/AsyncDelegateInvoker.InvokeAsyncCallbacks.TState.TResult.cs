using System;

namespace HLE.Threading;

public static partial class AsyncDelegateInvoker
{
    private static class InvokeAsyncCallbacks<TState, TResult>
    {
        internal static Action<FuncState<TState, TResult>> InvokeAsyncFunc { get; } = static state =>
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
        };
    }
}
