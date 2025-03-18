using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HLE.Threading;

public static class TaskHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ignore(this Task _)
    {
        // nop to suppress the warning of needing to await a Task
    }
}
