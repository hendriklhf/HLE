#if DEBUG
using System;
using System.Reflection;

namespace HLE;

internal static class TestRunnerHelpers
{
    public static bool IsTestRun { get; } = InitializeIsTestRun();

    private static bool InitializeIsTestRun()
    {
        Assembly? entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly is null)
        {
            return false;
        }

        string entryAssemblyName = entryAssembly.GetName().FullName;
        return entryAssemblyName.Contains("test", StringComparison.OrdinalIgnoreCase) ||
               entryAssemblyName.Contains("runner", StringComparison.OrdinalIgnoreCase);
    }
}

#endif
