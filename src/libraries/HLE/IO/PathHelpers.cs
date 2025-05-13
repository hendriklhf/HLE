using System;
using System.Diagnostics.Contracts;
using System.IO;
using HLE.Text;

namespace HLE.IO;

public static class PathHelpers
{
    private static readonly TypeFormatter s_typeToPathFormatter = new(new()
    {
        NamespaceSeparator = Path.DirectorySeparatorChar,
        GenericTypesSeparator = ", ",
        DimensionSeparator = ",",
        GenericDelimiters = new("{", "}")
    });

    [Pure]
    public static string TypeNameToPath<T>()
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
        => TypeNameToPath(typeof(T));

    [Pure]
    public static string TypeNameToPath(Type type) => s_typeToPathFormatter.Format(type);
}
