using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE;

public static class EnvironmentVariableProvider
{
    [Pure]
    [SuppressMessage("Style", "IDE0046:Convert to conditional expression")]
    public static IEnvironmentVariableProvider Create()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsEnvironmentVariableProvider();
        }

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            return new UnixEnvironmentVariableProvider();
        }

        throw new NotSupportedException("The current operating system is not yet supported.");
    }
}
