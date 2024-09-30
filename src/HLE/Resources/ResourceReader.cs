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
using HLE.Text;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Resources;

[method: MustDisposeResource]
public sealed unsafe partial class ResourceReader(Assembly assembly) : IDisposable, IEquatable<ResourceReader>, IReadOnlyCollection<Resource>
{
    int IReadOnlyCollection<Resource>.Count => _resourceMap.Count;

    private readonly Assembly _assembly = assembly;
    private readonly ConcurrentDictionary<string, Resource?> _resourceMap = new();
    private readonly List<Resource> _resources = [];
    private List<GCHandle>? _handles;

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
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowResourceDoesntExist(ReadOnlySpan<char> resourcePath)
            => throw new InvalidOperationException($"The resource \"{resourcePath}\" doesn't exist.");
    }

    public bool TryRead(ref PooledInterpolatedStringHandler resourcePath, out Resource resource)
    {
        bool success = TryRead(resourcePath.Text, out resource);
        resourcePath.Dispose();
        return success;
    }

    /// <summary>
    /// Tries to read a resource.
    /// </summary>
    /// <param name="resourcePath">The path of the resource.</param>
    /// <param name="resource">The resource bytes.</param>
    /// <returns>True, if the resource exists, false otherwise.</returns>
    public bool TryRead(ReadOnlySpan<char> resourcePath, out Resource resource) => TryRead(StringPool.Shared.GetOrAdd(resourcePath), out resource);

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

        if (streamLength > Array.MaxLength)
        {
            ThrowStreamLengthExceedsMaxArrayLength();
        }

        byte[] buffer = GC.AllocateUninitializedArray<byte>(streamLength, true);
        StoreHandle(GCHandle.Alloc(buffer));

        stream.ReadExactly(buffer);
        byte* bufferPointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(buffer));
        resource = new(bufferPointer, streamLength);
        _resourceMap.AddOrSet(resourcePath, resource);
        AddResource(resource);
        return true;

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowStreamLengthExceedsInt32()
            => throw new InvalidOperationException($"The stream length exceeds the maximum {typeof(int)} value.");

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowStreamLengthExceedsMaxArrayLength()
            => throw new InvalidOperationException($"The stream length exceeds the maximum {typeof(int)} value.");
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void AddResource(Resource resource) => _resources.Add(resource);

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void StoreHandle(GCHandle handle)
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
