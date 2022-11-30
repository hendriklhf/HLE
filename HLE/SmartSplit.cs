using System;

namespace HLE;

public sealed class SmartSplit
{
    public string this[int idx] => _splits[idx] ??= new(((ReadOnlySpan<char>)_string)[_ranges[idx]]);

    private readonly Range[] _ranges;
    private readonly string?[] _splits;
    private readonly string _string;

    public SmartSplit(string? str, char separator = ' ')
    {
        _string = str ?? string.Empty;
        ReadOnlySpan<char> span = _string;
        _ranges = span.GetRangesOfSplit(separator);
        _splits = new string?[_ranges.Length];
    }

    public SmartSplit(string? str, ReadOnlySpan<char> separator)
    {
        _string = str ?? string.Empty;
        ReadOnlySpan<char> span = _string;
        _ranges = span.GetRangesOfSplit(separator);
        _splits = new string?[_ranges.Length];
    }
}
