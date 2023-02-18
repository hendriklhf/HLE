using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Resources;

public sealed class ResourceReader : ICopyable<string>
{
    private readonly Assembly _assembly;
    private readonly Dictionary<string, string?> _resources = new();

    public ResourceReader(Assembly assembly, bool readAllResourcesOnInit = true)
    {
        _assembly = assembly;
        if (readAllResourcesOnInit)
        {
            ReadAllResources();
        }
    }

    [Pure]
    public string? Read(string resourceName)
    {
        string resourcePath = string.Join('.', _assembly.GetName().Name, resourceName);
        return ReadResourceFromPath(resourcePath);
    }

    private void ReadAllResources()
    {
        Span<string> resourcePaths = _assembly.GetManifestResourceNames();
        for (int i = 0; i < resourcePaths.Length; i++)
        {
            _ = ReadResourceFromPath(resourcePaths[i]);
        }
    }

    private string? ReadResourceFromPath(string resourcePath)
    {
        if (_resources.TryGetValue(resourcePath, out string? resource))
        {
            return resource;
        }

        using Stream? stream = _assembly.GetManifestResourceStream(resourcePath);
        if (stream is null)
        {
            _resources.Add(resourcePath, null);
            return null;
        }

        using StreamReader reader = new(stream);
        resource = reader.ReadToEnd();
        _resources.Add(resourcePath, resource);
        return resource;
    }

    public void CopyTo(string[] destination)
    {
        CopyTo((Span<string>)destination);
    }

    public void CopyTo(Memory<string> destination)
    {
        CopyTo(destination.Span);
    }

    public void CopyTo(Span<string> destination)
    {
        CopyTo(ref MemoryMarshal.GetReference(destination));
    }

    public unsafe void CopyTo(ref string destination)
    {
        CopyTo((string*)Unsafe.AsPointer(ref destination));
    }

    public unsafe void CopyTo(string* destination)
    {
        string?[] resources = _resources.Values.ToArray();
        string* source = (string*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(resources));
        Unsafe.CopyBlock(destination, source, (uint)(resources.Length * sizeof(string)));
    }
}
