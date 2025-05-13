using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Threading;

public static class TaskExtensions
{
    extension(Task)
    {
        [SuppressMessage("Roslynator", "RCS1046:Asynchronous method name should end with 'Async'")]
        [SuppressMessage("Minor Code Smell", "S4261:Methods should be named according to their synchronicities")]
        [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
        [SuppressMessage("Roslynator", "RCS1231:Make parameter ref read-only")]
        public static Task WhenAll(ReadOnlySpan<Task> tasks)
        {
            Task[] buffer = ArrayPool<Task>.Shared.Rent(tasks.Length);
            SpanHelpers.Copy(tasks, buffer);
            Task t = Task.WhenAll(buffer.Take(tasks.Length));
            SpanHelpers.Clear(buffer, tasks.Length);
            ArrayPool<Task>.Shared.Return(buffer);
            return t;
        }
    }
}
