using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;
#if NET10_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace HLE.Threading;

public static partial class EventInvoker
{
    public static Task InvokeAsync<TSender, TEventArgs>(
        AsyncEventHandler<TSender, TEventArgs>? eventHandler,
        TSender sender,
        TEventArgs args,
        CancellationToken cancellationToken = default
    )
    {
        if (eventHandler is null)
        {
            return Task.CompletedTask;
        }

#if NET9_0_OR_GREATER
        return eventHandler.HasSingleTarget ? eventHandler(sender, args, cancellationToken) : InvokeMultiTargetAsync(eventHandler, sender, args, cancellationToken);
#else
        return InvokeMultiTargetAsync(eventHandler, sender, args, cancellationToken);
#endif
    }

    public static Task InvokeAsync<TSender>(
        AsyncEventHandler<TSender>? eventHandler,
        TSender sender,
        CancellationToken cancellationToken = default
    )
    {
        if (eventHandler is null)
        {
            return Task.CompletedTask;
        }

#if NET9_0_OR_GREATER
        return eventHandler.HasSingleTarget ? eventHandler(sender, cancellationToken) : InvokeMultiTargetAsync(eventHandler, sender, cancellationToken);
#else
        return InvokeMultiTargetAsync(eventHandler, sender, cancellationToken);
#endif
    }

    [SuppressMessage("Roslynator", "RCS1229:Use async/await when necessary", Justification = "'tasks' can be disposed before the returned task is awaited")]
    private static Task InvokeMultiTargetAsync<TSender>(AsyncEventHandler<TSender> eventHandler, TSender sender, CancellationToken cancellationToken = default)
    {
#if NET10_0_OR_GREATER
        InlineArray8<Task> buffer = default;
#else
        TaskBuffer buffer = default;
#endif
        using ValueList<Task> tasks = new(buffer);
        foreach (AsyncEventHandler<TSender> target in Delegate.EnumerateInvocationList(eventHandler))
        {
            tasks.Add(target(sender, cancellationToken));
        }

        return Task.WhenAll(tasks.AsSpan());
    }

    [SuppressMessage("Roslynator", "RCS1229:Use async/await when necessary", Justification = "'tasks' can be disposed before the returned task is awaited")]
    private static Task InvokeMultiTargetAsync<TSender, TEventArgs>(AsyncEventHandler<TSender, TEventArgs> eventHandler, TSender sender, TEventArgs args, CancellationToken cancellationToken = default)
    {
#if NET10_0_OR_GREATER
        InlineArray8<Task> buffer = default;
#else
        TaskBuffer buffer = default;
#endif
        using ValueList<Task> tasks = new(buffer);
        foreach (AsyncEventHandler<TSender, TEventArgs> target in Delegate.EnumerateInvocationList(eventHandler))
        {
            tasks.Add(target(sender, args, cancellationToken));
        }

        return Task.WhenAll(tasks.AsSpan());
    }

    public static void QueueOnThreadPool<TEventArgs>(EventHandler<TEventArgs>? eventHandler, object? sender, TEventArgs eventArgs)
    {
        if (eventHandler is null)
        {
            return;
        }

#if NET9_0_OR_GREATER
        if (eventHandler.HasSingleTarget)
        {
            EventHandlerState<TEventArgs> state = new(eventHandler, sender, eventArgs);
            ThreadPool.QueueUserWorkItem(static state => state.EventHandler(state.Sender, state.EventArgs), state, true);
            return;
        }
#endif

        InvokeMultiTarget(eventHandler, sender, eventArgs);
    }

    public static void QueueOnThreadPool(EventHandler? eventHandler, object? sender)
    {
        if (eventHandler is null)
        {
            return;
        }

#if NET9_0_OR_GREATER
        if (eventHandler.HasSingleTarget)
        {
            EventHandlerState state = new(eventHandler, sender);
            ThreadPool.QueueUserWorkItem(static state => state.EventHandler(state.Sender, EventArgs.Empty), state, true);
            return;
        }
#endif

        InvokeMultiTarget(eventHandler, sender);
    }

    private static void InvokeMultiTarget<TEventArgs>(EventHandler<TEventArgs> eventHandler, object? sender, TEventArgs eventArgs)
    {
        foreach (EventHandler<TEventArgs> target in Delegate.EnumerateInvocationList(eventHandler))
        {
            EventHandlerState<TEventArgs> state = new(target, sender, eventArgs);
            ThreadPool.QueueUserWorkItem(static state => state.EventHandler(state.Sender, state.EventArgs), state, true);
        }
    }

    private static void InvokeMultiTarget(EventHandler eventHandler, object? sender)
    {
        foreach (EventHandler target in Delegate.EnumerateInvocationList(eventHandler))
        {
            EventHandlerState state = new(target, sender);
            ThreadPool.QueueUserWorkItem(static state => state.EventHandler(state.Sender, EventArgs.Empty), state, true);
        }
    }
}
