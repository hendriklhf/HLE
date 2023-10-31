using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using HLE.Collections;

namespace HLE.Strings;

public sealed partial class StringPool
{
    private readonly struct Bucket(int bucketCapacity = _defaultBucketCapacity) : IEnumerable<string>
    {
        internal readonly StringArray _strings = new(bucketCapacity);

        public void Clear()
        {
            Monitor.Enter(_strings);
            try
            {
                _strings.Clear();
            }
            finally
            {
                Monitor.Exit(_strings);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string value)
        {
            Monitor.Enter(_strings);
            try
            {
                _strings[^1] = value;
                _strings.MoveString(_strings.Length - 1, 0);
            }
            finally
            {
                Monitor.Exit(_strings);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out string value)
        {
            Monitor.Enter(_strings);
            try
            {
                int index = IndexOf(_strings, span);
                if (index < 0)
                {
                    value = null;
                    return false;
                }

                value = _strings[index];
                return true;
            }
            finally
            {
                Monitor.Exit(_strings);
            }
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IndexOf(StringArray stringArray, ReadOnlySpan<char> span)
        {
            int arrayLength = stringArray.Length;
            ReadOnlySpan<char> stringChars = stringArray._chars;
            ref string stringsReference = ref MemoryMarshal.GetArrayDataReference(stringArray._strings);
            ref int lengthsReference = ref MemoryMarshal.GetArrayDataReference(stringArray._lengths);
            ref int startReference = ref MemoryMarshal.GetArrayDataReference(stringArray._starts);
            for (int i = 0; i < arrayLength; i++)
            {
                int length = Unsafe.Add(ref lengthsReference, i);
                if (length == 0)
                {
                    return -1;
                }

                if (length != span.Length)
                {
                    continue;
                }

                ref char spanReference = ref MemoryMarshal.GetReference(span);
                ref char stringReference = ref MemoryMarshal.GetReference(Unsafe.Add(ref stringsReference, i).AsSpan());
                if (Unsafe.AreSame(ref spanReference, ref stringReference))
                {
                    return i;
                }

                int start = Unsafe.Add(ref startReference, i);
                ReadOnlySpan<char> bufferString = stringChars.SliceUnsafe(start, length);
                if (span.SequenceEqual(bufferString))
                {
                    return i;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    }
}
