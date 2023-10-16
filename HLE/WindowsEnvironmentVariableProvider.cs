using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Strings;

namespace HLE;

public sealed class WindowsEnvironmentVariableProvider : IEnvironmentVariableProvider, IEquatable<WindowsEnvironmentVariableProvider>
{
    [Pure]
    public unsafe EnvironmentVariables GetEnvironmentVariables()
    {
        Dictionary<string, string> environmentVariables = new(64);
        char* environmentStrings = Interop.Windows.GetEnvironmentStrings();
        try
        {
            char* str = environmentStrings;
            while (true)
            {
                ReadOnlySpan<char> variable = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(str);
                if (variable.Length == 0)
                {
                    break;
                }

                int indexOfEquals = variable.IndexOf('=');
                string key = StringPool.Shared.GetOrAdd(variable[..indexOfEquals]);
                string value = StringPool.Shared.GetOrAdd(variable[(indexOfEquals + 1)..]);
                str += variable.Length + 1;

                environmentVariables.Add(key, value);
            }
        }
        finally
        {
            _ = Interop.Windows.FreeEnvironmentStrings(environmentStrings);
        }

        return new(environmentVariables.ToFrozenDictionary());
    }

    [Pure]
    public bool Equals(WindowsEnvironmentVariableProvider? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(WindowsEnvironmentVariableProvider? left, WindowsEnvironmentVariableProvider? right)
        => Equals(left, right);

    public static bool operator !=(WindowsEnvironmentVariableProvider? left, WindowsEnvironmentVariableProvider? right)
        => !(left == right);
}
