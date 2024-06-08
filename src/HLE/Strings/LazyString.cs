using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Strings;

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

    private LazyString()
    {
        _chars = [];
        _string = string.Empty;
    }

    private LazyString(string str)
    {
        _chars = null;
        _string = str;
    }

    public LazyString(ref PooledInterpolatedStringHandler chars) : this(chars.Text)
        => chars.Dispose();

    [MustDisposeResource]
    public LazyString(ReadOnlySpan<char> chars)
    {
        if (chars.Length == 0)
        {
            _chars = [];
            _string = string.Empty;
            return;
        }

        char[] buffer = ArrayPool<char>.Shared.Rent(chars.Length);
        SpanHelpers<char>.Copy(chars, buffer);
        _chars = buffer;
        Length = chars.Length;
    }

    [MustDisposeResource]
    internal LazyString([HandlesResourceDisposal] RentedArray<char> chars, int length)
    {
        Debug.Assert(chars._pool == ArrayPool<char>.Shared);

        _chars = chars.Array;
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

        ArrayPool<char>.Shared.Return(chars);
        _chars = null;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref char GetReference()
    {
        if (_chars is not null)
        {
            return ref MemoryMarshal.GetArrayDataReference(_chars);
        }

        if (_string is not null)
        {
            return ref StringMarshal.GetReference(_string);
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

        if (_chars is not null)
        {
            return _chars.AsMemory();
        }

        if (_string is not null)
        {
            return _string.AsMemory();
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
        SpanHelpers<char>.Memmove(ref MemoryMarshal.GetArrayDataReference(result), ref chars, (uint)Length);
        return result;
    }

    [Pure]
    public char[] ToArray(int start) => AsSpan().ToArray(start);

    [Pure]
    public char[] ToArray(int start, int length) => AsSpan().ToArray(start, length);

    [Pure]
    public char[] ToArray(Range range) => AsSpan().ToArray(range);

    [Pure]
    public List<char> ToList()
    {
        if (Length == 0)
        {
            return [];
        }

        List<char> result = new(Length);
        CopyWorker<char> copyWorker = new(ref GetReference(), (uint)Length);
        copyWorker.CopyTo(result);
        return result;
    }

    [Pure]
    public List<char> ToList(int start) => AsSpan().ToList(start);

    [Pure]
    public List<char> ToList(int start, int length) => AsSpan().ToList(start, length);

    [Pure]
    public List<char> ToList(Range range) => AsSpan().ToList(range);

    [Pure]
    public override string ToString() => ToString(false);

    [Pure]
    public string ToString(bool poolString)
    {
        if (_string is not null)
        {
            return _string;
        }

        if (Length == 0)
        {
            return string.Empty;
        }

        char[]? charBuffer = _chars;
        _chars = null;

        Debug.Assert(charBuffer is not null);

        ReadOnlySpan<char> chars = charBuffer.AsSpanUnsafe(..Length);
        string str = poolString ? StringPool.Shared.GetOrAdd(chars) : new(chars);
        ArrayPool<char>.Shared.Return(charBuffer);

        _string = str;
        return str;
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

    public bool Contains(char item) => AsSpan().Contains(item);

    bool ICollection<char>.Remove(char item) => throw new NotSupportedException();

    ReadOnlySpan<char> IReadOnlySpanProvider<char>.GetReadOnlySpan() => AsSpan();

    ReadOnlyMemory<char> IReadOnlyMemoryProvider<char>.GetReadOnlyMemory() => AsMemory();

    public MemoryEnumerator<char> GetEnumerator() => new(ref GetReference(), Length);

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
    public bool Equals(LazyString? other) => Length == other?.Length && AsSpan().SequenceEqual(other.AsSpan());

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is LazyString other && Equals(other);

    [Pure]
    public override int GetHashCode() => string.GetHashCode(AsSpan());

    [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = $"named alternate is {nameof(AsSpan)}")]
    public static implicit operator ReadOnlySpan<char>(LazyString lazyString) => lazyString.AsSpan();

    public static bool operator ==(LazyString left, LazyString right) => Equals(left, right);

    public static bool operator !=(LazyString left, LazyString right) => !(left == right);
}
