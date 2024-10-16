using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using HLE.Text;

namespace HLE.TestRunner;

internal static class EnvironmentCombinator
{
    private static readonly string[] s_configurations = ["Debug", "Release"];
    private static readonly string[] s_runtimeIdentifiers = DetermineRuntimeIdentifiers();
    private static readonly EnvironmentVariable[] s_environmentVariables =
    [
        new("DOTNET_EnableAVX", ["0", "1"], Avx.IsSupported),
        new("DOTNET_EnableAVX2", ["0", "1"], Avx2.IsSupported),
        new("DOTNET_EnableAVX512F", ["0", "1"], Avx512F.IsSupported),
        new("DOTNET_EnableBMI1", ["0", "1"], Bmi1.IsSupported),
        new("DOTNET_EnableBMI2", ["0", "1"], Bmi2.IsSupported)
    ];

    [Pure]
    public static ImmutableArray<EnvironmentConfiguration> Combinate()
    {
        List<EnvironmentConfiguration> environmentConfigurations = new();

        foreach (string configuration in s_configurations)
        {
            foreach (string runtimeIdentifier in s_runtimeIdentifiers)
            {
                int applicableEnvironmentVariableCount = GetApplicableEnvironmentVariableCount();
                if (applicableEnvironmentVariableCount == 0)
                {
                    continue;
                }

                ReadOnlySpan<EnvironmentVariable> environmentVariables = s_environmentVariables;
                for (int i = 0; i < Math.Pow(2, applicableEnvironmentVariableCount); i++)
                {
                    string values = i.ToString($"b{applicableEnvironmentVariableCount}", CultureInfo.InvariantCulture);
                    EnvironmentConfiguration environmentConfiguration = new(configuration, runtimeIdentifier);
                    for (int j = 0; j < environmentVariables.Length; j++)
                    {
                        EnvironmentVariable environmentVariable = environmentVariables[j];
                        if (!environmentVariable.IsApplicable)
                        {
                            continue;
                        }

                        environmentConfiguration.AddEnvironmentVariable(environmentVariable.Name, SingleCharStringPool.GetOrAdd(values[j]));
                    }

                    environmentConfigurations.Add(environmentConfiguration);
                }
            }
        }

        return ImmutableCollectionsMarshal.AsImmutableArray(environmentConfigurations.ToArray());
    }

    private static int GetApplicableEnvironmentVariableCount()
    {
        ReadOnlySpan<EnvironmentVariable> environmentVariables = s_environmentVariables;
        int count = 0;
        for (int i = 0; i < environmentVariables.Length; i++)
        {
            if (environmentVariables[i].IsApplicable)
            {
                count++;
            }
        }

        return count;
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
