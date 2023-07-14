using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

internal sealed class RegexArray : ICollection<Regex>, IReadOnlyCollection<Regex>, ICopyable<Regex>, ICountable, IIndexAccessible<Regex>, IEquatable<RegexArray>
{
    public Regex this[int index]
    {
        get => _regexes[index];
        set => SetRegex(index, value);
    }

    public Regex this[Index index]
    {
        get => _regexes[index];
        set => SetRegex(index.GetOffset(Length), value);
    }

    public ReadOnlySpan<Regex> this[Range range] => _regexes.AsSpan(range);

    public int Length => _regexes.Length;

    int ICountable.Count => Length;

    int ICollection<Regex>.Count => Length;

    int IReadOnlyCollection<Regex>.Count => Length;

    bool ICollection<Regex>.IsReadOnly => false;

    private readonly Regex[] _regexes;
    private readonly StringArray _patterns;
    private readonly int[] _options;
    private readonly TimeSpan[] _timeouts;

    public RegexArray(int length)
    {
        _regexes = new Regex[length];
        _patterns = new(length);
        _options = new int[length];
        _timeouts = new TimeSpan[length];
    }

    public ReadOnlySpan<Regex> AsSpan() => _regexes;

    public int IndexOf(ReadOnlySpan<char> pattern, int startIndex = 0) => _patterns.IndexOf(pattern, startIndex);

    public bool Contains(ReadOnlySpan<char> pattern) => IndexOf(pattern) >= 0;

    private void SetRegex(int index, Regex regex)
    {
        _regexes[index] = regex;
        _patterns[index] = regex.ToString();
        _options[index] = (int)regex.Options;
        _timeouts[index] = regex.MatchTimeout;
    }

    public void CopyTo(List<Regex> destination, int offset = 0)
    {
        DefaultCopier<Regex> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(Regex[] destination, int offset)
    {
        DefaultCopier<Regex> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<Regex> destination)
    {
        DefaultCopier<Regex> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public void CopyTo(Span<Regex> destination)
    {
        DefaultCopier<Regex> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public void CopyTo(ref Regex destination)
    {
        DefaultCopier<Regex> copier = new(AsSpan());
        copier.CopyTo(ref destination);
    }

    public unsafe void CopyTo(Regex* destination)
    {
        DefaultCopier<Regex> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    void ICollection<Regex>.Add(Regex item) => throw new NotSupportedException();

    void ICollection<Regex>.Clear()
    {
        _regexes.AsSpan().Clear();
        _patterns.Clear();
        _options.AsSpan().Clear();
        _timeouts.AsSpan().Clear();
    }

    bool ICollection<Regex>.Contains(Regex item) => throw new NotSupportedException();

    bool ICollection<Regex>.Remove(Regex item) => throw new NotSupportedException();

    public IEnumerator<Regex> GetEnumerator()
    {
        foreach (Regex regex in _regexes)
        {
            yield return regex;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Equals(RegexArray? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is RegexArray other && Equals(other);
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    public static bool operator ==(RegexArray? left, RegexArray? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RegexArray? left, RegexArray? right)
    {
        return !(left == right);
    }
}
