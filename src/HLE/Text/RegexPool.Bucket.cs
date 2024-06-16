using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Text;

public sealed partial class RegexPool
{
    private partial struct Bucket : IEquatable<Bucket>
    {
        private Regexes _regexes;
        private readonly object _lock = new();

        public Bucket()
        {
        }

        public void Clear()
        {
            lock (_lock)
            {
                InlineArrayHelpers.AsSpan<Regexes, Regex?>(ref _regexes, Regexes.Length).Clear();
            }
        }

        public Regex GetOrAdd(string pattern, RegexOptions options, TimeSpan timeout)
        {
            lock (_lock)
            {
                if (TryGetWithoutLock(pattern, options, timeout, out Regex? regex))
                {
                    return regex;
                }

                regex = new(pattern, options, timeout);
                AddWithoutLock(regex);
                return regex;
            }
        }

        public Regex GetOrAdd(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout)
        {
            lock (_lock)
            {
                if (TryGetWithoutLock(pattern, options, timeout, out Regex? regex))
                {
                    return regex;
                }

                regex = new(new(pattern), options, timeout);
                AddWithoutLock(regex);
                return regex;
            }
        }

        public void Add(Regex regex)
        {
            lock (_lock)
            {
                AddWithoutLock(regex);
            }
        }

        private void AddWithoutLock(Regex regex)
        {
            ref Regex? source = ref InlineArrayHelpers.GetReference<Regexes, Regex?>(ref _regexes);
            ref Regex? destination = ref Unsafe.Add(ref source, 1);
            SpanHelpers.Memmove(ref destination, ref source, DefaultBucketCapacity - 1);
            source = regex;
        }

        public bool TryGet(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout, [MaybeNullWhen(false)] out Regex regex)
        {
            lock (_lock)
            {
                return TryGetWithoutLock(pattern, options, timeout, out regex);
            }
        }

        private bool TryGetWithoutLock(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout, [MaybeNullWhen(false)] out Regex regex)
        {
            ref Regex? regexesReference = ref InlineArrayHelpers.GetReference<Regexes, Regex?>(ref _regexes);
            for (int i = 0; i < DefaultBucketCapacity; i++)
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

        /// <summary>
        /// Moves a matching item by four places, so that it can be found faster next time.
        /// </summary>
        /// <param name="indexOfMatchingRegex">The index of the matching regex in <see cref="_regexes"/>.</param>
        private void MoveRegexByFourIndices(int indexOfMatchingRegex)
            => InlineArrayHelpers.AsSpan<Regexes, Regex?>(ref _regexes, Regexes.Length).MoveItem(indexOfMatchingRegex, indexOfMatchingRegex - 4);

        public bool Contains(Regex regex) => TryGet(regex.ToString(), regex.Options, regex.MatchTimeout, out _);

        public bool Contains(ReadOnlySpan<char> pattern, RegexOptions options, TimeSpan timeout) => TryGet(pattern, options, timeout, out _);

        public readonly bool Equals(Bucket other) => ReferenceEquals(_lock, other._lock);

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Bucket other && Equals(other);

        public override readonly int GetHashCode() => RuntimeHelpers.GetHashCode(_lock);

        public static bool operator ==(Bucket left, Bucket right) => left.Equals(right);

        public static bool operator !=(Bucket left, Bucket right) => !left.Equals(right);
    }
}
