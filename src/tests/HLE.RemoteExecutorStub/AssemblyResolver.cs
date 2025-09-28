using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace HLE.RemoteExecutorStub;

internal static class AssemblyResolver
{
    private static uint s_registered;

    public static void Register()
    {
        if (Interlocked.Exchange(ref s_registered, 1) != 0)
        {
            return;
        }

        AppDomain.CurrentDomain.AssemblyResolve += Resolve;
    }

    private static Assembly? Resolve(object? sender, ResolveEventArgs args)
    {
        AssemblyName assemblyName = new(args.Name);

        IEnumerable<string> files = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (string.Equals(fileName, assemblyName.Name, StringComparison.OrdinalIgnoreCase))
            {
                return Assembly.LoadFile(file);
            }
        }

        return null;
    }
}
