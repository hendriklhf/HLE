using System;

namespace HLE.Threading;

public static partial class AsyncDelegateInvoker
{
    private static class InvokeAsyncCallbacks<T>
    {
        public static Action<ActionState<T>> InvokeAsyncAction { get; } = static state =>
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
        };

        public static Action<FuncState<T>> InvokeAsyncFunc { get; } = static state =>
        {
            T result;
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
        };
    }
}
