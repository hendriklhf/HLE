using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

public sealed unsafe class ResourceReader : IDisposable, IEquatable<ResourceReader>, IReadOnlyCollection<Resource>
{
    public Assembly Assembly { get; }

    int IReadOnlyCollection<Resource>.Count => _resources.Count;

    private readonly string _assemblyName;
    private readonly ConcurrentDictionary<string, Resource> _resources = new();

    private List<GCHandle>? _handles;
    private bool _disposed;

    public ResourceReader(Assembly assembly)
    {
        Assembly = assembly;
        string? assemblyName = assembly.GetName().Name;
        ArgumentException.ThrowIfNullOrEmpty(assemblyName);
        _assemblyName = $"{assemblyName}.";
    }

    public void Dispose()
    {
        if (_disposed || _handles is null)
        {
            return;
        }

        _disposed = true;
        Span<GCHandle> handles = CollectionsMarshal.AsSpan(_handles);
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i].Free();
        }
    }

    [Pure]
    public Resource Read(ReadOnlySpan<char> resourceName)
    {
        ThrowIfDisposed();

        string resourcePath = BuildResourcePath(resourceName);
        if (!TryReadCore(resourcePath, out Resource resource))
        {
            ThrowResourceDoesntExist(resourcePath);
        }

        return resource;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            ThrowHelper.ThrowObjectDisposedException<ResourceReader>();
        }
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
        ThrowIfDisposed();

        string resourcePath = BuildResourcePath(resourceName);
        return TryReadCore(resourcePath, out resource);
    }

    [SkipLocalsInit]
    private string BuildResourcePath(ReadOnlySpan<char> resourceName)
    {
        UnsafeBufferWriter<char> bufferWriter = new(stackalloc char[_assemblyName.Length + resourceName.Length]);
        bufferWriter.Write(_assemblyName); // _assemblyName ends with a '.'
        bufferWriter.Write(resourceName);

        return StringPool.Shared.GetOrAdd(bufferWriter.WrittenSpan);
    }

    private bool TryReadCore(string resourcePath, out Resource resource)
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

        // fallback for the case that the implementation of GetManifestResourceStream has changed
        byte[] buffer = GC.AllocateUninitializedArray<byte>(streamLength, true);
        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Normal);
        StoreHandle(handle);

        int bytesRead = 0;
        do
        {
            bytesRead += stream.Read(buffer);
        }
        while (bytesRead != streamLength);

        byte* bufferPointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(buffer));
        resource = new(bufferPointer, streamLength);
        _resources.AddOrSet(resourcePath, resource);
        return true;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void StoreHandle(GCHandle handle)
    {
        _handles ??= new(2);
        _handles.Add(handle);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowStreamLengthExceedsInt32()
        => throw new NotSupportedException($"The stream length exceeds the the maximum {typeof(int)} value.");

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
