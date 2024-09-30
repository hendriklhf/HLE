using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using HLE.Marshalling.Unix;
using HLE.Text;

namespace HLE;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
internal sealed unsafe partial class UnixEnvironmentVariableProvider : IEnvironmentVariableProvider, IEquatable<UnixEnvironmentVariableProvider>
{
    [Pure]
    public EnvironmentVariables GetEnvironmentVariables()
    {
        Variable* environment = (Variable*)Interop.GetEnvironment();
        try
        {
            if (environment == null)
            {
                return EnvironmentVariables.Empty;
            }

            const byte EqualsSign = (byte)'=';

            Dictionary<string, string> result = new(64);
            Variable* variables = environment;
            while (variables->Value != null)
            {
                ReadOnlySpan<byte> entry = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(variables->Value);
                if (entry.Length < 2)
                {
                    variables++;
                    continue;
                }

                int indexOfEquals = entry.IndexOf(EqualsSign);
                ReadOnlySpan<byte> key = entry[..indexOfEquals];
                ReadOnlySpan<byte> value = entry[(indexOfEquals + 1)..];

                string keyString = StringPool.Shared.GetOrAdd(key, Encoding.UTF8);
                string valueString = StringPool.Shared.GetOrAdd(value, Encoding.UTF8);

                result.Add(keyString, valueString);

                variables++;
            }

            return new(result.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase));
        }
        finally
        {
            Interop.FreeEnvironment(environment);
        }
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] UnixEnvironmentVariableProvider? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(UnixEnvironmentVariableProvider? left, UnixEnvironmentVariableProvider? right)
        => Equals(left, right);

    public static bool operator !=(UnixEnvironmentVariableProvider? left, UnixEnvironmentVariableProvider? right)
        => !(left == right);
}
