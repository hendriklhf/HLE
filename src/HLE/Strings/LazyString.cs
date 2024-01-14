using System;
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
public sealed class LazyString : IDisposable, IEquatable<LazyString>, IReadOnlySpanProvider<char>, IReadOnlyMemoryProvider<char>, ICopyable<char>,
    IIndexAccessible<char>, ICollectionProvider<char>
{
    public ref readonly char this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return ref Unsafe.Add(ref GetReference(), index);
        }
    }

    char IIndexAccessible<char>.this[int index] => this[index];

    public ref readonly char this[Index index] => ref this[index.GetOffset(Length)];

    public ReadOnlySpan<char> this[Range range] => new Slicer<char>(ref GetReference(), Length).SliceReadOnlySpan(range);

    public int Length { get; }

    int ICountable.Count => Length;

    private RentedArray<char> _chars;
    private string? _string;

    public static LazyString Empty { get; } = new();

    private LazyString()
    {
        _chars = [];
        _string = string.Empty;
    }

    [MustDisposeResource]
    public LazyString(ReadOnlySpan<char> chars)
    {
        if (chars.Length == 0)
        {
            return;
        }

        RentedArray<char> buffer = ArrayPool<char>.Shared.RentAsRentedArray(chars.Length);
        CopyWorker<char>.Copy(chars, buffer.AsSpan());
        _chars = buffer;
        Length = chars.Length;
    }

    [MustDisposeResource]
    public LazyString([HandlesResourceDisposal] RentedArray<char> chars, int length)
    {
        _chars = chars;
        Length = length;
    }

    public void Dispose() => _chars.Dispose();

    [Pure]
    internal ref char GetReference() => ref _string is null ? ref _chars.Reference : ref StringMarshal.GetReference(_string);

    [Pure]
    public ReadOnlySpan<char> AsSpan()
    {
        if (Length == 0)
        {
            return [];
        }

#pragma warning disable RCS1084 // use "??", which can't be used here
        return _string is null ? _chars.AsSpan(..Length) : _string;
#pragma warning restore RCS1084
    }

    [Pure]
    public ReadOnlyMemory<char> AsMemory()
    {
        if (Length == 0)
        {
            return ReadOnlyMemory<char>.Empty;
        }

        return _string?.AsMemory() ?? _chars.AsMemory(..Length);
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
        CopyWorker<char>.Copy(ref chars, ref MemoryMarshal.GetArrayDataReference(result), (uint)Length);
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
    public override string ToString() => ToString(0);

    [Pure]
    public string ToString(int maximumPoolingLength)
    {
        if (_string is not null)
        {
            return _string;
        }

        if (Length == 0)
        {
            return string.Empty;
        }

        ReadOnlySpan<char> chars = _chars.AsSpan(..Length);
        string str = Length <= maximumPoolingLength ? StringPool.Shared.GetOrAdd(chars) : new(chars);
        _chars.Dispose();
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

    ReadOnlySpan<char> IReadOnlySpanProvider<char>.GetReadOnlySpan() => AsSpan();

    ReadOnlyMemory<char> IReadOnlyMemoryProvider<char>.GetReadOnlyMemory() => AsMemory();

    public MemoryEnumerator<char> GetEnumerator() => new(ref GetReference(), Length);

    [Pure]
    public bool Equals(LazyString? other) => Length == other?.Length && AsSpan().SequenceEqual(other.AsSpan());

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is LazyString other && Equals(other);

    [Pure]
    public override int GetHashCode() => string.GetHashCode(AsSpan());

    [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = $"named alternate is {nameof(AsSpan)}")]
    public static implicit operator ReadOnlySpan<char>(LazyString lazyString) => lazyString.AsSpan();

    public static implicit operator string(LazyString lazyString) => lazyString.ToString();

    public static bool operator ==(LazyString left, LazyString right) => Equals(left, right);

    public static bool operator !=(LazyString left, LazyString right) => !(left == right);
}
