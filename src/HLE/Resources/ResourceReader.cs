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
using System.Threading;
using HLE.Collections;
using HLE.Memory;
using HLE.Text;

namespace HLE.Resources;

public sealed unsafe partial class ResourceReader(Assembly assembly) :
    IDisposable,
    IEquatable<ResourceReader>,
    IReadOnlyCollection<Resource>
{
    int IReadOnlyCollection<Resource>.Count => _resources.Count;

    private readonly Assembly _assembly = assembly;
    private readonly ConcurrentDictionary<string, Resource?> _resourceMap = new();
    private readonly List<Resource> _resources = [];
    private List<NativeMemory<byte>>? _handles;

    ~ResourceReader() => DisposeCore();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DisposeCore();
    }

    private void DisposeCore()
    {
        List<NativeMemory<byte>>? handlesList = Interlocked.Exchange(ref _handles, null);
        if (handlesList is null)
        {
            return;
        }

        ReadOnlySpan<NativeMemory<byte>> handles = CollectionsMarshal.AsSpan(handlesList);
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i].Dispose();
        }
    }

    [Pure]
    public Resource Read(ref PooledInterpolatedStringHandler resourcePath)
    {
        Resource resource = Read(resourcePath.Text);
        resourcePath.Dispose();
        return resource;
    }

    [Pure]
    public Resource Read(ReadOnlySpan<char> resourcePath) => Read(StringPool.Shared.GetOrAdd(resourcePath));

    [Pure]
    public Resource Read(string resourcePath)
    {
        if (!TryRead(resourcePath, out Resource resource))
        {
            ThrowResourceDoesntExist(resourcePath);
        }

        return resource;

        [DoesNotReturn]
        static void ThrowResourceDoesntExist(ReadOnlySpan<char> resourcePath)
            => throw new InvalidOperationException($"The resource \"{resourcePath}\" doesn't exist.");
    }

    /// <inheritdoc cref="TryRead(ReadOnlySpan{char},out Resource)"/>
    public bool TryRead(ref PooledInterpolatedStringHandler resourcePath, out Resource resource)
    {
        bool success = TryRead(resourcePath.Text, out resource);
        resourcePath.Dispose();
        return success;
    }

    /// <inheritdoc cref="TryRead(ReadOnlySpan{char},out Resource)"/>
    public bool TryRead(string resourcePath, out Resource resource)
    {
        if (!_resourceMap.TryGetValue(resourcePath, out Resource? nullableResource))
        {
            return TryReadCore(resourcePath, out resource);
        }

        if (nullableResource is null)
        {
            resource = default;
            return false;
        }

        resource = nullableResource.Value;
        return true;
    }

    /// <summary>
    /// Tries to read a resource.
    /// </summary>
    /// <param name="resourcePath">The path of the resource.</param>
    /// <param name="resource">The resource bytes.</param>
    /// <returns>True, if the resource exists, false otherwise.</returns>
    public bool TryRead(ReadOnlySpan<char> resourcePath, out Resource resource)
    {
        ConcurrentDictionary<string, Resource?>.AlternateLookup<ReadOnlySpan<char>> alternateLookup = _resourceMap.GetAlternateLookup<ReadOnlySpan<char>>();
        if (!alternateLookup.TryGetValue(resourcePath, out Resource? nullableResource))
        {
            return TryReadCore(StringPool.Shared.GetOrAdd(resourcePath), out resource);
        }

        if (nullableResource is null)
        {
            resource = default;
            return false;
        }

        resource = nullableResource.Value;
        return true;
    }

    private bool TryReadCore(string resourcePath, out Resource resource)
    {
        using Stream? stream = _assembly.GetManifestResourceStream(resourcePath);
        if (stream is null)
        {
            _resourceMap.AddOrSet(resourcePath, null);
            resource = default;
            return false;
        }

        if (stream.Length > int.MaxValue)
        {
            ThrowStreamLengthExceedsInt32();
        }

        int streamLength = (int)stream.Length;
        if (!_assembly.IsCollectible)
        {
            if (stream is UnmanagedMemoryStream memoryStream)
            {
                memoryStream.Position = 0;
                byte* pointer = memoryStream.PositionPointer;
                resource = new(pointer, streamLength);
                _resourceMap.AddOrSet(resourcePath, resource);
                AddResource(resource);
                return true;
            }

            Debug.Fail($"The implementation of {nameof(_assembly.GetManifestResourceStream)} has changed.");
        }

        // fallback for the case that the implementation of GetManifestResourceStream has changed or the Assembly is collectible

        NativeMemory<byte> buffer = NativeMemory<byte>.Alloc(streamLength, false);
        StoreHandle(buffer);

        stream.ReadExactly(buffer.AsSpan());

        resource = new(buffer.Pointer, streamLength);
        _resourceMap.AddOrSet(resourcePath, resource);
        AddResource(resource);
        return true;

        [DoesNotReturn]
        static void ThrowStreamLengthExceedsInt32()
            => throw new InvalidOperationException($"The stream length exceeds the maximum {typeof(int)} value.");
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void AddResource(Resource resource) => _resources.Add(resource);

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void StoreHandle(NativeMemory<byte> handle)
    {
        _handles ??= [];
        _handles.Add(handle);
    }

    [Pure]
    public Enumerator GetEnumerator() => new(_resources);

    IEnumerator<Resource> IEnumerable<Resource>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] ResourceReader? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ResourceReader? left, ResourceReader? right) => Equals(left, right);

    public static bool operator !=(ResourceReader? left, ResourceReader? right) => !(left == right);
}
