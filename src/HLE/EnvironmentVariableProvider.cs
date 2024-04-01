using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE;

internal static class EnvironmentVariableProvider
{
    private static readonly IEnvironmentVariableProvider s_provider = CreateProvider();

    [Pure]
    public static EnvironmentVariables GetEnvironmentVariables() => s_provider.GetEnvironmentVariables();

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression")]
    private static IEnvironmentVariableProvider CreateProvider()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsEnvironmentVariableProvider();
        }

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            return new UnixEnvironmentVariableProvider();
        }

        ThrowHelper.ThrowOperatingSystemNotSupported();
        return null!;
    }
}
