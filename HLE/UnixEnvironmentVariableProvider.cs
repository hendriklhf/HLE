using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using HLE.Marshalling;

namespace HLE;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("freebsd")]
public sealed class UnixEnvironmentVariableProvider : IEnvironmentVariableProvider, IEquatable<UnixEnvironmentVariableProvider>
{
    [Pure]
    public unsafe EnvironmentVariables GetEnvironmentVariables()
    {
        Dictionary<string, string> result = new(64);

        nint environment = Interop.Unix.GetEnvironment();
        if (environment == 0)
        {
            return EnvironmentVariables.Empty;
        }

        const byte equalsSign = (byte)'=';

        try
        {
            nint env = environment;
            byte* entryPtr = *(byte**)env;
            while (entryPtr != null)
            {
                ReadOnlySpan<byte> entry = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(entryPtr);
                if (entry.Length < 2)
                {
                    env += sizeof(nint);
                    entryPtr = *(byte**)env;
                    continue;
                }

                int indexOfEquals = entry.IndexOf(equalsSign);
                ReadOnlySpan<byte> key = entry[..indexOfEquals];
                ReadOnlySpan<byte> value = entry[(indexOfEquals + 1)..];

                string keyString = Encoding.UTF8.GetString(key);
                string valueString = Encoding.UTF8.GetString(value);

                result.Add(keyString, valueString);

                env += sizeof(nint);
                entryPtr = *(byte**)env;
            }
        }
        finally
        {
            Interop.Unix.FreeEnvironment(environment);
        }

        return new(result.ToFrozenDictionary());
    }

    [Pure]
    public bool Equals(UnixEnvironmentVariableProvider? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(UnixEnvironmentVariableProvider? left, UnixEnvironmentVariableProvider? right)
        => Equals(left, right);

    public static bool operator !=(UnixEnvironmentVariableProvider? left, UnixEnvironmentVariableProvider? right)
        => !(left == right);
}
