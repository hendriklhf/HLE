using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using HLE.Collections;
using HLE.Memory;
using HLE.Strings;

namespace HLE.Resources;

public sealed class ResourceReader : IEquatable<ResourceReader>, ICountable
{
    public Assembly Assembly { get; }

    public int Count => _resources.Count;

    private readonly string _assemblyName;
    private readonly ConcurrentDictionary<string, byte[]?> _resources = new();

    public ResourceReader(Assembly assembly, bool readAllResourcesOnInitialization = false)
    {
        Assembly = assembly;
        string? assemblyName = assembly.GetName().Name;
        ArgumentException.ThrowIfNullOrEmpty(assemblyName);

        _assemblyName = assemblyName;
        if (readAllResourcesOnInitialization)
        {
            ReadAllResources();
        }
    }

    [Pure]
    [SkipLocalsInit]
    public byte[]? Read(ReadOnlySpan<char> resourceName)
    {
        ValueStringBuilder pathBuilder = new(stackalloc char[1 + _assemblyName.Length + resourceName.Length]);
        pathBuilder.Append(_assemblyName);
        pathBuilder.Append('.');
        pathBuilder.Append(resourceName);
        string resourcePath = StringPool.Shared.GetOrAdd(pathBuilder.WrittenSpan);
        return ReadResourceFromPath(resourcePath);
    }

    private void ReadAllResources()
    {
        Span<string> resourcePaths = Assembly.GetManifestResourceNames();
        for (int i = 0; i < resourcePaths.Length; i++)
        {
            _ = ReadResourceFromPath(resourcePaths[i]);
        }
    }

    private byte[]? ReadResourceFromPath(string resourcePath)
    {
        if (_resources.TryGetValue(resourcePath, out byte[]? resource))
        {
            return resource;
        }

        using Stream? stream = Assembly.GetManifestResourceStream(resourcePath);
        if (stream is null)
        {
            _resources.AddOrSet(resourcePath, null);
            return null;
        }

        if (stream.Length > int.MaxValue)
        {
            throw new NotSupportedException($"The stream length exceeds the the maximum {typeof(int)} value.");
        }

        int streamLength = (int)stream.Length;
        using PooledBufferWriter<byte> bufferWriter = new(streamLength);
        int sizeHint = streamLength < 1000 ? streamLength : 1000;
        int bytesRead = stream.Read(bufferWriter.GetSpan(sizeHint));
        bufferWriter.Advance(bytesRead);
        while (bytesRead < streamLength && bytesRead > 0)
        {
            bytesRead = stream.Read(bufferWriter.GetSpan(sizeHint));
            bufferWriter.Advance(bytesRead);
        }

        resource = bufferWriter.WrittenSpan.ToArray();
        _resources.AddOrSet(resourcePath, resource);
        return resource;
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
        return RuntimeHelpers.GetHashCode(this);
    }

    public static bool operator ==(ResourceReader? left, ResourceReader? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ResourceReader? left, ResourceReader? right)
    {
        return !(left == right);
    }
}
