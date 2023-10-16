using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

    /// <summary>
    /// Tries to read a resource.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="resource">The resource bytes.</param>
    /// <returns>True, if the resource exists, false otherwise.</returns>
    public bool TryRead(ReadOnlySpan<char> resourceName, out ImmutableArray<byte> resource)
    {
        string resourcePath = BuildResourcePath(resourceName);
        byte[]? resourceArray = ReadResource(resourcePath);
        if (resourceArray is null)
        {
            resource = ImmutableArray<byte>.Empty;
            return false;
        }

        resource = ImmutableCollectionsMarshal.AsImmutableArray(resourceArray);
        return true;
    }

    [SkipLocalsInit]
    private string BuildResourcePath(ReadOnlySpan<char> resourceName)
    {
        ValueStringBuilder pathBuilder = new(stackalloc char[1 + _assemblyName.Length + resourceName.Length]);
        pathBuilder.Append(_assemblyName);
        pathBuilder.Append('.');
        pathBuilder.Append(resourceName);
        return StringPool.Shared.GetOrAdd(pathBuilder.WrittenSpan);
    }

    private void ReadAllResources()
    {
        ReadOnlySpan<string> resourcePaths = Assembly.GetManifestResourceNames();
        for (int i = 0; i < resourcePaths.Length; i++)
        {
            ReadResource(resourcePaths[i]);
        }
    }

    private byte[]? ReadResource(string resourcePath)
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
            ThrowStreamLengthExceedsInt32();
        }

        int streamLength = (int)stream.Length;
        using PooledBufferWriter<byte> bufferWriter = new(streamLength);
        int bytesRead = stream.Read(bufferWriter.GetSpan(streamLength));
        bufferWriter.Advance(bytesRead);
        while (bytesRead > 0)
        {
            bytesRead = stream.Read(bufferWriter.GetSpan(streamLength));
            bufferWriter.Advance(bytesRead);
        }

        resource = GC.AllocateUninitializedArray<byte>(bufferWriter.Count, true);
        bufferWriter.CopyTo(resource);
        _resources.AddOrSet(resourcePath, resource);
        return resource;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowStreamLengthExceedsInt32()
        => throw new NotSupportedException($"The stream length exceeds the the maximum {typeof(int)} value.");

    [Pure]
    public bool Equals(ResourceReader? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ResourceReader? left, ResourceReader? right) => Equals(left, right);

    public static bool operator !=(ResourceReader? left, ResourceReader? right) => !(left == right);
}
