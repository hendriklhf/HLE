using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace HLE.RemoteExecution.Stub;

internal static class Program
{
    private const BindingFlags AllVisibilities = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    static Program() => AssemblyResolver.Register();

    private static Task<int> Main()
    {
        try
        {
            string? assemblyLocation = Environment.GetEnvironmentVariable("HLE_REMOTE_EXECUTOR_ASSEMBLY");
            ArgumentNullException.ThrowIfNull(assemblyLocation);
            string? declaringTypeName = Environment.GetEnvironmentVariable("HLE_REMOTE_EXECUTOR_TYPE");
            ArgumentNullException.ThrowIfNull(declaringTypeName);
            string? methodName = Environment.GetEnvironmentVariable("HLE_REMOTE_EXECUTOR_METHOD");
            ArgumentNullException.ThrowIfNull(methodName);

            Assembly assembly = Assembly.LoadFile(assemblyLocation);

            Type? assemblyType = assembly.GetType(declaringTypeName);
            ArgumentNullException.ThrowIfNull(assemblyType);

            MethodInfo? method = assemblyType.GetMethod(methodName, AllVisibilities);
            ArgumentNullException.ThrowIfNull(method);

            object? instance = null;
            if (!method.IsStatic)
            {
                instance = CreateInstance(assemblyType);
            }

            object? result = method.Invoke(instance, BindingFlags.DoNotWrapExceptions, null, null, null);
            return result is Task t ? AwaitTask(t) : Task.FromResult(0);
        }
        catch (Exception ex)
        {
            return Task.FromResult(HandleException(ex));
        }
    }

    private static object CreateInstance(Type type)
    {
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
        return null!;

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
}
