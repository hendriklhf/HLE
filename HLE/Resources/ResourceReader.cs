using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Memory;
using HLE.Strings;

namespace HLE.Resources;

public sealed class ResourceReader : IEquatable<ResourceReader>
{
    public int Count => _resources.Count;

    private readonly Assembly _assembly;
    private readonly string _assemblyName;
    private readonly Dictionary<string, byte[]?> _resources = new();

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
    public byte[]? Read(ReadOnlySpan<char> resourceName)
    {
        ValueStringBuilder pathBuilder = stackalloc char[1 + _assemblyName.Length + resourceName.Length];
        pathBuilder.Append(_assemblyName);
        pathBuilder.Append('.');
        pathBuilder.Append(resourceName);
        return ReadResourceFromPath(StringPool.Shared.GetOrAdd(pathBuilder.WrittenSpan));
    }

    [Pure]
    public async ValueTask<byte[]?> ReadAsync(string resourceName)
    {
        return await ReadAsync(resourceName.AsMemory());
    }

    [Pure]
    public async ValueTask<byte[]?> ReadAsync(ReadOnlyMemory<char> resourceName)
    {
        using PoolBufferStringBuilder pathBuilder = new(1 + _assemblyName.Length + resourceName.Length);
        pathBuilder.Append(_assemblyName);
        pathBuilder.Append('.');
        pathBuilder.Append(resourceName.Span);
        return await ReadResourceFromPathAsync(StringPool.Shared.GetOrAdd(pathBuilder.WrittenSpan));
    }

    private void ReadAllResources()
    {
        Span<string> resourcePaths = _assembly.GetManifestResourceNames();
        for (int i = 0; i < resourcePaths.Length; i++)
        {
            _ = ReadResourceFromPath(resourcePaths[i]);
        }
    }

    private byte[]? ReadResourceFromPath(string resourcePath)
    {
        ref byte[]? resource = ref CollectionsMarshal.GetValueRefOrAddDefault(_resources, resourcePath, out bool exists);
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

        int streamLength = int.CreateChecked(stream.Length);
        using PoolBufferWriter<byte> bufferWriter = new(streamLength, 1000);
        int bufferWriteSize = streamLength < 1000 ? streamLength : 1000;
        int bytesRead = stream.Read(bufferWriter.GetSpan(bufferWriteSize));
        bufferWriter.Advance(bytesRead);
        while (bytesRead < streamLength && bytesRead > 0)
        {
            bytesRead = stream.Read(bufferWriter.GetSpan(bufferWriteSize));
            bufferWriter.Advance(bytesRead);
        }

        resource = bufferWriter.WrittenSpan.ToArray();
        return resource;
    }

    private async ValueTask<byte[]?> ReadResourceFromPathAsync(string resourcePath)
    {
        if (_resources.TryGetValue(resourcePath, out byte[]? resource))
        {
            return resource;
        }

        await using Stream? stream = _assembly.GetManifestResourceStream(resourcePath);
        if (stream is null)
        {
            _resources.AddOrSet(resourcePath, null);
            return null;
        }

        int streamLength = int.CreateChecked(stream.Length);
        using PoolBufferWriter<byte> bufferWriter = new(streamLength, 1000);
        int sizeHint = streamLength < 1000 ? streamLength : 1000;
        int bytesRead = await stream.ReadAsync(bufferWriter.GetMemory(sizeHint));
        bufferWriter.Advance(bytesRead);
        while (bytesRead < streamLength && bytesRead > 0)
        {
            bytesRead = await stream.ReadAsync(bufferWriter.GetMemory(sizeHint));
            bufferWriter.Advance(bytesRead);
        }

        resource = bufferWriter.WrittenSpan.ToArray();
        return resource;
    }

    public void CopyTo(byte[][] destination, int offset = 0)
    {
        CopyTo(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(destination), offset));
    }

    public void CopyTo(Memory<byte[]> destination)
    {
        CopyTo(ref MemoryMarshal.GetReference(destination.Span));
    }

    public void CopyTo(Span<byte[]> destination)
    {
        CopyTo(ref MemoryMarshal.GetReference(destination));
    }

    public unsafe void CopyTo(ref byte[] destination)
    {
        byte[]?[] resources = _resources.Values.Where(v => v is not null).ToArray();
        ref byte source = ref Unsafe.As<byte[]?, byte>(ref MemoryMarshal.GetArrayDataReference(resources));
        ref byte destinationByte = ref Unsafe.As<byte[], byte>(ref destination);
        Unsafe.CopyBlock(ref destinationByte, ref source, (uint)(resources.Length * sizeof(byte[])));
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
        return MemoryHelper.GetRawDataPointer(this).GetHashCode();
    }
}
