using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HLE.Threading;

public static unsafe class TaskHelpers
{
    private static readonly delegate*<ReadOnlySpan<Task>, Task> s_whenAll =
        (delegate*<ReadOnlySpan<Task>, Task>)
        typeof(Task).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .First(static m => m.Name == nameof(Task.WhenAll) && m.GetParameters()[0].ParameterType == typeof(ReadOnlySpan<Task>))
            .MethodHandle.GetFunctionPointer();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ignore(this Task _)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Roslynator", "RCS1046:Asynchronous method name should end with \'Async\'")]
    public static Task WhenAll(ReadOnlySpan<Task> tasks) => s_whenAll(tasks);
}
