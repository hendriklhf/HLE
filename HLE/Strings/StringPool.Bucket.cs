using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

public sealed partial class StringPool
{
    private readonly struct Bucket(int bucketCapacity = _defaultBucketCapacity)
        : IEnumerable<string>, IEquatable<Bucket>
    {
        internal readonly string?[] _strings = new string[bucketCapacity];

        private const int _moveItemThreshold = 6;

        public void Clear()
        {
            Monitor.Enter(_strings);
            try
            {
                Array.Clear(_strings);
            }
            finally
            {
                Monitor.Exit(_strings);
            }
        }

        public string GetOrAdd(ReadOnlySpan<char> span)
        {
            Monitor.Enter(_strings);
            try
            {
                if (TryGet(span, out string? value))
                {
                    return value;
                }

                value = new(span);
                Add(value);
                return value;
            }
            finally
            {
                Monitor.Exit(_strings);
            }
        }

        public void Add(string value)
        {
            Monitor.Enter(_strings);
            try
            {
                ref string? stringsReference = ref MemoryMarshal.GetArrayDataReference(_strings);
                CopyWorker<string?>.Copy(ref stringsReference, ref Unsafe.Add(ref stringsReference, 1), (uint)(_strings.Length - 1));
                stringsReference = value;
            }
            finally
            {
                Monitor.Exit(_strings);
            }
        }

        public bool TryGet(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
        {
            Monitor.Enter(_strings);
            try
            {
                Span<string?> strings = _strings;
                for (int i = 0; i < strings.Length; i++)
                {
                    string? str = strings[i];
                    if (str is null)
                    {
                        value = null;
                        return false;
                    }

                    if (!span.SequenceEqual(str))
                    {
                        continue;
                    }

                    if (i > _moveItemThreshold)
                    {
                        strings.MoveItem(i, i - _moveItemThreshold - 1);
                    }

                    value = str;
                    return true;
                }

                value = null;
                return false;
            }
            finally
            {
                Monitor.Exit(_strings);
            }
        }

        public bool Contains(ReadOnlySpan<char> span) => TryGet(span, out _);

        public IEnumerator<string> GetEnumerator()
        {
            foreach (string? str in _strings)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (str is not null)
                {
                    yield return str;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Equals(Bucket other) => _strings.Equals(other._strings);

        public override bool Equals(object? obj) => obj is Bucket other && Equals(other);

        public override int GetHashCode() => _strings.GetHashCode();

        public static bool operator ==(Bucket left, Bucket right) => left.Equals(right);

        public static bool operator !=(Bucket left, Bucket right) => !left.Equals(right);
    }
}
