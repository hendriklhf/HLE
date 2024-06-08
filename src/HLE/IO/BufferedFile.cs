using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;

namespace HLE.IO;

public sealed class BufferedFile : IDisposable, IEquatable<BufferedFile>
{
    private SafeFileHandle? _handle;

    internal BufferedFile(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, FileOptions fileOptions)
        => _handle = File.OpenHandle(path, fileMode, fileAccess, fileShare, fileOptions);

    public void Dispose()
    {
        SafeFileHandle? handle = _handle;
        if (handle is null)
        {
            return;
        }

        handle.Dispose();
        _handle = null;
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] BufferedFile? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(BufferedFile? left, BufferedFile? right) => Equals(left, right);

    public static bool operator !=(BufferedFile? left, BufferedFile? right) => !(left == right);
}
