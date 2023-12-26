using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

public sealed partial class RegexPool
{
    private readonly struct Bucket : IEnumerable<Regex>, IEquatable<Bucket>
    {
        private readonly Regex?[] _regexes = new Regex[DefaultBucketCapacity];

        public Bucket()
        {
        }

        public void Clear()
        {
            lock (_regexes)
            {
                Array.Clear(_regexes);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Regex GetOrAdd(string pattern, RegexOptions options, TimeSpan timeout)
        {
            lock (_regexes)
            {
                if (TryGet(pattern, options, timeout, out Regex? regex))
                {
                    return regex;
                }

                regex = new(pattern, options, timeout);
                Add(regex);
                return regex;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Regex GetOrAdd(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout)
        {
            lock (_regexes)
            {
                if (TryGet(pattern, options, timeout, out Regex? regex))
                {
                    return regex;
                }

                regex = new(new(pattern), options, timeout);
                Add(regex);
                return regex;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Regex regex)
        {
            lock (_regexes)
            {
                ref Regex? source = ref MemoryMarshal.GetArrayDataReference(_regexes);
                ref Regex? destination = ref Unsafe.Add(ref source, 1);
                CopyWorker<Regex?>.Copy(ref source, ref destination, (uint)(_regexes.Length - 1));
                source = regex;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout, [MaybeNullWhen(false)] out Regex regex)
        {
            lock (_regexes)
            {
                ref Regex? regexesReference = ref MemoryMarshal.GetArrayDataReference(_regexes);
                int regexesLength = _regexes.Length;
                for (int i = 0; i < regexesLength; i++)
                {
                    Regex? current = Unsafe.Add(ref regexesReference, i);
                    if (current is null)
                    {
                        // a null reference can only be followed by more null references,
                        // so we can exit early because the regex can definitely not be found
                        regex = null;
                        return false;
                    }

                    if (options != current.Options || timeout != current.MatchTimeout || !pattern.SequenceEqual(current.ToString()))
                    {
                        continue;
                    }

                    if (i > 3)
                    {
                        MoveRegexByFourIndices(i);
                    }

                    regex = current;
                    return true;
                }

                regex = null;
                return false;
            }
        }

        /// <summary>
        /// Moves a matching item by four places, so that it can be found faster next time.
        /// </summary>
        /// <param name="indexOfMatchingRegex">The index of the matching regex in <see cref="_regexes"/>.</param>
        private void MoveRegexByFourIndices(int indexOfMatchingRegex)
            => _regexes.MoveItem(indexOfMatchingRegex, indexOfMatchingRegex - 4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Regex regex) => TryGet(regex.ToString(), regex.Options, regex.MatchTimeout, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout) => TryGet(pattern, options, timeout, out _);

        public IEnumerator<Regex> GetEnumerator()
        {
            foreach (Regex? regex in _regexes)
            {
                if (regex is not null)
                {
                    yield return regex;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Equals(Bucket other) => _regexes.Equals(other._regexes);

        public override bool Equals([NotNullWhen(true)] object? obj) => obj is Bucket other && Equals(other);

        public override int GetHashCode() => _regexes.GetHashCode();

        public static bool operator ==(Bucket left, Bucket right) => left.Equals(right);

        public static bool operator !=(Bucket left, Bucket right) => !left.Equals(right);
    }
}
