using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace HLE.Threading;

public static partial class AsyncDelegateInvoker
{
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    private readonly struct ActionState(TaskCompletionSource taskCompletionSource, Action action)
    {
        public TaskCompletionSource TaskCompletionSource { get; } = taskCompletionSource;

        public Action Action { get; } = action;
    }
}
