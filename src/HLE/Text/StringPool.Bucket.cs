using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Text;

public sealed partial class StringPool
{
    private partial struct Bucket : IEquatable<Bucket>
    {
        private Strings _strings;
        private readonly Lock _lock = new();

        private const int MoveItemThreshold = 4;

        public Bucket()
        {
        }

        public void Clear()
        {
            lock (_lock)
            {
                InlineArrayHelpers.AsSpan<Strings, string?>(ref _strings).Clear();
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
            ref string? stringsReference = ref InlineArrayHelpers.GetReference<Strings, string?>(ref _strings);
            SpanHelpers.Memmove(ref Unsafe.Add(ref stringsReference, 1), ref stringsReference, Strings.Length - 1);
            stringsReference = value;
        }

        private bool TryGetWithoutLock(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
        {
            Span<string?> strings = InlineArrayHelpers.AsSpan<Strings, string?>(ref _strings);
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

        public readonly bool Equals(Bucket other) => _lock == other._lock;

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Bucket other && Equals(other);

        public override readonly int GetHashCode() => _lock.GetHashCode();

        public static bool operator ==(Bucket left, Bucket right) => left.Equals(right);

        public static bool operator !=(Bucket left, Bucket right) => !left.Equals(right);
    }
}
