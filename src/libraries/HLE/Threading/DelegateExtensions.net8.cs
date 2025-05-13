using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using HLE.Collections;

namespace HLE.Threading;

internal static class DelegateExtensions
{
    extension(Delegate)
    {
        public static InvocationListEnumerator<TDelegate> EnumerateInvocationList<TDelegate>(TDelegate d) where TDelegate : Delegate?
        {
            if (d is null)
            {
                return InvocationListEnumerator<TDelegate>.Empty;
            }

            Delegate[] targets = d.GetInvocationList();
            return new(Unsafe.As<TDelegate[]>(targets));
        }
    }

    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    public struct InvocationListEnumerator<TDelegate>(TDelegate[] targets) :
        IEnumerator<TDelegate>
        where TDelegate : Delegate?
    {
        public readonly TDelegate Current => _enumerator.Current;

        readonly object? IEnumerator.Current => Current;

        private ArrayEnumerator<TDelegate> _enumerator = new(targets);

        public static InvocationListEnumerator<TDelegate> Empty => new([]);

        public readonly InvocationListEnumerator<TDelegate> GetEnumerator() => this;

        public bool MoveNext() => _enumerator.MoveNext();

        [DoesNotReturn]
        void IEnumerator.Reset() => throw new NotSupportedException();

        readonly void IDisposable.Dispose()
        {
        }
    }
}
