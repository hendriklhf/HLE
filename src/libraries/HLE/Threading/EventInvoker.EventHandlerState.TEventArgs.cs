using System;
using System.Diagnostics.CodeAnalysis;

namespace HLE.Threading;

public static partial class EventInvoker
{
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    private readonly struct EventHandlerState<TEventArgs>(EventHandler<TEventArgs> eventHandler, object? sender, TEventArgs eventArgs)
    {
        public EventHandler<TEventArgs> EventHandler { get; } = eventHandler;

        public object? Sender { get; } = sender;

        public TEventArgs EventArgs { get; } = eventArgs;
    }
}
