using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HLE.RemoteExecution;

public static class RemoteExecutor
{
    private static readonly string s_stubPath = Path.Combine(GetArtifactsPath(), "bin", "HLE.RemoteExecutorStub", $"{Configuration}_{GetFrameworkVersion()}", "HLE.RemoteExecutorStub.exe");

    private const string ArtifactsPathMetadataKey = "ArtifactsPath";
    private const string Configuration =
#if RELEASE
        "release";
#else
        "debug";
#endif

    public static Task<RemoteExecutorResult> InvokeAsync(Delegate method, RemoteExecutorOptions? options, params ReadOnlySpan<object?> args)
    {
        _ = args;

        // TODO: validate return type of method (void or Task)

        MethodInfo methodInfo = method.Method;
        string location = methodInfo.DeclaringType!.Assembly.Location;
        string declaringTypeName = methodInfo.DeclaringType!.FullName!;
        string methodName = methodInfo.Name;

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

        if (options?.EnvironmentVariables is { Count: not 0 } environmentVariables)
        {
            foreach ((string key, string value) in environmentVariables)
            {
                startInfo.Environment.Add(key, value);
            }
        }

        return StartProcessAsync(startInfo);
    }

    private static async Task<RemoteExecutorResult> StartProcessAsync(ProcessStartInfo startInfo)
    {
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
            .FirstOrDefault(static attribute => attribute.Key == ArtifactsPathMetadataKey)
            ?.Value;

        return artifactsPath ?? throw new InvalidOperationException($"The assembly does not have the \"{ArtifactsPathMetadataKey}\" metadata.");
    }
}
