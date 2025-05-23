using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Text;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "analyzer is wrong")]
[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "analyzer is wrong")]
[SuppressMessage("Minor Code Smell", "S3398:\"private\" methods called only by inner classes should be moved to those classes", Justification = "analyzer is stupid")]
public static class DefaultInterpolatedStringHandlerExtensions
{
    extension(DefaultInterpolatedStringHandler handler)
    {
        public ReadOnlySpan<char> Text => GetText(handler);

        public void Clear() => ClearCore(handler);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Text")]
    private static extern ReadOnlySpan<char> GetText(DefaultInterpolatedStringHandler h);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Clear")]
    private static extern void ClearCore(DefaultInterpolatedStringHandler h);
}
