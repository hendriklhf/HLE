using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using HLE.Collections;

namespace HLE.Resources;

public sealed partial class ResourceReader
{
    public struct Enumerator : IEnumerator<Resource>, IEquatable<Enumerator>
    {
        public readonly Resource Current => _enumerator.Current;

        readonly object IEnumerator.Current => Current;

        private ArrayEnumerator<Resource> _enumerator;

        internal Enumerator(List<Resource> resources) => _enumerator = new(resources);

        public bool MoveNext() => _enumerator.MoveNext();

        void IEnumerator.Reset() => throw new NotSupportedException();

        readonly void IDisposable.Dispose()
        {
        }

        [Pure]
        public readonly bool Equals(Enumerator other) => _enumerator.Equals(other._enumerator);

        [Pure]
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Enumerator enumerator && Equals(enumerator);

        [Pure]
        public override readonly int GetHashCode() => _enumerator.GetHashCode();

        public static bool operator ==(Enumerator left, Enumerator right) => left.Equals(right);

        public static bool operator !=(Enumerator left, Enumerator right) => !(left == right);
    }
}
