using System;
using System.Collections.Generic;
using HLE.Memory;

namespace HLE.Text;

public static class StringExtensions
{
    public static void CopyTo(this string? str, List<char> destination, int offset = 0)
    {
        ReadOnlySpan<char> span = str;
        if (span.Length == 0)
        {
            return;
        }

        CopyWorker<char> copyWorker = new(span);
        copyWorker.CopyTo(destination, offset);
    }

    public static void CopyTo(this string? str, char[] destination, int offset = 0)
    {
        ReadOnlySpan<char> span = str;
        if (span.Length == 0)
        {
            return;
        }

        CopyWorker<char> copyWorker = new(span);
        copyWorker.CopyTo(destination, offset);
    }

    public static void CopyTo(this string? str, Memory<char> destination)
    {
        ReadOnlySpan<char> span = str;
        if (span.Length == 0)
        {
            return;
        }

        CopyWorker<char> copyWorker = new(span);
        copyWorker.CopyTo(destination);
    }

    public static void CopyTo(this string? str, ref char destination)
    {
        ReadOnlySpan<char> span = str;
        if (span.Length == 0)
        {
            return;
        }

        SpanHelpers.Copy(span, ref destination);
    }

    public static unsafe void CopyTo(this string? str, char* destination)
    {
        ReadOnlySpan<char> span = str;
        if (span.Length == 0)
        {
            return;
        }

        SpanHelpers.Copy(span, destination);
    }
}
