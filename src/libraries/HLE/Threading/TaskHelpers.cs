using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HLE.Threading;

public static class TaskHelpers
{
    extension(Task task)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Performance", "CA1822:Mark members as static")]
        public void Ignore()
        {
            // nop to suppress the warning of needing to await a Task
        }
    }
}
