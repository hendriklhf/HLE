using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Memory;
using HLE.Strings;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Resources;

public sealed unsafe class ResourceReader : IDisposable, IEquatable<ResourceReader>, IReadOnlyCollection<Resource>
{
    public Assembly Assembly { get; }

    int IReadOnlyCollection<Resource>.Count => _resources.Count;

    private readonly string _assemblyName;
    private readonly ConcurrentDictionary<string, Resource> _resources = new();

    private List<GCHandle>? _handles;

    [MustDisposeResource]
    public ResourceReader(Assembly assembly)
    {
        Assembly = assembly;
        string? assemblyName = assembly.GetName().Name;
        ArgumentException.ThrowIfNullOrEmpty(assemblyName);
        _assemblyName = $"{assemblyName}.";
    }

    public void Dispose()
    {
        List<GCHandle>? handlesList = _handles;
        if (handlesList is null)
        {
            return;
        }

        Span<GCHandle> handles = CollectionsMarshal.AsSpan(handlesList);
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i].Free();
        }

        _handles = null;
    }

    [Pure]
    public Resource Read(ReadOnlySpan<char> resourceName)
    {
        string resourcePath = BuildResourcePath(resourceName);
        if (!TryReadCore(resourcePath, out Resource resource))
        {
            ThrowResourceDoesntExist(resourcePath);
        }

        return resource;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowResourceDoesntExist(ReadOnlySpan<char> resourcePath)
        => throw new InvalidOperationException($"The resource \"{resourcePath}\" doesn't exist.");

    /// <summary>
    /// Tries to read a resource.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="resource">The resource bytes.</param>
    /// <returns>True, if the resource exists, false otherwise.</returns>
    public bool TryRead(ReadOnlySpan<char> resourceName, out Resource resource)
    {
        string resourcePath = BuildResourcePath(resourceName);
        return TryReadCore(resourcePath, out resource);
    }

    [SkipLocalsInit]
    private string BuildResourcePath(ReadOnlySpan<char> resourceName)
    {
        string assemblyName = _assemblyName;
        Span<char> buffer = stackalloc char[assemblyName.Length + resourceName.Length];
        SpanHelpers<char>.Copy(assemblyName, buffer);
        SpanHelpers<char>.Copy(resourceName, buffer.SliceUnsafe(assemblyName.Length));

        return StringPool.Shared.GetOrAdd(buffer);
    }

    private bool TryReadCore(string resourcePath, out Resource resource)
    {
        if (_resources.TryGetValue(resourcePath, out resource))
        {
            return resource != default;
        }

        using Stream? stream = Assembly.GetManifestResourceStream(resourcePath);
        if (stream is null)
        {
            _resources.AddOrSet(resourcePath, default);
            return false;
        }

        if (stream.Length > int.MaxValue)
        {
            ThrowStreamLengthExceedsInt32();
        }

        int streamLength = (int)stream.Length;
        if (stream is UnmanagedMemoryStream memoryStream)
        {
            memoryStream.Position = 0;
            byte* pointer = memoryStream.PositionPointer;
            resource = new(pointer, streamLength);
            _resources.AddOrSet(resourcePath, resource);
            return true;
        }

        Debug.Fail($"The implementation of {nameof(Assembly.GetManifestResourceStream)} has changed.");

        if (streamLength > Array.MaxLength)
        {
            ThrowStreamLengthExceedsMaxArrayLength();
        }

        // fallback for the case that the implementation of GetManifestResourceStream has changed
        byte[] buffer = GC.AllocateUninitializedArray<byte>(streamLength, true);
        StoreHandle(GCHandle.Alloc(buffer));

        int bytesRead = 0;
        do
        {
            bytesRead += stream.Read(buffer);
        }
        while (bytesRead != buffer.Length);

        byte* bufferPointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(buffer));
        resource = new(bufferPointer, streamLength);
        _resources.AddOrSet(resourcePath, resource);
        return true;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void StoreHandle(GCHandle handle)
    {
        _handles ??= new(8);
        _handles.Add(handle);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowStreamLengthExceedsInt32()
        => throw new InvalidOperationException($"The stream length exceeds the maximum {typeof(int)} value.");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowStreamLengthExceedsMaxArrayLength()
        => throw new InvalidOperationException($"The stream length exceeds the maximum {typeof(int)} value.");

    [Pure]
    public bool Equals([NotNullWhen(true)] ResourceReader? other) => ReferenceEquals(this, other);

    public IEnumerator<Resource> GetEnumerator() => _resources.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ResourceReader? left, ResourceReader? right) => Equals(left, right);

    public static bool operator !=(ResourceReader? left, ResourceReader? right) => !(left == right);
}
