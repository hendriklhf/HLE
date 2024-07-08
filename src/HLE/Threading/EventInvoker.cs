using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Threading;

public static partial class EventInvoker
{
    [SkipLocalsInit]
    [SuppressMessage("Roslynator", "RCS1229:Use async/await when necessary", Justification = "'tasks' can be disposed before the returned task is awaited")]
    public static Task InvokeAsync<TSender, TEventArgs>(AsyncEventHandler<TSender, TEventArgs>? eventHandler, TSender sender, TEventArgs args)
    {
        if (eventHandler is null)
        {
            return Task.CompletedTask;
        }

        if (eventHandler.HasSingleTarget)
        {
            return eventHandler(sender, args);
        }

        Unsafe.SkipInit(out TaskBuffer buffer);
        using ValueList<Task> tasks = new(InlineArrayHelpers.AsSpan<TaskBuffer, Task>(ref buffer, TaskBuffer.Length));
        foreach (AsyncEventHandler<TSender, TEventArgs> target in Delegate.EnumerateInvocationList(eventHandler))
        {
            tasks.Add(target(sender, args));
        }

#pragma warning disable HAA0101 // no
        return Task.WhenAll(tasks.AsSpan());
#pragma warning restore HAA0101
    }

    public static void QueueOnThreadPool<TEventArgs>(EventHandler<TEventArgs>? eventHandler, object? sender, TEventArgs eventArgs)
    {
        if (eventHandler is null)
        {
            return;
        }

        if (eventHandler.HasSingleTarget)
        {
            EventHandlerState<TEventArgs> state = new(eventHandler, sender, eventArgs);
            ThreadPool.QueueUserWorkItem(DelegateCache<TEventArgs>.QueueUserWorkItem, state, true);
            return;
        }

        foreach (EventHandler<TEventArgs> target in Delegate.EnumerateInvocationList(eventHandler))
        {
            EventHandlerState<TEventArgs> state = new(target, sender, eventArgs);
            ThreadPool.QueueUserWorkItem(DelegateCache<TEventArgs>.QueueUserWorkItem, state, true);
        }
    }

    public static void QueueOnThreadPool(EventHandler? eventHandler, object? sender)
    {
        if (eventHandler is null)
        {
            return;
        }

        if (eventHandler.HasSingleTarget)
        {
            EventHandlerState state = new(eventHandler, sender);
            ThreadPool.QueueUserWorkItem(static state => state.EventHandler(state.Sender, EventArgs.Empty), state, true);
            return;
        }

        foreach (EventHandler target in Delegate.EnumerateInvocationList(eventHandler))
        {
            EventHandlerState state = new(target, sender);
            ThreadPool.QueueUserWorkItem(static state => state.EventHandler(state.Sender, EventArgs.Empty), state, true);
        }
    }
}
