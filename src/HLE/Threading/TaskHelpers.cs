using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HLE.Threading;

public static unsafe class TaskHelpers
{
    private static readonly delegate*<ReadOnlySpan<Task>, Task> s_whenAll = GetWhenAllFunctionPointer();

    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields")]
    private static delegate*<ReadOnlySpan<Task>, Task> GetWhenAllFunctionPointer()
    {
        MethodInfo? whenAllMethodInfo = Array.Find(typeof(Task).GetMethods(BindingFlags.NonPublic | BindingFlags.Static),
            static m => m.Name == nameof(Task.WhenAll) && m.GetParameters()[0].ParameterType == typeof(ReadOnlySpan<Task>));

        if (whenAllMethodInfo is not null)
        {
            return (delegate*<ReadOnlySpan<Task>, Task>)whenAllMethodInfo.MethodHandle.GetFunctionPointer();
        }

        Debug.Fail($"Using {nameof(Task.WhenAll)} fallback!");
        return &WhenAllFallback;
    }

    [SuppressMessage("Roslynator", "RCS1046:Asynchronous method name should end with \'Async\'")]
    [SuppressMessage("Minor Code Smell", "S4261:Methods should be named according to their synchronicities")]
    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
    private static Task WhenAllFallback(ReadOnlySpan<Task> tasks) => Task.WhenAll(tasks.ToArray());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ignore(this Task _)
    {
        // nop to suppress the warning of needing to await a Task
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Roslynator", "RCS1046:Asynchronous method name should end with \'Async\'")]
    [SuppressMessage("Minor Code Smell", "S4261:Methods should be named according to their synchronicities")]
    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
    public static Task WhenAll(ReadOnlySpan<Task> tasks) => s_whenAll(tasks);
}
