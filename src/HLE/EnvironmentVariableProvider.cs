using System;
using System.Diagnostics.Contracts;

namespace HLE;

public static class EnvironmentVariableProvider
{
    [Pure]
    public static IEnvironmentVariableProvider Create()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsEnvironmentVariableProvider();
        }

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
        {
            return new UnixEnvironmentVariableProvider();
        }

        throw new NotSupportedException("The current operating system is not yet supported.");
    }
}
