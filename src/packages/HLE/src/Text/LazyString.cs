using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Text;

[DebuggerDisplay("{AsSpan()}")]
[JsonConverter(typeof(JsonConverter))]
public sealed partial class LazyString :
    IDisposable,
    IEquatable<LazyString>,
    IReadOnlySpanProvider<char>,
    IReadOnlyMemoryProvider<char>,
    ICopyable<char>,
    IIndexable<char>,
    ICollectionProvider<char>,
    IReadOnlyCollection<char>,
    ICollection<char>,
    ISpanFormattable
{
    public ref readonly char this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return ref Unsafe.Add(ref GetReference(), index);
        }
    }

    char IIndexable<char>.this[int index] => this[index];

    char IIndexable<char>.this[Index index] => this[index];

    public ref readonly char this[Index index] => ref this[index.GetOffset(Length)];

    public ReadOnlySpan<char> this[Range range] => Slicer.SliceReadOnly(ref GetReference(), Length, range);

    public int Length { get; }

    int ICollection<char>.Count => Length;

    bool ICollection<char>.IsReadOnly => true;

    int IReadOnlyCollection<char>.Count => Length;

    int ICountable.Count => Length;

    private char[]? _chars;

    [SuppressMessage("Major Code Smell", "S2933:Fields that are only assigned in the constructor should be \"readonly\"",
        Justification = "analyzer is wrong. a mutable ref has been taken of the field.")]
    private string? _string;

    public static LazyString Empty { get; } = new();

    private LazyString() : this(string.Empty)
    {
    }

    private LazyString(string str)
    {
        _string = str;
        Length = str.Length;
    }

    public LazyString(ref PooledInterpolatedStringHandler chars) : this(chars.Text)
        => chars.Dispose();

    public LazyString(ReadOnlySpan<char> chars)
    {
        if (chars.Length == 0)
        {
            _chars = [];
            _string = string.Empty;
            return;
        }

        char[] buffer = ArrayPool<char>.Shared.Rent(chars.Length);
        SpanHelpers.Copy(chars, buffer);
        _chars = buffer;
        Length = chars.Length;
    }

    internal LazyString(char[] chars, int length)
    {
        if (length == 0)
        {
            _string = string.Empty;
            ArrayPool<char>.Shared.Return(chars);
            return;
        }

        _chars = chars;
        Length = length;
    }

    [Pure]
    public static LazyString FromString(string str) => str.Length == 0 ? Empty : new(str);

    public void Dispose()
    {
        char[]? chars = Interlocked.Exchange(ref _chars, null);
        if (chars is null)
        {
            return;
        }

        ArrayPool<char>.Shared.Return(chars);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref char GetReference()
    {
        char[]? chars = _chars;
        if (chars is not null)
        {
            return ref MemoryMarshal.GetArrayDataReference(chars);
        }

        string str = AwaitStringCreation(ref _string);
        return ref StringMarshal.GetReference(str);
    }

    [Pure]
    public ReadOnlySpan<char> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref GetReference(), Length);

    [Pure]
    public ReadOnlySpan<char> AsSpan(int start) => Slicer.SliceReadOnly(ref GetReference(), Length, start);

    [Pure]
    public ReadOnlySpan<char> AsSpan(int start, int length) => Slicer.SliceReadOnly(ref GetReference(), Length, start, length);

    [Pure]
    public ReadOnlySpan<char> AsSpan(Range range) => Slicer.SliceReadOnly(ref GetReference(), Length, range);

    [Pure]
    public ReadOnlyMemory<char> AsMemory()
    {
        if (Length == 0)
        {
            return ReadOnlyMemory<char>.Empty;
        }

        return _chars?.AsMemory(0, Length) ?? AwaitStringCreation(ref _string).AsMemory();
    }

    [Pure]
    public ReadOnlyMemory<char> AsMemory(int start) => AsMemory()[start..];

    [Pure]
    public ReadOnlyMemory<char> AsMemory(int start, int length) => AsMemory().Slice(start, length);

    [Pure]
    public ReadOnlyMemory<char> AsMemory(Range range) => AsMemory()[range];

    [Pure]
    public char[] ToArray()
    {
        if (Length == 0)
        {
            return [];
        }

        ref char chars = ref GetReference();
        char[] result = GC.AllocateUninitializedArray<char>(Length);
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(result), ref chars, Length);
        return result;
    }

    [Pure]
    public char[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public char[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public char[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public List<char> ToList() => AsSpan().ToList();

    [Pure]
    public List<char> ToList(int start) => AsSpan().ToList(start);

    [Pure]
    public List<char> ToList(int start, int length) => AsSpan().ToList(start, length);

    [Pure]
    public List<char> ToList(Range range) => AsSpan().ToList(range);

    public bool TryGetString([MaybeNullWhen(false)] out string str)
    {
        str = _string;
        return str is not null;
    }

    [Pure]
    public override string ToString() => ToString(false);

    [Pure]
    public string ToString(bool poolString)
    {
        return _string ?? ToStringSlow(this, poolString);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static string ToStringSlow(LazyString lazyString, bool poolString)
        {
            if (lazyString.Length == 0)
            {
                lazyString._string = string.Empty;
                return string.Empty;
            }

            ref string? str = ref lazyString._string;
            char[]? charBuffer = Interlocked.Exchange(ref lazyString._chars, null);
            if (charBuffer is null)
            {
                // "charBuffer" being null means that "ToStringSlow" has been called
                // approximately at the same time from another thread.
                // The other thread should have already set the "_string" field in LazyString or
                // will in a short time, so we wait until it will be set.

                return AwaitStringCreation(ref str);
            }

            ReadOnlySpan<char> chars = charBuffer.AsSpanUnsafe(..lazyString.Length);
            str = poolString ? StringPool.Shared.GetOrAdd(chars) : new(chars);
            ArrayPool<char>.Shared.Return(charBuffer);
            return str;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string AwaitStringCreation(ref string? str)
    {
        return Volatile.Read(ref str) ?? AwaitStringCreationCore(ref str);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static string AwaitStringCreationCore(ref string? reference)
        {
            const int MaximumSpinCount = 512;

            SpinWait spinWait = new();
            do
            {
                string? str = Volatile.Read(ref reference);
                if (str is not null)
                {
                    return str;
                }

                spinWait.SpinOnce();
            }
            while (spinWait.Count < MaximumSpinCount);

            // if we spun for too long and expected the string to be set,
            // but instead it has not been set by another thread,
            // we throw an exception, as the only possibility is
            // that the LazyString has been disposed.

            ThrowHelper.ThrowObjectDisposedException<LazyString>();
            return null!;
        }
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (destination.Length < Length)
        {
            charsWritten = 0;
            return false;
        }

        SpanHelpers.Memmove(ref MemoryMarshal.GetReference(destination), ref GetReference(), Length);
        charsWritten = Length;
        return true;
    }

    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString();

    public void CopyTo(List<char> destination, int offset = 0)
        => SpanHelpers.CopyChecked(AsSpan(), destination, offset);

    public void CopyTo(char[] destination, int offset = 0)
        => SpanHelpers.CopyChecked(AsSpan(), destination.AsSpan(offset..));

    public void CopyTo(Memory<char> destination) => SpanHelpers.CopyChecked(AsSpan(), destination.Span);

    public void CopyTo(Span<char> destination) => SpanHelpers.CopyChecked(AsSpan(), destination);

    public void CopyTo(ref char destination) => SpanHelpers.Copy(AsSpan(), ref destination);

    public unsafe void CopyTo(char* destination) => SpanHelpers.Copy(AsSpan(), destination);

    void ICollection<char>.Add(char item) => throw new NotSupportedException();

    void ICollection<char>.Clear() => throw new NotSupportedException();

    bool ICollection<char>.Remove(char item) => throw new NotSupportedException();

    [Pure]
    public int IndexOf(char item) => AsSpan().IndexOf(item);

    [Pure]
    public bool Contains(char item) => AsSpan().Contains(item);

    [Pure]
    public ReadOnlyMemoryEnumerator<char> GetEnumerator() => new(AsSpan());

    IEnumerator<char> IEnumerable<char>.GetEnumerator()
    {
        if (Length == 0)
        {
            return EmptyEnumeratorCache<char>.Enumerator;
        }

        char[]? chars = _chars;
        if (chars is not null)
        {
            return new ArrayEnumerator<char>(chars, 0, Length);
        }

        string str = AwaitStringCreation(ref _string);
        return new StringEnumerator(str);
    }

    // ReSharper disable once NotDisposedResourceIsReturned
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<char>)this).GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] LazyString? other) => Equals(other, StringComparison.Ordinal);

    [Pure]
    public bool Equals([NotNullWhen(true)] LazyString? other, StringComparison comparisonType)
        => ReferenceEquals(this, other) || (other is not null && Equals(other.AsSpan(), comparisonType));

    [Pure]
    public bool Equals([NotNullWhen(true)] string? other)
        => Equals(other.AsSpan(), StringComparison.Ordinal);

    [Pure]
    public bool Equals([NotNullWhen(true)] string? other, StringComparison comparisonType)
        => other is not null && Equals(other.AsSpan(), comparisonType);

    private bool Equals(ReadOnlySpan<char> chars, StringComparison comparisonType)
        => AsSpan().Equals(chars, comparisonType);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj switch
    {
        LazyString lazyString => Equals(lazyString, StringComparison.Ordinal),
        string str => Equals(str, StringComparison.Ordinal),
        _ => false
    };

    [Pure]
    public override int GetHashCode() => string.GetHashCode(AsSpan(), StringComparison.Ordinal);

    [Pure]
    public int GetHashCode(StringComparison comparisonType) => string.GetHashCode(AsSpan(), comparisonType);

    public static bool operator ==(LazyString? left, LazyString? right) => Equals(left, right);

    public static bool operator !=(LazyString? left, LazyString? right) => !(left == right);

    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    public static bool operator ==(LazyString? left, string? right) => Equals(left, right);

    public static bool operator !=(LazyString? left, string? right) => !(left == right);
}
