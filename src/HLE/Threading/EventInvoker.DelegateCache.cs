using System;

namespace HLE.Threading;

public static partial class EventInvoker
{
    private static class DelegateCache<TEventArgs>
    {
        public static Action<EventHandlerState<TEventArgs>> QueueUserWorkItem { get; } = static state => state.EventHandler(state.Sender, state.EventArgs);
    }
}
