using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace HLE;

public readonly partial struct EnvironmentVariables
{
    public struct Enumerator(EnvironmentVariables environmentVariables) : IEnumerator<EnvironmentVariable>, IEquatable<Enumerator>
    {
        public readonly EnvironmentVariable Current => new(_names[_current], _values[_current]);

        readonly object IEnumerator.Current => Current;

        private readonly string[] _names = ImmutableCollectionsMarshal.AsArray(environmentVariables._environmentVariables.Keys) ?? [];
        private readonly string[] _values = ImmutableCollectionsMarshal.AsArray(environmentVariables._environmentVariables.Values) ?? [];
        private int _current = -1;

        // ReSharper disable once MemberHidesStaticFromOuterClass
        [SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members")]
        public static Enumerator Empty => new(EnvironmentVariables.Empty);

        public bool MoveNext() => ++_current < _names.Length;

        readonly void IEnumerator.Reset() => throw new NotSupportedException();

        readonly void IDisposable.Dispose()
        {
        }

        [Pure]
        public readonly bool Equals(Enumerator other) => ReferenceEquals(_names, other._names) && ReferenceEquals(_values, other._values) && _current == other._current;

        [Pure]
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Enumerator other && Equals(other);

        [Pure]
        public override readonly int GetHashCode() => HashCode.Combine(_names, _values, _current);

        public static bool operator ==(Enumerator left, Enumerator right) => left.Equals(right);

        public static bool operator !=(Enumerator left, Enumerator right) => !(left == right);
    }
}
