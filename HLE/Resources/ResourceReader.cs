using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;
using HLE.Strings;

namespace HLE.Resources;

public sealed class ResourceReader : ICopyable<string>, IEquatable<ResourceReader>
{
    public int Count => _resources.Count;

    private readonly Assembly _assembly;
    private readonly string _assemblyName;
    private readonly Dictionary<string, string?> _resources = new();

    public ResourceReader(Assembly assembly, bool readAllResourcesOnInit = true)
    {
        _assembly = assembly;
        _assemblyName = _assembly.GetName().Name ?? throw new ArgumentNullException(nameof(assembly), "Assembly name is null.");
        if (readAllResourcesOnInit)
        {
            ReadAllResources();
        }
    }

    [Pure]
    public string? Read(ReadOnlySpan<char> resourceName)
    {
        ValueStringBuilder pathBuilder = stackalloc char[1 + _assemblyName.Length + resourceName.Length];
        pathBuilder.Append(_assemblyName);
        pathBuilder.Append('.');
        pathBuilder.Append(resourceName);
        return ReadResourceFromPath(StringPool.Shared.GetOrAdd(pathBuilder.WrittenSpan));
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
        ref string? resource = ref CollectionsMarshal.GetValueRefOrAddDefault(_resources, resourcePath, out bool exists);
        if (exists)
        {
            return resource;
        }

        using Stream? stream = _assembly.GetManifestResourceStream(resourcePath);
        if (stream is null)
        {
            resource = null;
            return null;
        }

        using StreamReader reader = new(stream);
        resource = reader.ReadToEnd();
        return resource;
    }

    public void CopyTo(string[] destination, int offset = 0)
    {
        CopyTo(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(destination), offset));
    }

    public void CopyTo(Memory<string> destination)
    {
        CopyTo(ref MemoryMarshal.GetReference(destination.Span));
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
        string?[] resources = _resources.Values.Where(v => v is not null).ToArray();
        string* source = (string*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(resources));
        Unsafe.CopyBlock(destination, source, (uint)(resources.Length * sizeof(string)));
    }

    [Pure]
    public bool Equals(ResourceReader? other)
    {
        return ReferenceEquals(this, other);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj);
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine(_assembly, _assemblyName, _resources);
    }
}
