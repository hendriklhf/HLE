using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HLE.RemoteExecution;

public static class RemoteExecutor
{
    private static readonly string s_stubPath = Path.Combine(GetArtifactsPath(), "bin", "HLE.RemoteExecution.Stub", $"{Configuration}_{GetFrameworkVersion()}", "HLE.RemoteExecution.Stub.exe");

    private const string ArtifactsPathMetadataKey = "ArtifactsPath";
    private const string Configuration =
#if RELEASE
        "release";
#else
        "debug";
#endif

    public static Task<RemoteExecutorResult> InvokeAsync(Delegate method, RemoteExecutorOptions? options = null)
    {
        ProcessStartInfo startInfo = BuildStartInfo(method, options);
        return StartProcessAsync(startInfo);
    }

    public static Task<RemoteExecutorResult> InvokeAsync<TArgument>(Delegate method, RemoteExecutorOptions? options, TArgument argument)
    {
        ValidateArgumentType<TArgument>();
        ProcessStartInfo startInfo = BuildStartInfo(method, options);
        AddArgument(startInfo.Environment, argument);
        return StartProcessAsync(startInfo);
    }

    private static ProcessStartInfo BuildStartInfo(Delegate method, RemoteExecutorOptions? options)
    {
        MethodInfo methodInfo = method.Method;
        ValidateMethodReturnType(methodInfo);

        ProcessStartInfo startInfo = new()
        {
            FileName = s_stubPath,
            WorkingDirectory = Environment.CurrentDirectory,
            RedirectStandardOutput = true
        };

        BuildEnvironment(startInfo.Environment, options, methodInfo, method.Target?.GetType());
        return startInfo;
    }

    private static unsafe void AddArgument<TArgument>(IDictionary<string, string?> environment, TArgument argument)
    {
        if (typeof(TArgument) == typeof(string))
        {
            environment.Add(RemoteExecutionEnvironment.Argument, Unsafe.As<string>(argument));
            return;
        }

        Debug.Assert(!RuntimeHelpers.IsReferenceOrContainsReferences<TArgument>());

        ReadOnlySpan<byte> bytes = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<TArgument, byte>(ref argument), sizeof(TArgument));
        string base64 = Convert.ToBase64String(bytes);
        environment.Add(RemoteExecutionEnvironment.Argument, base64);
        environment.Add(RemoteExecutionEnvironment.ArgumentSize, bytes.Length.ToString());
    }

    private static void BuildEnvironment(IDictionary<string, string?> environment, RemoteExecutorOptions? options, MethodInfo method, Type? callerType = null)
    {
        environment.Clear();

        Type? methodType = method.DeclaringType;
        ArgumentNullException.ThrowIfNull(methodType);
        Assembly methodAssembly = methodType.Assembly;

        environment.Add(RemoteExecutionEnvironment.MethodAssembly, methodAssembly.Location);
        environment.Add(RemoteExecutionEnvironment.MethodType, methodType.FullName);
        environment.Add(RemoteExecutionEnvironment.Method, method.Name);

        if (!method.IsStatic)
        {
            ArgumentNullException.ThrowIfNull(callerType);
            environment.Add(RemoteExecutionEnvironment.InstanceAssembly, callerType.Assembly.Location);
            environment.Add(RemoteExecutionEnvironment.InstanceType, callerType.FullName);
        }

        if (options?.EnvironmentVariables is { Count: not 0 } environmentVariables)
        {
            foreach ((string key, string value) in environmentVariables)
            {
                environment.Add(key, value);
            }
        }
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

    private static void ValidateMethodReturnType(MethodInfo method)
    {
        if (method.ReturnType != typeof(void) && !method.ReturnType.IsAssignableTo(typeof(Task)))
        {
            Throw();
        }

        return;

        [DoesNotReturn]
        static void Throw()
            => throw new ArgumentException($"The method must return nothing or a {typeof(Task)}.", nameof(method));
    }

    private static void ValidateArgumentType<T>()
    {
        if (typeof(T) == typeof(string))
        {
            return;
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            ThrowInvalidType();
        }

        return;

        [DoesNotReturn]
        static void ThrowInvalidType()
            => throw new ArgumentException("The argument type must be a string or struct that contains no managed types.");
    }
}
