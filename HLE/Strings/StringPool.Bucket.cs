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
    private readonly struct Bucket(int bucketCapacity = DefaultBucketCapacity)
        : IEnumerable<string>, IEquatable<Bucket>
    {
        internal readonly string?[] _strings = new string[bucketCapacity];

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
                if (TryGet(span, out string? value))
                {
                    return value;
                }

                value = new(span);
                Add(value);
                return value;
            }
        }

        public void Add(string value)
        {
            lock (_strings)
            {
                ref string? stringsReference = ref MemoryMarshal.GetArrayDataReference(_strings);
                CopyWorker<string?>.Copy(ref stringsReference, ref Unsafe.Add(ref stringsReference, 1), (uint)(_strings.Length - 1));
                stringsReference = value;
            }
        }

        public bool TryGet(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
        {
            lock (_strings)
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

        public override bool Equals([NotNullWhen(true)] object? obj) => obj is Bucket other && Equals(other);

        public override int GetHashCode() => _strings.GetHashCode();

        public static bool operator ==(Bucket left, Bucket right) => left.Equals(right);

        public static bool operator !=(Bucket left, Bucket right) => !left.Equals(right);
    }
}
