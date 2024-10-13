using System;
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
[JsonConverter(typeof(LazyStringJsonConverter))]
public sealed class LazyString :
    IDisposable,
    IEquatable<LazyString>,
    IReadOnlySpanProvider<char>,
    IReadOnlyMemoryProvider<char>,
    ICopyable<char>,
    IIndexable<char>,
    ICollectionProvider<char>,
    IReadOnlyCollection<char>,
    ICollection<char>
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

    public ReadOnlySpan<char> this[Range range] => new Slicer<char>(ref GetReference(), Length).SliceReadOnlySpan(range);

    public int Length { get; }

    int ICollection<char>.Count => Length;

    bool ICollection<char>.IsReadOnly => true;

    int IReadOnlyCollection<char>.Count => Length;

    int ICountable.Count => Length;

    private char[]? _chars;
    private string? _string;

    public static LazyString Empty { get; } = new();

    private LazyString() : this(string.Empty)
    {
    }

    private LazyString(string str)
    {
        _chars = null;
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
        _chars = chars;
        Length = length;
    }

    [Pure]
    public static LazyString FromString(string str) => str.Length == 0 ? Empty : new(str);

    public void Dispose()
    {
        char[]? chars = _chars;
        if (chars is null)
        {
            return;
        }

        _chars = null;
        ArrayPool<char>.Shared.Return(chars);
    }

    internal void DisposeInterlocked()
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
    internal ref char GetReference()
    {
        char[]? chars = _chars;
        if (chars is not null)
        {
            return ref MemoryMarshal.GetArrayDataReference(chars);
        }

        string? str = _string;
        if (str is not null)
        {
            return ref StringMarshal.GetReference(str);
        }

        ThrowHelper.ThrowUnreachableException();
        return ref Unsafe.NullRef<char>();
    }

    [Pure]
    public ReadOnlySpan<char> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref GetReference(), Length);

    [Pure]
    public ReadOnlyMemory<char> AsMemory()
    {
        if (Length == 0)
        {
            return ReadOnlyMemory<char>.Empty;
        }

        char[]? chars = _chars;
        if (chars is not null)
        {
            return chars.AsMemory();
        }

        string? str = _string;
        if (str is not null)
        {
            return str.AsMemory();
        }

        ThrowHelper.ThrowUnreachableException();
        return ReadOnlyMemory<char>.Empty;
    }

    [Pure]
    public char[] ToArray()
    {
        if (Length == 0)
        {
            return [];
        }

        ref char chars = ref GetReference();
        char[] result = GC.AllocateUninitializedArray<char>(Length);
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(result), ref chars, (uint)Length);
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
        return _string ?? ToStringCore(this, poolString);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static string ToStringCore(LazyString lazyString, bool poolString)
        {
            if (lazyString.Length == 0)
            {
                return string.Empty;
            }

            char[]? charBuffer = lazyString._chars;
            lazyString._chars = null;

            Debug.Assert(charBuffer is not null);

            ReadOnlySpan<char> chars = charBuffer.AsSpanUnsafe(..lazyString.Length);
            string str = poolString ? StringPool.Shared.GetOrAdd(chars) : new(chars);
            ArrayPool<char>.Shared.Return(charBuffer);

            lazyString._string = str;
            return str;
        }
    }

    public void CopyTo(List<char> destination, int offset = 0)
    {
        CopyWorker<char> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(char[] destination, int offset = 0)
    {
        CopyWorker<char> copyWorker = new(ref GetReference(), (uint)Length);
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<char> destination)
    {
        CopyWorker<char> copyWorker = new(ref GetReference(), (uint)Length);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(Span<char> destination)
    {
        CopyWorker<char> copyWorker = new(ref GetReference(), (uint)Length);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(ref char destination)
    {
        CopyWorker<char> copyWorker = new(ref GetReference(), (uint)Length);
        copyWorker.CopyTo(ref destination);
    }

    public unsafe void CopyTo(char* destination)
    {
        CopyWorker<char> copyWorker = new(ref GetReference(), (uint)Length);
        copyWorker.CopyTo(destination);
    }

    void ICollection<char>.Add(char item) => throw new NotSupportedException();

    void ICollection<char>.Clear() => throw new NotSupportedException();

    [Pure]
    public bool Contains(char item) => AsSpan().Contains(item);

    bool ICollection<char>.Remove(char item) => throw new NotSupportedException();

    ReadOnlySpan<char> IReadOnlySpanProvider<char>.GetReadOnlySpan() => AsSpan();

    ReadOnlyMemory<char> IReadOnlyMemoryProvider<char>.GetReadOnlyMemory() => AsMemory();

    [Pure]
    public MemoryEnumerator<char> GetEnumerator() => new(AsSpan());

    IEnumerator<char> IEnumerable<char>.GetEnumerator()
    {
        if (Length == 0)
        {
            return EmptyEnumeratorCache<char>.Enumerator;
        }

        string? str = _string;
        if (str is not null)
        {
            return new StringEnumerator(str);
        }

        char[]? chars = _chars;
        if (chars is not null)
        {
            return new ArrayEnumerator<char>(chars, 0, Length);
        }

        ThrowHelper.ThrowUnreachableException();
        return null!;
    }

    // ReSharper disable once NotDisposedResourceIsReturned
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<char>)this).GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] LazyString? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is LazyString other && Equals(other);

    [Pure]
    public override int GetHashCode() => string.GetHashCode(AsSpan(), StringComparison.Ordinal);

    public static bool operator ==(LazyString left, LazyString right) => Equals(left, right);

    public static bool operator !=(LazyString left, LazyString right) => !(left == right);
}
