using System;
using System.Reflection;

namespace HLE.RemoteExecutorStub;

internal static class Program
{
    private const BindingFlags AllVisibilities = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    static Program() => AssemblyResolver.Register();

    private static int Main()
    {
        try
        {
            string assemblyLocation = Environment.GetEnvironmentVariable("HLE_REMOTE_EXECUTOR_ASSEMBLY")!;
            string declaringTypeName = Environment.GetEnvironmentVariable("HLE_REMOTE_EXECUTOR_TYPE")!;
            string methodName = Environment.GetEnvironmentVariable("HLE_REMOTE_EXECUTOR_METHOD")!;

            Assembly assembly = Assembly.LoadFile(assemblyLocation);
            Type? assemblyType = assembly.GetType(declaringTypeName);
            MethodInfo? method = assemblyType!.GetMethod(methodName, AllVisibilities);

            method!.Invoke(null, BindingFlags.DoNotWrapExceptions, null, null, null);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return ex.HResult;
        }
    }
}
