using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HLE.TestUtilities;

public static class RemoteExecutor
{
    private static readonly string s_stubPath = Path.Combine(GetArtifactsPath(), "bin", "HLE.RemoteExecutorStub", $"{GetConfiguration()}_{GetFrameworkVersion()}", "HLE.RemoteExecutorStub.exe");

    public static async Task<RemoteExecutorResult> InvokeAsync(MethodInfo method)
    {
        if (!method.IsStatic)
        {
            throw new InvalidOperationException();
        }

        string location = method.DeclaringType!.Assembly.Location;
        string declaringTypeName = method.DeclaringType!.FullName!;
        string methodName = method.Name;

        ProcessStartInfo startInfo = new()
        {
            FileName = s_stubPath,
            WorkingDirectory = Environment.CurrentDirectory,
            RedirectStandardOutput = true,
            Environment =
            {
                ["HLE_REMOTE_EXECUTOR_ASSEMBLY"] = location,
                ["HLE_REMOTE_EXECUTOR_TYPE"] = declaringTypeName,
                ["HLE_REMOTE_EXECUTOR_METHOD"] = methodName
            }
        };

        using Process? process = Process.Start(startInfo);
        ArgumentNullException.ThrowIfNull(process);

        await process.WaitForExitAsync();

        string output = await process.StandardOutput.ReadToEndAsync();

        return new()
        {
            ExitCode = process.ExitCode,
            Output = output
        };
    }

    private static string GetConfiguration()
#if RELEASE
        => "release";
#else
        => "debug";
#endif

    private static string GetFrameworkVersion()
    {
        if (RuntimeInformation.FrameworkDescription.StartsWith(".NET 10.0", StringComparison.OrdinalIgnoreCase))
        {
            return "net10.0";
        }

        if (RuntimeInformation.FrameworkDescription.StartsWith(".NET 9.0", StringComparison.OrdinalIgnoreCase))
        {
            return "net9.0";
        }

        if (RuntimeInformation.FrameworkDescription.StartsWith(".NET 8.0", StringComparison.OrdinalIgnoreCase))
        {
            return "net8.0";
        }

        throw new NotSupportedException();
    }

    private static string GetArtifactsPath()
    {
        string? artifactsPath = typeof(RemoteExecutor).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(static attribute => attribute.Key == "ArtifactsPath")
            ?.Value;

        return artifactsPath ?? throw new InvalidOperationException("The assembly does not have the 'ArtifactsPath' metadata.");
    }
}
