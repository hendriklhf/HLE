#if !NET10_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HLE.Threading;

public static partial class EventInvoker
{
    [InlineArray(Length)]
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
    [SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration")]
    [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed")]
    [SuppressMessage("Roslynator", "RCS1169:Make field read-only")]
    [SuppressMessage("Style", "IDE0044:Add readonly modifier")]
    private struct TaskBuffer
    {
        private Task _task;

        private const int Length = 8;
    }
}
#endif
