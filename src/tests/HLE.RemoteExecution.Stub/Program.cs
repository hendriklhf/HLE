using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HLE.RemoteExecution.Stub;

internal static class Program
{
    private const BindingFlags AllVisibilities = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    static Program()
    {
        AssemblyResolver.Register();
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    private static Task<int> Main()
    {
        try
        {
            PrintEnvironmentVariables();

            string assemblyLocation = GetRequiredEnvironmentVariable(RemoteExecutionEnvironment.MethodAssembly);
            string declaringTypeName = GetRequiredEnvironmentVariable(RemoteExecutionEnvironment.MethodType);
            string methodName = GetRequiredEnvironmentVariable(RemoteExecutionEnvironment.Method);

            Assembly assembly = Assembly.LoadFile(assemblyLocation);

            Type? assemblyType = assembly.GetType(declaringTypeName);
            ArgumentNullException.ThrowIfNull(assemblyType);

            MethodInfo? method = assemblyType.GetMethod(methodName, AllVisibilities);
            ArgumentNullException.ThrowIfNull(method);

            object? instance = null;
            if (!method.IsStatic)
            {
                instance = CreateInstance();
            }

            ReadOnlySpan<ParameterInfo> parameterInfos = method.GetParameters();
            object?[]? parameters = parameterInfos.Length switch
            {
                0 => null,
                1 => [ParseParameter(parameterInfos[0].ParameterType)],
                _ => throw new InvalidOperationException("The method to invoke must have zero or one parameter.")
            };

            object? result = method.Invoke(instance, BindingFlags.DoNotWrapExceptions, null, parameters, null);
            return result is Task t ? AwaitTask(t) : Task.FromResult(0);
        }
        catch (Exception ex)
        {
            return Task.FromResult(HandleException(ex));
        }
    }

    private static object CreateInstance()
    {
        string assemblyLocation = GetRequiredEnvironmentVariable(RemoteExecutionEnvironment.InstanceAssembly);
        string typeName = GetRequiredEnvironmentVariable(RemoteExecutionEnvironment.InstanceType);

        Assembly assembly = Assembly.LoadFile(assemblyLocation);
        Type? type = assembly.GetType(typeName);
        ArgumentNullException.ThrowIfNull(type);

        ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        foreach (ConstructorInfo constructor in constructors)
        {
            ParameterInfo[] parameters = constructor.GetParameters();

            switch (parameters.Length)
            {
                case 0:
                    return constructor.Invoke(null);
                case 1 when parameters[0].ParameterType.Name == "ITestOutputHelper":
                    return constructor.Invoke([null]);
                default:
                    continue;
            }
        }

        ThrowNoConstructor(type);
        return null;

        [DoesNotReturn]
        static void ThrowNoConstructor(Type type)
            => throw new InvalidOperationException($"{type} does not have a parameterless constructor or a constructor that takes an ITestOutputHelper.");
    }

    private static async Task<int> AwaitTask(Task task)
    {
        try
        {
            await task;
            return 0;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    private static int HandleException(Exception ex)
    {
        Console.WriteLine(ex);
        return ex.HResult;
    }

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "analyzer is wrong")]
    private static unsafe object ParseParameter(Type parameterType)
    {
        string value = GetRequiredEnvironmentVariable(RemoteExecutionEnvironment.Argument);
        if (parameterType == typeof(string))
        {
            return value;
        }

        int size = int.Parse(GetRequiredEnvironmentVariable(RemoteExecutionEnvironment.ArgumentSize));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

        delegate*<string, int, object> decodeAndBox = CreateDecodeAndBoxMethod(parameterType);
        return decodeAndBox(value, size);
    }

    private static unsafe delegate*<string, int, object> CreateDecodeAndBoxMethod(Type parameterType)
        => (delegate*<string, int, object>)
            typeof(Program).GetMethod(nameof(DecodeAndBox), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(parameterType)
                .MethodHandle
                .GetFunctionPointer();

    private static object DecodeAndBox<T>(string base64, int size) where T : unmanaged
    {
        Span<byte> bytes = stackalloc byte[size];
        bool success = Convert.TryFromBase64Chars(base64, bytes, out int bytesWritten);
        if (!success || bytesWritten != size)
        {
            ThrowInvalidSize();
        }

        return Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(bytes));

        static void ThrowInvalidSize()
            => throw new InvalidOperationException("The size of the decoded argument does not match the expected size.");
    }

    private static void PrintEnvironmentVariables()
    {
        Console.WriteLine($"Started remote process with ID {Environment.ProcessId}.{Environment.NewLine}");

        IDictionary variables = Environment.GetEnvironmentVariables();
        if (variables.Count != 0)
        {
            Console.WriteLine("Environment variables:");

            foreach (DictionaryEntry variable in variables)
            {
                Console.WriteLine($"{variable.Key}=\"{variable.Value}\"");
            }

            Console.WriteLine();
        }
    }

    private static void OnProcessExit(object? sender, EventArgs e)
        => Console.WriteLine($"{Environment.NewLine}Remote process with ID {Environment.ProcessId} is exiting.");

    private static string GetRequiredEnvironmentVariable(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);

        if (value is null)
        {
            ThrowVariableNotFound(name);
        }

        return value;

        [DoesNotReturn]
        static void ThrowVariableNotFound(string name)
            => throw new InvalidOperationException($"The environment variable \"{name}\" was not found.");
    }
}
