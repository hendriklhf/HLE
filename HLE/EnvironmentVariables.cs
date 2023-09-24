using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Strings;

namespace HLE;

public sealed partial class EnvironmentVariables : IReadOnlyDictionary<string, string>, ICountable, IEquatable<EnvironmentVariables>
{
    public string? this[string name]
    {
        get
        {
            _environmentVariables.TryGetValue(name, out string? value);
            return value;
        }
    }

    string IReadOnlyDictionary<string, string>.this[string key] => _environmentVariables[key];

    public ImmutableArray<string> Names => _environmentVariables.Keys;

    public ImmutableArray<string> Values => _environmentVariables.Values;

    IEnumerable<string> IReadOnlyDictionary<string, string>.Keys => Names;

    IEnumerable<string> IReadOnlyDictionary<string, string>.Values => Values;

    public int Count => _environmentVariables.Count;

    private readonly FrozenDictionary<string, string> _environmentVariables;

    private EnvironmentVariables(FrozenDictionary<string, string> environmentVariables)
    {
        _environmentVariables = environmentVariables;
    }

    bool IReadOnlyDictionary<string, string>.ContainsKey(string key)
    {
        return _environmentVariables.ContainsKey(key);
    }

    bool IReadOnlyDictionary<string, string>.TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        return _environmentVariables.TryGetValue(key, out value);
    }

    public static unsafe EnvironmentVariables Create()
    {
        Dictionary<string, string> environmentVariables = new(64);
        char* environmentStrings = GetEnvironmentStrings();
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
            _ = FreeEnvironmentStrings(environmentStrings);
        }

        return new(environmentVariables.ToFrozenDictionary());
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return _environmentVariables.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Equals(EnvironmentVariables? other)
    {
        return _environmentVariables.Equals(other?._environmentVariables);
    }

    public override bool Equals(object? obj)
    {
        return obj is EnvironmentVariables other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _environmentVariables.GetHashCode();
    }

    public static bool operator ==(EnvironmentVariables? left, EnvironmentVariables? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(EnvironmentVariables? left, EnvironmentVariables? right)
    {
        return !(left == right);
    }

    [LibraryImport("kernel32.dll", EntryPoint = "GetEnvironmentStringsW")]
    private static unsafe partial char* GetEnvironmentStrings();

    [return: MarshalAs(UnmanagedType.Bool)]
    [LibraryImport("kernel32.dll", EntryPoint = "FreeEnvironmentStringsW")]
    private static unsafe partial bool FreeEnvironmentStrings(char* ptr);
}
