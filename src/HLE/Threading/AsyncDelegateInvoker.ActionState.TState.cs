using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace HLE.Threading;

public static partial class AsyncDelegateInvoker
{
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    private readonly struct ActionState<TState>(TaskCompletionSource taskCompletionSource, Action<TState> action, TState state)
    {
        public TaskCompletionSource TaskCompletionSource { get; } = taskCompletionSource;

        public Action<TState> Action { get; } = action;

        public TState State { get; } = state;
    }
}
