using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using HLE.Marshalling;

namespace HLE.TestRunner;

internal static class EnvironmentCombinator
{
    private static readonly string[] s_configurations = ["Debug", "Release"];
    private static readonly string[] s_runtimeIdentifiers = DetermineRuntimeIdentifiers();

    [Pure]
    public static ReadOnlyMemory<EnvironmentConfiguration> Combine()
    {
        List<EnvironmentConfiguration> environmentConfigurations = new();

        foreach (string configuration in s_configurations)
        {
            foreach (string runtimeIdentifier in s_runtimeIdentifiers)
            {
                EnvironmentConfiguration environmentConfiguration = new(configuration, runtimeIdentifier);
                environmentConfigurations.Add(environmentConfiguration);
            }
        }

        return ListMarshal.AsReadOnlyMemory(environmentConfigurations);
    }

    private static string[] DetermineRuntimeIdentifiers()
    {
        if (OperatingSystem.IsWindows())
        {
            return DetermineWindowsRuntimes();
        }

        if (OperatingSystem.IsLinux())
        {
            return DetermineLinuxRuntimes();
        }

        if (OperatingSystem.IsMacOS())
        {
            return DetermineMacOsRuntimes();
        }

        throw new PlatformNotSupportedException();
    }

    private static string[] DetermineWindowsRuntimes()
    {
        Debug.Assert(OperatingSystem.IsWindows());

        return RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => ["win-x64", "win-x86"],
            Architecture.X86 => ["win-x86"],
            Architecture.Arm64 => ["win-arm64"],
            _ => throw new PlatformNotSupportedException()
        };
    }

    private static string[] DetermineLinuxRuntimes()
    {
        Debug.Assert(OperatingSystem.IsLinux());

        return RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => ["linux-x64"],
            Architecture.Arm64 => ["linux-arm64", "linux-arm"],
            Architecture.Arm => ["linux-arm"],
            _ => throw new PlatformNotSupportedException()
        };
    }

    private static string[] DetermineMacOsRuntimes()
    {
        Debug.Assert(OperatingSystem.IsMacOS());

        return RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => ["osx-x64"],
            Architecture.Arm64 => ["osx-arm64"],
            _ => throw new PlatformNotSupportedException()
        };
    }
}
