using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HLE.Resources;

public sealed class ResourceReader
{
    private readonly Assembly _assembly;
    private readonly Dictionary<string, string?> _resources = new();

    public ResourceReader(Assembly assembly, bool readOnInit = true)
    {
        _assembly = assembly;
        if (readOnInit)
        {
            ReadAllResources();
        }
    }

    private void ReadAllResources()
    {
        Span<string> resourcePaths = _assembly.GetManifestResourceNames();
        for (int i = 0; i < resourcePaths.Length; i++)
        {
            _ = ReadResourceFromPath(resourcePaths[i]);
        }
    }

    public string? ReadResource(string resourceName)
    {
        string resourcePath = string.Join('.', _assembly.GetName().Name, resourceName);
        return ReadResourceFromPath(resourcePath);
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
}
