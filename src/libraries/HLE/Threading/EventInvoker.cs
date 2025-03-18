using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Threading;

public static partial class EventInvoker
{
    public static Task InvokeAsync<TSender, TEventArgs>(AsyncEventHandler<TSender, TEventArgs>? eventHandler, TSender sender, TEventArgs args)
    {
        if (eventHandler is null)
        {
            return Task.CompletedTask;
        }

        return eventHandler.HasSingleTarget ? eventHandler(sender, args) : InvokeMultiTargetAsync(eventHandler, sender, args);
    }

    public static Task InvokeAsync<TSender>(AsyncEventHandler<TSender>? eventHandler, TSender sender)
    {
        if (eventHandler is null)
        {
            return Task.CompletedTask;
        }

        return eventHandler.HasSingleTarget ? eventHandler(sender) : InvokeMultiTargetAsync(eventHandler, sender);
    }

    [SuppressMessage("Roslynator", "RCS1229:Use async/await when necessary", Justification = "'tasks' can be disposed before the returned task is awaited")]
    private static Task InvokeMultiTargetAsync<TSender>(AsyncEventHandler<TSender> eventHandler, TSender sender)
    {
        TaskBuffer buffer = default;
        using ValueList<Task> tasks = new(InlineArrayHelpers.AsSpan<TaskBuffer, Task>(ref buffer));
        foreach (AsyncEventHandler<TSender> target in Delegate.EnumerateInvocationList(eventHandler))
        {
            tasks.Add(target(sender));
        }

        return Task.WhenAll(tasks.AsSpan());
    }

    [SuppressMessage("Roslynator", "RCS1229:Use async/await when necessary", Justification = "'tasks' can be disposed before the returned task is awaited")]
    private static Task InvokeMultiTargetAsync<TSender, TEventArgs>(AsyncEventHandler<TSender, TEventArgs> eventHandler, TSender sender, TEventArgs args)
    {
        TaskBuffer buffer = default;
        using ValueList<Task> tasks = new(InlineArrayHelpers.AsSpan<TaskBuffer, Task>(ref buffer));
        foreach (AsyncEventHandler<TSender, TEventArgs> target in Delegate.EnumerateInvocationList(eventHandler))
        {
            tasks.Add(target(sender, args));
        }

        return Task.WhenAll(tasks.AsSpan());
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
            ThreadPool.QueueUserWorkItem(static state => state.EventHandler(state.Sender, state.EventArgs), state, true);
            return;
        }

        InvokeMultiTarget(eventHandler, sender, eventArgs);
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
