using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

public sealed partial class StringPool
{
    private partial struct Bucket : IEquatable<Bucket>
    {
        private Strings _strings;
        private readonly object _lock = new();

        private const int MoveItemThreshold = 4;

        public Bucket()
        {
        }

        public void Clear()
        {
            lock (_lock)
            {
                _strings.AsSpan().Clear();
            }
        }

        public string GetOrAdd(ReadOnlySpan<char> span)
        {
            lock (_lock)
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
            lock (_lock)
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
            lock (_lock)
            {
                AddWithoutLock(value);
            }
        }

        public bool TryGet(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
        {
            lock (_lock)
            {
                return TryGetWithoutLock(span, out value);
            }
        }

        public bool Contains(ReadOnlySpan<char> span) => TryGet(span, out _);

        private void AddWithoutLock(string value)
        {
            ref string? stringsReference = ref _strings.Reference;
            SpanHelpers<string?>.Memmove(ref Unsafe.Add(ref stringsReference, 1), ref stringsReference, DefaultBucketCapacity - 1);
            stringsReference = value;
        }

        private bool TryGetWithoutLock(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
        {
            Span<string?> strings = _strings.AsSpan();
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

        // ReSharper disable once InconsistentlySynchronizedField
        public readonly bool Equals(Bucket other) => ReferenceEquals(_lock, other._lock);

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Bucket other && Equals(other);

        // ReSharper disable once InconsistentlySynchronizedField
        public override readonly int GetHashCode() => RuntimeHelpers.GetHashCode(_lock);

        public static bool operator ==(Bucket left, Bucket right) => left.Equals(right);

        public static bool operator !=(Bucket left, Bucket right) => !left.Equals(right);
    }
}
