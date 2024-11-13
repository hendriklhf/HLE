using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace HLE.Threading;

public static partial class AsyncDelegateInvoker
{
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    private readonly struct FuncState<TState, TResult>(TaskCompletionSource<TResult> taskCompletionSource, Func<TState, TResult> func, TState state)
    {
        public TaskCompletionSource<TResult> TaskCompletionSource { get; } = taskCompletionSource;

        public Func<TState, TResult> Func { get; } = func;

        public TState State { get; } = state;
    }
}
