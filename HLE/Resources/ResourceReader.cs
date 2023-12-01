using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using HLE.Collections;
using HLE.Strings;

namespace HLE.Resources;

public sealed unsafe class ResourceReader : IEquatable<ResourceReader>
{
    public Assembly Assembly { get; }

    private readonly string _assemblyName;
    private readonly ConcurrentDictionary<string, Resource> _resources = new();

    public ResourceReader(Assembly assembly)
    {
        Assembly = assembly;
        string? assemblyName = assembly.GetName().Name;
        ArgumentException.ThrowIfNullOrEmpty(assemblyName);
        _assemblyName = assemblyName;
    }

    /// <summary>
    /// Tries to read a resource.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="resource">The resource bytes.</param>
    /// <returns>True, if the resource exists, false otherwise.</returns>
    public bool TryRead(ReadOnlySpan<char> resourceName, out Resource resource)
    {
        string resourcePath = BuildResourcePath(resourceName);
        return TryReadResource(resourcePath, out resource);
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

    private bool TryReadResource(string resourcePath, out Resource resource)
    {
        if (_resources.TryGetValue(resourcePath, out resource))
        {
            return resource != Resource.Empty;
        }

        using Stream? stream = Assembly.GetManifestResourceStream(resourcePath);
        if (stream is null)
        {
            _resources.AddOrSet(resourcePath, Resource.Empty);
            return false;
        }

        if (stream.Length > int.MaxValue)
        {
            ThrowStreamLengthExceedsInt32();
        }

        UnmanagedMemoryStream memoryStream = (UnmanagedMemoryStream)stream;
        memoryStream.Position = 0;
        byte* pointer = memoryStream.PositionPointer;
        resource = new(pointer, (int)memoryStream.Length);
        _resources.AddOrSet(resourcePath, resource);
        return true;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowStreamLengthExceedsInt32()
        => throw new NotSupportedException($"The stream length exceeds the the maximum {typeof(int)} value.");

    [Pure]
    public bool Equals([NotNullWhen(true)] ResourceReader? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ResourceReader? left, ResourceReader? right) => Equals(left, right);

    public static bool operator !=(ResourceReader? left, ResourceReader? right) => !(left == right);
}
