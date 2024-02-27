using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

public sealed partial class StringPool
{
    private readonly struct Bucket(int bucketCapacity = DefaultBucketCapacity) :
        IEnumerable<string>,
        IEquatable<Bucket>
    {
        internal readonly string?[] _strings = GC.AllocateArray<string>(bucketCapacity, true);

        private const int MoveItemThreshold = 6;

        public void Clear()
        {
            lock (_strings)
            {
                Array.Clear(_strings);
            }
        }

        public string GetOrAdd(ReadOnlySpan<char> span)
        {
            lock (_strings)
            {
                if (TryGetWithoutLock(span, out string? value))
                {
                    return value;
                }

                value = new(span);
                AddWithoutLock(value);
                return value;
            }
        }

        public string GetOrAdd(string str)
        {
            lock (_strings)
            {
                if (TryGetWithoutLock(str, out _))
                {
                    return str;
                }

                AddWithoutLock(str);
                return str;
            }
        }

        public void Add(string value)
        {
            lock (_strings)
            {
                AddWithoutLock(value);
            }
        }

        public bool TryGet(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
        {
            lock (_strings)
            {
                return TryGetWithoutLock(span, out value);
            }
        }

        public bool Contains(ReadOnlySpan<char> span) => TryGet(span, out _);

        private void AddWithoutLock(string value)
        {
            ref string? stringsReference = ref MemoryMarshal.GetArrayDataReference(_strings);
            SpanHelpers<string?>.Memmove(ref Unsafe.Add(ref stringsReference, 1), ref stringsReference, (uint)(_strings.Length - 1));
            stringsReference = value;
        }

        private bool TryGetWithoutLock(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
        {
            Span<string?> strings = _strings;
            for (int i = 0; i < strings.Length; i++)
            {
                string? str = strings[i];
                if (str is null)
                {
                    // a null reference can only be followed by more null references,
                    // so we can exit early because the string can definitely not be found
                    value = null;
                    return false;
                }

                if (!span.SequenceEqual(str))
                {
                    continue;
                }

                if (i > MoveItemThreshold)
                {
                    strings.MoveItem(i, i - MoveItemThreshold - 1);
                }

                value = str;
                return true;
            }

            value = null;
            return false;
        }

        public IEnumerator<string> GetEnumerator()
        {
            // ReSharper disable once InconsistentlySynchronizedField
            foreach (string? str in _strings)
            {
                if (str is null)
                {
                    break;
                }

                yield return str;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // ReSharper disable once InconsistentlySynchronizedField
        public bool Equals(Bucket other) => _strings.Equals(other._strings);

        public override bool Equals([NotNullWhen(true)] object? obj) => obj is Bucket other && Equals(other);

        // ReSharper disable once InconsistentlySynchronizedField
        public override int GetHashCode() => _strings.GetHashCode();

        public static bool operator ==(Bucket left, Bucket right) => left.Equals(right);

        public static bool operator !=(Bucket left, Bucket right) => !left.Equals(right);
    }
}
